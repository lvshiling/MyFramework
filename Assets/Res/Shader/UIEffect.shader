// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "UI/UIEffect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        _TonePower ("GrayPower", Range( 0, 1 )) = 1
        _PixelPower ("PixelPower", Range( 0, 1 )) = 1

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UIEffect"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP
            #pragma shader_feature __ TONE_GRAY
            #pragma shader_feature __ EFFECT_BLUR EFFECT_PIXEL

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            fixed4 Blur(sampler2D tex, half2 uv, half2 addUv, half bias)
            {
                return ( tex2D(tex, uv + half2(addUv.x, addUv.y)) + tex2D(tex, uv + half2(-addUv.x, addUv.y))
                       + tex2D(tex, uv + half2(addUv.x, -addUv.y)) + tex2D(tex, uv + half2(-addUv.x, -addUv.y)) ) * bias;
            }
            
            fixed4 Tex2DBlurring(sampler2D tex, half2 uv, half2 blur)
            {
                half4 color = tex2D(tex, uv);
                return color * 0.41511 + Blur( tex, uv, blur * 3, 0.12924 ) + Blur( tex, uv, blur * 5, 0.01343 ) 
                + Blur( tex, uv, blur * 6, 0.00353 );
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = v.texcoord;

                OUT.color = v.color * _Color;
                return OUT;
            }

            sampler2D _MainTex;
            half4 _MainTex_TexelSize;
            float _TonePower;
            float _PixelPower;

            fixed4 frag(v2f IN) : SV_Target
            {
                #ifdef EFFECT_BLUR
                    half4 color = (Tex2DBlurring(_MainTex, IN.texcoord, _PixelPower*2*_MainTex_TexelSize.xy) + _TextureSampleAdd) * IN.color;
                #elif EFFECT_PIXEL
                    float2 uv = IN.texcoord * _MainTex_TexelSize.zw;
                    uv = floor( uv / ( _PixelPower * 20 ) ) * _PixelPower * 20;
                    IN.texcoord = uv * _MainTex_TexelSize.xy;
                    half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                #else
                    half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                #endif

                #ifdef TONE_GRAY
                color.rgb = lerp( color.rgb, Luminance(color.rgb), _TonePower);
                #endif

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
        ENDCG
        }
    }
}
