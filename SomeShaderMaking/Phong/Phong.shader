Shader "C3/Phong"
{
    // phong模型
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
                float3 normal : NORMAL; // 依旧需要法线
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float4 worldPos : TEXCOORD2;  // 需要片段的世界坐标，用于计算视线方向
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
                //fixed4 col = tex2D(_MainTex, i.uv);
                
                // Phong模型需要计算法线方向、视线方向、光照方向
                float3 worldNormal = i.worldNormal;  // 法线方向
                // _WorldSpaceLightPos0获取主光源的方向（如果是方向光）或位置（如果是点光源）
                float3 worldLight = normalize(_WorldSpaceLightPos0.xyz);  // 光照方向
                // 位置和方向是三维的，xyz已经足以保存信息
                float3 worldView = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
                // reflect方法可以给定入射方向计算出反射方向（原来worldLight从表面指向光源）
                float3 worldReflect = normalize(reflect(-worldLight, worldNormal));
                
                // 漫反射颜色设置为纹理颜色
                float4 diffuseColor = tex2D(_MainTex, i.uv);
                // 镜面反射颜色（强度）设置为白色
                float4 specularColor = _SpecularColor; /*float4(1, 1, 1, 1);*/
                // 反光度
                float shininess = _Shininess; /*32;*/
                
                // 1、环境光照（常量环境因子，可以从Lighting.cginc直接获取）（常量环境因子要乘以材质颜色）
                float3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * diffuseColor.rgb;
                
                // 2、漫反射（和兰伯特一样,_LightColor0表示主光源的颜色和强度）（已经考虑到环境光照用标准兰伯特就好）
                float3 diffuse = _LightColor0.rgb * diffuseColor.rgb * max(0, dot(worldNormal, worldLight));
                
                // 3、镜面反射
                float3 specular = _LightColor0.rgb * specularColor * pow(max(0, dot(worldReflect, worldView)), shininess);
                
                // col.rgb和col.xyz完全一样
                float4 col;
                col.rgb = ambient + diffuse + specular;
                
                return col;
                //return fixed4(UNITY_LIGHTMODEL_AMBIENT.xyz, 1);
            }
            ENDCG
        }
    }
}
