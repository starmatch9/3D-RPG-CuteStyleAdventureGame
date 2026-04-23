Shader "C3/Blinn-Phong"
{
    // Blinn-phong模型：把原来的cos算成半程向量和法线的夹角
    // 环境光 + 漫反射 + 镜面反射
    
    // 1、环境光照：一个很小的常量环境因子
    // 2、漫反射：光照颜色 * 材质颜色 * 夹角余弦
    // 3、镜面反射：光照颜色 * 镜面反射颜色（强度） * 高光系数（夹角余弦 ^ 反光度）
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        [Header(Phong)]
        _SpecularColor ("高光颜色", Color) = (1, 1, 1, 1)
        _Shininess ("反光度", Range(1, 648)) = 32
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

            // 有Lighting.cginc后面才能用_LightColor0
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float4 worldPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float4 _SpecularColor;
            float _Shininess;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = normalize(UnityObjectToWorldNormal(v.normal));
                o.worldPos = mul(unity_ObjectToWorld, v.vertex); // 乘以模型矩阵变换到时间空间中
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 worldNormal = i.worldNormal;  // 法线方向
                float3 worldLight = normalize(_WorldSpaceLightPos0.xyz);  // 光照方向
                float3 worldView = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
                
                // 算半程向量即可
                float3 worldHalf = normalize(worldLight + worldView);
                
                // 漫反射颜色设置为纹理颜色
                float4 diffuseColor = tex2D(_MainTex, i.uv);
                // 镜面反射颜色（强度）设置为白色
                float4 specularColor = _SpecularColor;
                // 反光度
                float shininess = _Shininess;
                
                // 1、环境光照
                float3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * diffuseColor.rgb;
                
                // 2、漫反射
                float3 diffuse = _LightColor0.rgb * diffuseColor.rgb * max(0, dot(worldNormal, worldLight));
                
                // 3、镜面反射（使用半程向量）
                float3 specular = _LightColor0.rgb * specularColor * pow(max(0, dot(worldHalf, worldNormal)), shininess);
                
                float4 col;
                col.rgb = ambient + diffuse + specular;
                
                return col;
            }
            ENDCG
        }
    }
}
