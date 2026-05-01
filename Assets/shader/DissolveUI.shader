Shader "UI/DissolveUI"
{
    Properties
    {
        _MainTex      ("Sprite Texture", 2D)      = "white" {}
        _NoiseTex     ("Noise Texture",  2D)      = "white" {}
        _Threshold    ("Dissolve Threshold", Range(0,1)) = 0
        _EdgeWidth    ("Edge Width",  Range(0,0.15)) = 0.04
        _EdgeColor    ("Edge Color",  Color)      = (1,1,1,1)

        // Unity UI stencil / clipping boilerplate
        _Color          ("Tint",               Color)  = (1,1,1,1)
        _StencilComp    ("Stencil Comparison", Float)  = 8
        _Stencil        ("Stencil ID",         Float)  = 0
        _StencilOp      ("Stencil Operation",  Float)  = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask  ("Stencil Read Mask",  Float) = 255
        _ColorMask      ("Color Mask",         Float)  = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"           = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType"      = "Transparent"
            "PreviewType"     = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref       [_Stencil]
            Comp      [_StencilComp]
            Pass      [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull      Off
        Lighting  Off
        ZWrite    Off
        ZTest     [unity_GUIZTestMode]
        Blend     SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                float2 uv            : TEXCOORD0;
                float2 noiseUV       : TEXCOORD1;
                float4 worldPosition : TEXCOORD2;
                float4 color         : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4    _MainTex_ST;
            float4    _NoiseTex_ST;
            float4    _Color;
            float4    _ClipRect;
            float     _Threshold;
            float     _EdgeWidth;
            float4    _EdgeColor;

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPosition = v.vertex;
                o.vertex  = UnityObjectToClipPos(v.vertex);
                o.uv      = TRANSFORM_TEX(v.uv, _MainTex);
                o.noiseUV = TRANSFORM_TEX(v.uv, _NoiseTex);
                o.color   = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col   = tex2D(_MainTex, i.uv) * i.color;
                fixed  noise = tex2D(_NoiseTex, i.noiseUV).r;

                // UI rect clipping
                col.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);

                // 噪声值 >= Threshold 的像素完全不显示（新封面还未"溶入"）
                if (noise >= _Threshold) discard;

                // 边缘发光带
                if (noise >= _Threshold - _EdgeWidth)
                {
                    float t = 1.0 - ((_Threshold - noise) / _EdgeWidth);
                    col = lerp(col, _EdgeColor, t * _EdgeColor.a);
                }

                return col;
            }
            ENDCG
        }
    }
}
