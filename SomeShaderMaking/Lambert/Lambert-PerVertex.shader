// 兰伯特光照模型
Shader "C3/Lambert-PerVertex"
{
    // 逐顶点光照着色
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
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                // 只要后面的槽位用到了COLOR，颜色混合时片段就一定会插值，如果没用COLOR就不会插值
                float3 color : COLOR; // 需要传递颜色，用于后续插值
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                
                // 将物体空间的法线转换为世界空间的法线
                float3 worldNormal = normalize(UnityObjectToWorldNormal(v.normal));
                // 计算光照方向
                float3 worldLight = normalize(_WorldSpaceLightPos0.xyz);
                
                // 漫反射强度：diffuse = 光照颜色 * 材质颜色 * cos
                float3 diffuse = _LightColor0.rgb * max(0, dot(worldNormal, worldLight));
                
                o.color = diffuse;
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                UNITY_APPLY_FOG(i.fogCoord, col);
                col.rgb *= i.color;
                return col;
            }
            ENDCG
        }
    }
}
