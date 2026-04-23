// 这句话用来定义此文件在Unity中的路径，可以在材质中选择这个Shader
Shader "C1/DS_2"
{
    // 属性块：可以定义一些属性参数，这些参数可以在材质面板中进行调整
    Properties
    {   
        // {}中不能写任何东西，只是一个占位符，表示这个属性的默认值是一个纯白色的纹理
        _MainTex ("Texture", 2D) = "white" {}
    }
    // 子着色器块：可以包含一个或多个渲染通道，每个通道定义了如何渲染对象
    // SubShader可以有多个，Pass也可以有多个
    SubShader
    {
        // 标签影响的都是使用该材质的物体
        // 觉得该物体的渲染类型、渲染顺序等属性
        // Tags { "RenderType"="Opaque" }  //渲染属性为不透明 alpha不生效
        Tags { "RenderType"="Transparent"
                "Queue"="Transparent"}
        // 设置混合
        Blend SrcAlpha OneMinusSrcAlpha

        // 用来标定质量的值，此值高于全局LOD上限时，此SubShader将被忽略，使用下一个SubShader
        LOD 100

        Pass
        {
            // 标定着色器语言的范围，CG表示使用Cg/HLSL语言编写着色器
            // 在这个范围外的都是普通的ShaderLab语法
            CGPROGRAM
            #pragma vertex vert  // 指定顶点着色器函数的名称，这个函数负责处理每个顶点的数据
            #pragma fragment frag  // 指定片段着色器函数的名称，这个函数负责处理每个像素的数据
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"  // 包含Unity提供的CG库文件，这个文件中定义了许多常用的函数和宏，可以简化着色器的编写

            // 定义输入结构体appdata，包含顶点位置和纹理坐标
            struct appdata
            {
                // 从网格体模型导入的顶点位置数据获取
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            // v2f 表示 vertex to fragment，专用于顶点到片段着色器的数据结构
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex; // _MainTex表示一个2D纹理采样器变量，可以用来从_MainTex纹理中采样颜色值
            // _MainTex_ST表示_MainTex纹理的缩放和平移参数，通常由Unity自动生成并传递给着色器
            // 即便不在Shader中使用_MainTex_ST，Unity也会自动生成这个变量并传递给着色器，以便在需要进行纹理坐标变换时使用
            float4 _MainTex_ST; 

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // 将顶点位置从对象空间转换到裁剪空间
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex); // 将顶点的纹理坐标进行变换，适应_MainTex纹理的缩放和平移
                o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw; // 直接使用_MainTex_ST进行纹理坐标变换，等价于TRANSFORM_TEX宏的功能
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            // 片段着色器的输入参数类型必须与顶点着色器的输出参数类型一致
            fixed4 frag (v2f i) : SV_Target // SV_Target表示输出到默认的帧缓冲中
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);// 从_MainTex纹理中采样颜色值，使用i.uv作为纹理坐标
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
