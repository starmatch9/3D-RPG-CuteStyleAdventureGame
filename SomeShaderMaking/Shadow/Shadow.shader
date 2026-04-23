Shader "C3/Shadow"
{
    // 制作阴影的前提：
    // 在Unity内置渲染管线中，Unity会先生成ShadowMap（跑ShadowCaster通道）
    // 再通过ForwardBase和ForwardAdd渲染物体（此时才能拿到阴影信息）
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        [Header(Phong)]
        _SpecularColor ("高光颜色", Color) = (1, 1, 1, 1)
        _Shininess ("反光度", Range(1, 648)) = 32
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100
        // 一个Pass就是也给绘制流程，一个drawcall

        // ----基础Pass：处理主要光源的投影----
        
        // 没有主光源也会走BasePass这个通道
        // 这个通道的执行不依赖主光源是否存在，只要物体被渲染，BasePass就一定会执行
        // 所以说环境光、纹理采样啥的依旧进行
        // 只不过_WorldSpaceLightPos0 和 _LightColor0 可能是默认值或零向量
        
        Pass
        {
            // 主光源处理的标签：ForwardBase
            Tags
            {
                "LightMode" = "ForwardBase"
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // 多重编译
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc" //自动处理采样和衰减

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };


            // TRANSFER_SHADOW(o)在展开后成为o._ShadowCoord = ComputeScreenPos(o.pos);
            // 所以它硬性要求v2f中必须有一个名为pos的裁剪空间位置成员
            // 所以裁剪空间位置成员的名字必须叫做pos
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION; //只能叫pos这个名字
                float3 worldNormal : TEXCOORD1;
                float4 worldPos : TEXCOORD2;

                // 保存阴影贴图坐标的，在AutoLight里（即从光源视角看过去的屏幕坐标）
                SHADOW_COORDS(3) // 占用 TEXCOORD3
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _SpecularColor;
            float _Shininess;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = normalize(UnityObjectToWorldNormal(v.normal));
                o.worldPos = mul(unity_ObjectToWorld, v.vertex); // 乘以模型矩阵变换到时间空间中

                // 等价于o._ShadowCoord = ComputeScreenPos(o.pos);
                // 相当于为v2f中的阴影贴图坐标赋值
                TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 worldNormal = i.worldNormal;
                float3 worldLight = normalize(_WorldSpaceLightPos0.xyz);
                float3 worldView = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
                float3 worldHalf = normalize(worldLight + worldView);

                // 采样材质本身颜色
                float4 diffuseColor = tex2D(_MainTex, i.uv);

                // 光照计算（Blinn-Phong三大分量）
                float3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * diffuseColor.rgb;
                float3 diffuse = _LightColor0.rgb * diffuseColor.rgb * max(0, dot(worldNormal, worldLight));
                float3 specular = _LightColor0.rgb * _SpecularColor.rgb * pow(
                    max(0, dot(worldHalf, worldNormal)), _Shininess);

                // 阴影衰减，计算出当前光源的衰减值，赋值给第一个参数
                // atten为输出变量名
                // i为v2f结构体用来取阴影坐标
                // i.worldPos.xyz取世界坐标（主要用于取点光源的距离衰减）
                // （光源的位置/方向已经在 _WorldSpaceLightPos0 里了，宏内部会直接用）
                UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos.xyz)

                // 点光源（顶点光照，旧版API）
                // 这一步属于额外补光，用于计算点光源，逐顶点计算，性能较ForwardAdd高
                // 所以只有BasePass没有AddPass时可以选择使用这个额外补光
                // float3 Shade4PointLights(
                //     float4 lightPosX,      // 4个光源的X坐标（打包成float4）
                //     float4 lightPosY,      // 4个光源的Y坐标
                //     float4 lightPosZ,      // 4个光源的Z坐标
                //     float3 lightColor0,    // 光源0的颜色
                //     float3 lightColor1,    // 光源1的颜色
                //     float3 lightColor2,    // 光源2的颜色
                //     float3 lightColor3,    // 光源3的颜色
                //     float4 lightAttenSq,   // 衰减系数的倒数
                //     float3 worldPos,       // 当前顶点/像素的世界坐标
                //     float3 worldNormal     // 世界空间法线
                // );
                // 以下都是获取场景中各个点光源的属性，但是参数毕竟是我们自己传的
                // 我们也可以选择手动补光，传自定义参数
                // unity_4LightPosX0    // 前 4 个点光源的 X 坐标
                // unity_4LightPosY0    // 前 4 个点光源的 Y 坐标
                // unity_4LightPosZ0    // 前 4 个点光源的 Z 坐标
                // unity_LightColor[0]  // 前 4 个点光源的颜色
                // unity_4LightAtten0   // 前 4 个点光源的衰减系数
                
                /*float3 pointLights = Shade4PointLights(
                    unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                    unity_LightColor[0].rgb, unity_LightColor[1].rgb,
                    unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                    unity_4LightAtten0, i.worldPos.xyz, worldNormal
                ) * diffuseColor.rgb;*/
                
                // 也就是说有了ForwardAdd通道就不需要Shade4PointLights了，否则会光源过亮
                // 感觉还是删了比较好，没啥用，不简洁
                
                // 合成：阴影只影响漫反射和高光，不影响环境光和点光源
                float4 col;
                // col.rgb = ambient + pointLights + (diffuse + specular) * atten;
                col.rgb = ambient + (diffuse + specular) * atten;
                col.a = 1;

                return col;
            }
            ENDCG
        }

        // ----额外Pass：处理其他光源的投影----
        Pass
        {
            Tags
            {
                "LightMode" = "ForwardAdd"
            }
            Blend One One  // 表示加法混合：最终颜色 += 当前光源贡献

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdadd_fullshadows

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float2 uv : TEXCOORD4;
                
                // 相当于把阴影坐标和衰减坐标分别放在纹理2（槽位）和纹理3（槽位）中
                // 之之前的SHADOW_COORDS只声明阴影坐标，这个同时声明阴影和衰减坐标（点光源的距离信息的，一个光源一套）
                LIGHTING_COORDS(2, 3)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _SpecularColor;
            float _Shininess;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = normalize(UnityObjectToWorldNormal(v.normal));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // 把顶点位置转换成光源视角下的坐标
                // 然后存好阴影和衰减坐标
                TRANSFER_VERTEX_TO_FRAGMENT(o);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 N = normalize(i.worldNormal);
                // 自动适配点光源/聚光/方向光
                float3 L = normalize(UnityWorldSpaceLightDir(i.worldPos));
                float3 V = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
                float3 H = normalize(L + V);
                
                // 采样材质本身颜色
                float4 diffuseColor = tex2D(_MainTex, i.uv);

                float3 diffuse = _LightColor0.rgb * max(0, dot(N, L)) * diffuseColor.rgb;
                float3 specular = _LightColor0.rgb * _SpecularColor.rgb *
                    pow(max(0, dot(N, H)), _Shininess);

                UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);

                // 环境分量不用多次计算，Base里算一次就行
                return fixed4((diffuse + specular) * atten, 1);
            }
            ENDCG
        }
    }
    // Fallback "Diffuse"本质就是替我们补上了缺失的ShadowCasterPass
    // 我们也可以自己写一个ShaderCasterPass，毕竟是学习
    // Fallback能够借用Diffuse Shader中现成的能力
    // 是一种兜底工具，当我们自己的shader缺少某些功能/Pass/不支持某平台时
    // Unity会用Fallback指定的Shader中的内容补上
    // 类似的工具还有一个 “UsePass”
    Fallback "Diffuse"
}