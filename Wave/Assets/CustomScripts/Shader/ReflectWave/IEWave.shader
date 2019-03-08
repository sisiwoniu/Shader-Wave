//イメージエフェクトタイプの波
Shader "Custom/IEWave"
{
    Properties
    {	
		[PerRendererData]
        _MainTex ("Texture", 2D) = "white" {}
		_BumpValue("Bump Value", Range(0, 9999)) = 150
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

		Blend SrcAlpha OneMinusSrcAlpha
		
		Cull Back

		ZTest Always

		//GrabPass {
		//	"_Buffer"
		//}
		
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
				//float4 grabpos : TEXCOORD1;
            };

            sampler2D _MainTex;
			float _BumpValue;
			fixed2 c_uv;
			float4 _MainTex_TexelSize;
			//sampler2D _Buffer;
			//float2 c_uv;
			//float4 _Buffer_TexelSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
				//正しい同次座標を取得
				//o.grabpos = ComputeGrabScreenPos(o.vertex); 
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
				float2 shiftx = {_MainTex_TexelSize.x, 0};

				float2 shifty = {0, _MainTex_TexelSize.y};

				float3 texX = 2 * tex2D(_MainTex, i.uv.xy + shiftx) - 1;

				float3 texx = 2 * tex2D(_MainTex, i.uv.xy - shiftx) - 1;

				float3 texY = 2 * tex2D(_MainTex, i.uv.xy + shifty) - 1;

				float3 texy = 2 * tex2D(_MainTex, i.uv.xy - shifty) - 1;

				float3 du = {1, 0, texX.x - texx.x};

				float3 dv = {0, 1, texY.x - texy.x};

				float normal = normalize(cross(du, dv));

				float2 offset = normal * _BumpValue * _MainTex_TexelSize.xy;

				fixed4 col = tex2D(_MainTex, i.uv.xy + offset);

                return col;
            }
            ENDCG
        }
    }
}
