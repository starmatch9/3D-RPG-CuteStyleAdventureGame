Shader "C2/C2-2"
{
    // 制作水平线效果，无论怎么转，水平线上面的黑，下面的白
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 worldPosition : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPosition = mul(unity_ObjectToWorld, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 把模型空间的原点变换到世界空间坐标（水平线）（后续可以暴露参数）
                half4 horLine = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
                // 此片段世界空间坐标到中心世界坐标的距离（有正负之分）
                half yOffset = horLine.y - i.worldPosition.y;
                // 当yOffset在[0, 0.3]区间时，从0平滑过渡到1（这个函数的输出永远是0到1）
                // smoothstep也会钳制，超出[0, 0.3]的部分恒为0或1
                fixed4 col = smoothstep(0, 0.3, yOffset); /*yOffset;*/ 
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
