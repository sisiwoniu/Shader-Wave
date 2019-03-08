Shader "Custom/Reflect"
{
    Properties
    {	
		[Toggle]
		_OnlyReflect("Only Reflect", float) = 0
		_RefColAlpha("Ref Color Alpha", Range(0, 1)) = 1
        _MainTex("Texture", 2D) = "white" {}
		_RefTex("Ref", 2D) = "white" {}
		//法線マップ
		_BumpMap("NormalMap", 2D) = "bump" {}
		//歪み具合
		_BumpAmt("BumpAmt", Range(0, 9999)) = 0
		_WaveTex("Wave", 2D) = "gray" {}
		_ParallaxScale("Parallax Scale", float) = 1
		_NormalScaleFactor("Normal Scale Factor", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }

		ZWrite On

		Cull Back
		
		Blend SrcAlpha OneMinusSrcAlpha

		CGINCLUDE

		#include "UnityCG.cginc"

		#pragma shader_feature _ONLYREFLECT_ON

		struct appdata 
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2f 
		{
			float4 vertex : SV_POSITION;
			float4 ref : TEXCOORD1;
			float2 uv : TEXCOORD0;
		};

		sampler2D _MainTex;
		float4 _MainTex_ST;

		sampler2D _RefTex;
		float4 _RefTex_TexelSize;

		sampler2D _BumpMap;
		sampler2D _WaveTex;
		float4 _WaveTex_TexelSize;

		//ローカルからワールドに変換するための変換行列
		float4x4 _RefW;
		//ワールドからビューに変換し、ビューからスクリーン空間に変換するための変換行列
		float4x4 _RefVP;

		float _BumpAmt;
		float _ParallaxScale;
		float _NormalScaleFactor;

		fixed _RefColAlpha;

		v2f vert(appdata i) 
		{
			v2f o;

			o.vertex = UnityObjectToClipPos(i.vertex);

			o.uv = TRANSFORM_TEX(i.uv, _MainTex);

			//ローカル空間からワールドにし、ビューに変換し、プロジェクションに変換する
			//これだけやると結果が「同次座標」になります
			o.ref = mul(_RefVP, mul(_RefW, i.vertex));

			return o;
		}

		fixed4 frag(v2f i) : SV_Target 
		{
			#ifdef _ONLYREFLECT_ON
			//これだけで反射描画ができるわけ
			//[i.ref.xy / i.ref.w]は同次座標を通常座標に変換し、通常値範囲は「-1~1」になっている
			//これをUVに変換する必要があり、UVの範囲は「0~1」になるので「* 0.5 + 0.5」によって値を整える
			fixed4 refCol = tex2D(_RefTex, i.ref.xy / i.ref.w * 0.5 + 0.5) * _RefColAlpha;
			#else

			//法線情報を取得
			float2 normal = UnpackNormal(tex2D(_BumpMap, i.uv + _Time.x / 2)).rg;

			//波動方程式の解を_WaveTexで受け取り、波による歪み具合をnormalに加算
			//_WaveTexは波の高さなので、高さの変化量から法線を求める
			//基本オフセット単位は「_WaveTex」画像の単位pixelサイズ
			float2 shiftX = {_WaveTex_TexelSize.x, 0};

			float2 shiftZ = {0, _WaveTex_TexelSize.y};

			shiftX *= _ParallaxScale * _NormalScaleFactor;

			shiftZ *= _ParallaxScale * _NormalScaleFactor;

			//h(x + shiftX)
			float3 texX = 2 * tex2D(_WaveTex, float2(i.uv.xy + shiftX)) - 1;

			//h(x - shitfX);
			float3 texx = 2 * tex2D(_WaveTex, float2(i.uv.xy - shiftX)) - 1;

			//h(y + shiftZ);
			float3 texZ = 2 * tex2D(_WaveTex, float2(i.uv.xy + shiftZ)) - 1;

			//h(y - shiftZ);
			float3 texz = 2 * tex2D(_WaveTex, float2(i.uv.xy - shiftZ)) - 1;

			float3 du = {1, 0, _NormalScaleFactor * (texX.x - texx.x)};

			float3 dv = {0, 1, _NormalScaleFactor * (texZ.x - texz.x)};

			//ここで高さ（Y軸の更新分）を計算
			normal += normalize(cross(du, dv));

			//(通常の法線 + 波の法線)で、投影するテクスチャのオフセットを計算
			//「_BumpAmt」は揺れの細かいパラメーター 「_RefTex_TexelSize.xy」は揺れのpixel単位
			float2 offset = normal * _BumpAmt * _RefTex_TexelSize.xy;

			//プロジェクションの同次座標にオフセットを加算する
			i.ref.xy += offset;

			//同次座標から通常座標に変換してから、その座標を利用して色を取る
			fixed4 refCol = tex2D(_RefTex, i.ref.xy / i.ref.w * 0.5 + 0.5);

			refCol.a = 1 * _RefColAlpha;

			#endif
			
			return refCol;
		}

		ENDCG

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}
