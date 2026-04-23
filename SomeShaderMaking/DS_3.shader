// 用噪声纹理滚动采样得到扰动值，用扰动值来偏移主纹理的uv坐标，达到水波纹的效果
Shader "C1/DS_3"
{
    Properties
    {   
        // 主纹理
        _MainTex ("Texture", 2D) = "white" {}
        // 噪声纹理（每个纹素都是随机的灰度值）
        _SoundTex ("Sound", 2D) = "white" {}

        // 扰动强度：后面会乘以噪声纹素的值（0-1）来得到最终的扰动值，有点像振幅
        _DistortAmount("Distort Amount", Range(0, 2)) = 0.5
        // 扰动纹理在X方向上的滚动速度
        _DistortTexXSpeed("Scroll speed X", Range(-50, 50)) = 5
        // 扰动纹理在Y方向上的滚动速度
        _DistortTexYSpeed("Scroll speed Y", Range(-50, 50)) = 5
        // 每个实例的随机种子，用来让每个实例的扰动效果不完全一样
        [HideInInspector] _Randomseed("Random Seed", Range(0, 10000)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent"
                "Queue"="Transparent"}
        // 设置透明度混合的方式：源alpha * 颜色 + （1-源alpha） * 背景颜色
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            // 定义顶点着色器和片段着色器的入口函数
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            // 提供Unity的工具函数或工具宏
            #include "UnityCG.cginc"
            // 定义一个顶点着色器的输入结构
            struct appdata
            {
                // 模型文件中若每个纹理坐标通道都没有数据，会生成默认值
                // 一个模型可能有多套纹理坐标，默认uv是主纹理的坐标，对应TEXCOORD0
                float4 vertex : POSITION;   // 顶点位置
                float2 uv : TEXCOORD0;   // 第0套UV坐标
            };
            //定义一个从顶点着色器输出给片元着色器数据的结构
            struct v2f
            {
                float2 uv : TEXCOORD0;  // 主纹理的uv坐标
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION; // 顶点位置（裁剪空间）
                // 添加噪声的uv坐标
                half2 soundUV : TEXCOORD2; // 第2套UV坐标
            };

            // 注意：要让着色器语言和属性面板关联起来，变量名必须一致
            sampler2D _MainTex; // 主采样器
            float4 _MainTex_ST;  // 平铺与偏移参数
            // 噪声图的纹理采样器
            sampler2D _SoundTex; // 噪声图采样器
            float4 _SoundTex_ST;  // 噪声图的平铺与偏移参数
            half _DistortAmount; // 扰动强度
            half _DistortTexXSpeed; // 扰动纹理在X方向上的滚动速度
            half _DistortTexYSpeed; // 扰动纹理在Y方向上的滚动速度
            // 每个实例的随机种子
            // 其实就是float _Randomseed;，用这个宏是可以支持每个物体实例不同的随机种子
            UNITY_DEFINE_INSTANCED_PROP(float, _Randomseed)
            // UNITY_INSTANCING_BUFFER_START(Props) //GPT说得这样
            // UNITY_DEFINE_INSTANCED_PROP(float, _Randomseed)
            // UNITY_INSTANCING_BUFFER_END(Props)
            // 顶点着色器函数
            v2f vert (appdata v)
            {
                v2f o;  // 定义一个输出变量
                o.vertex = UnityObjectToClipPos(v.vertex);  // 将顶点位置从对象空间转换到裁剪空间
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);  // 根据_MainTex的平铺与偏移参数计算主纹理的uv坐标
                o.soundUV = TRANSFORM_TEX(v.uv, _SoundTex);  // 根据_SoundTex的平铺与偏移参数计算噪声纹理的uv坐标
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            //片段着色器函数
            // 返回几个分量就用几个分量的返回类型，fixed4、float都可以
            fixed4 frag (v2f i) : SV_Target  //声明最终结果输出到默认缓冲目标（SV_Target就是默认帧缓冲区（屏幕））
            {
                // 读取自己实例的随机种子
                half randomSeed = UNITY_ACCESS_INSTANCED_PROP(Props, _Randomseed);
                // 让噪声图的uv在时间的基础上滚动，达到动态扰动的效果
                // 默认是朝右上滚动
                // %1表示保持在0-1之间循环滚动，_Time.x是游戏运行的时间，乘以滚动速度就得到了滚动的距离 
                // 这两个也是完全连续的，从1跳到0时会跳一下
                i.soundUV.x += ((_Time.x + randomSeed)*_DistortTexXSpeed)%1;
                i.soundUV.y += ((_Time.y + randomSeed)*_DistortTexYSpeed)%1;
                // tex2D函数根据uv坐标从纹理中采样颜色值，灰度图rgb都一样
                // -0.5让范围变成-0.5到0.5，偏移有正有负
                // tex2D双线性插值最终d的变换是平滑连续的
                // d = d(x, y)就是一个连续函数 所以最终的扰动效果也是连续平滑的
                // 所以说不同的扰动图效果不一样，和扰动图变换是否剧烈有关：
                // 噪声图：最终效果十分剧烈，有颗粒感
                // 波纹图：最终效果比较柔和，有水波纹的感觉
                // 云纹图：有点像空气柔和扭曲
                // 纯色图：没有扭曲，只有整体的偏移，类似于滚动纹理的效果
                half d = (tex2D(_SoundTex, i.soundUV).r-0.5)*0.2* _DistortAmount;
                // 用扰动值修改主纹理的uv
                i.uv.x += d;
                i.uv.y += d;
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
