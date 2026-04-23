Shader "C2/C2-3"
{
    // 此shader的效果：越接近屏幕中心越亮，越远离屏幕中心越暗
    // 游戏视图和场景视图的摄像机位置不一样，那么看到的结果就不一样
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
                // 把物体原点转换到裁剪空间（裁剪空间是透视投影之后，远的w大，近的w小哪个空间，还未进行透视除法）
                half4 pos = UnityObjectToClipPos(float4(0, 0, 0, 1));
                // 在裁剪空间中，x、y除以w后就是NDC中的屏幕坐标了
                // smoothstep(1, 0, abs(pos.x / pos.w))相当于 1 - smoothstep(0, 1, abs(pos.x / pos.w))
                // 在中点时就是1，越远离中点就越为0（x/w的区间也就是0~1）
                // 可以用勾股定理严格计算，但是直接相乘性价比高
                fixed4 col = smoothstep(1, 0, abs(pos.x / pos.w)) * smoothstep(1, 0, abs(pos.y / pos.w));
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
