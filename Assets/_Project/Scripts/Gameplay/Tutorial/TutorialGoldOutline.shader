Shader "JogoBruxinha/Tutorial Gold Outline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _OutlineColor ("Yellow", Color) = (1, 0.92, 0.12, 1)
        _Fade ("Visibility", Float) = 1
        _OutlinePixels ("Width in texture pixels", Float) = 1.5
        _UVRect ("Sprite UV bounds", Vector) = (0, 0, 1, 1)
        _SpriteFlip ("Sprite flip", Vector) = (1, 1, 1, 1)
        _Solid ("Solid area line", Float) = 0
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "CanUseSpriteAtlas"="True" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        ColorMask [_ColorMask]
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            float4 _OutlineColor;
            float4 _UVRect;
            float4 _SpriteFlip;
            float _OutlinePixels;
            float _Solid;
            float _Fade;
            float4 _ClipRect;
            struct Attributes { float3 positionOS : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; float2 localPosition : TEXCOORD1; };
            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS * _SpriteFlip.xyz);
                o.uv = v.uv;
                o.color = v.color;
                o.localPosition = v.positionOS.xy;
                return o;
            }
            float AlphaAt(float2 uv)
            {
                if (any(uv < _UVRect.xy) || any(uv > _UVRect.zw)) return 0;
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a;
            }
            half4 frag(Varyings v) : SV_Target
            {
                float opacity = _OutlineColor.a * v.color.a * _Fade;
                #ifdef UNITY_UI_CLIP_RECT
                float2 inside = step(_ClipRect.xy, v.localPosition) * step(v.localPosition, _ClipRect.zw);
                opacity *= inside.x * inside.y;
                #endif
                if (_Solid > 0.5) return half4(_OutlineColor.rgb, opacity);
                float2 d = _MainTex_TexelSize.xy * _OutlinePixels;
                float edge = min(min(AlphaAt(v.uv + float2(d.x, 0)), AlphaAt(v.uv - float2(d.x, 0))),
                                 min(AlphaAt(v.uv + float2(0, d.y)), AlphaAt(v.uv - float2(0, d.y))));
                edge = min(edge, min(min(AlphaAt(v.uv + d), AlphaAt(v.uv - d)),
                                     min(AlphaAt(v.uv + float2(d.x, -d.y)), AlphaAt(v.uv + float2(-d.x, d.y)))));
                return half4(_OutlineColor.rgb, opacity * AlphaAt(v.uv) * (1 - edge));
            }
            ENDHLSL
        }
    }
}
