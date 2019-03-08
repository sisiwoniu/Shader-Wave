Shader "Custom/InputWaveBuffer"
{
    Properties
    {
		_Dist("RDist", Range(0, 1)) = 0.001
	}
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

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

			float2 _uv;
			fixed _Dist;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed dist = distance(_uv, i.uv);

				fixed d = step(dist, _Dist);

				fixed4 col = {1, 0, 0, 1};

				col.r *= d;

                return col;
            }
            ENDCG
        }
    }
}
