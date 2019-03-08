Shader "Custom/RefWave"
{
    Properties
    {
        _InputTex("Input", 2D) = "black" {}
		_PrevTex("Prev", 2D) = "black" {}
		_Prev2Tex("Prev2", 2D) = "black" {}
		//拡散調整
		_RoundAdjuster("Adjuster", Range(0, 1)) = 0
		//拡散率
		_Stride("Stride", Range(0, 10)) = 0
		//衰减率
		_Atten("Attenuation", Range(0, 1)) = 1
		//波の微調整パラメーター
		_C("C", float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
		Cull Off

		ZWrite Off

		ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

			sampler2D _InputTex;
			sampler2D _PrevTex;
			sampler2D _Prev2Tex;
			float4 _PrevTex_TexelSize;
			float _Stride;
			float _RoundAdjuster;
			float _Atten;
			float _C;

            fixed4 frag(v2f i) : SV_Target
            {		
				//振動幅
				float2 stride = float2(_Stride, _Stride) * _PrevTex_TexelSize.xy;

				//波動方程式 h = 高さ、t = 時間
				//「h(t + 1) = 2h + c(h(x + 1) + h(x - 1) + h(y + 1) + h(y - 1) - 4h) - h(t - 1)」
				//これが高さ「h」に当たる
				half4 prev = (tex2D(_PrevTex, i.uv) * 2) - 1;

				half value = (prev.r * 2 - 
								(tex2D(_Prev2Tex, i.uv).r * 2 - 1) + (
								(tex2D(_PrevTex, half2(i.uv.x + stride.x, i.uv.y)).r * 2 - 1) + 
								(tex2D(_PrevTex, half2(i.uv.x - stride.x, i.uv.y)).r * 2 - 1) +
								(tex2D(_PrevTex, half2(i.uv.x, i.uv.y + stride.y)).r * 2 - 1) +
								(tex2D(_PrevTex, half2(i.uv.x, i.uv.y - stride.y)).r * 2 - 1) - 
								prev.r * 4) * 
								_C);

				float4 input = tex2D(_InputTex, i.uv);
				
				//現在の増加した波の高さ
				value += input.r;

				//衰减率
				value *= (1 - _Atten);

				value = (value + 1) * 0.5;

				value -= (_RoundAdjuster * 0.01);

                return fixed4(value, 0, 0, 1);
            }
            ENDCG
        }
    }
}
