// 半兰伯特光照模型
Shader "C3/C-Lambert"
{
    // 漫反射已经做的很好了
    // 就算再暗也要显示出一点纹理的亮度（加入环境光）
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
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL; // 光照模型需要法线
            };

            struct v2f
            {
                // 用到槽位的GPU才帮我们插值
                // TEXCOORD0 ~ TEXCOORD7 纹理坐标，会被插值
                // COLOR / COLOR0 ~ COLOR3 颜色，会被插值
                // SV_POSITION 是特例，不插值（存裁剪空间深度值，用于深度测试）
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.worldNormal = normalize(UnityObjectToWorldNormal(v.normal));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                UNITY_APPLY_FOG(i.fogCoord, col);
                
                float3 worldNormal = i.worldNormal;
                float3 worldLight = normalize(_WorldSpaceLightPos0.xyz);
                
                //标准半兰伯特公式：（就只有这个部分改变了）
                // diffuse = dot(N, L) * 0.5 + 0.5
                // 把[-1, 1]映射到[0, 1]，保证背光面不是全黑
                float3 diffuse = _LightColor0.rgb * max(0, dot(worldNormal, worldLight) * 0.5 + 0.5);
                col.rgb *= diffuse;
                
                return col;
            }
            ENDCG
        }
    }
}
