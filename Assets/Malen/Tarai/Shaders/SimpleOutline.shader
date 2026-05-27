Shader "Custom/Sprite/SimpleOutline"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}

        _Color("Tint", Color) = (1, 1, 1, 1)

        _OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineThickness("Outline Thickness", Float) = 2.0
        _OutlineResolution("Outline Resolution", Vector) = (1024, 1024, 0, 0)
        _OutlineAntiAlias("Outline Anti Alias", Float) = 1.0
        _AlphaThreshold("Alpha Threshold", Range(0, 1)) = 0.1

        [MaterialToggle] PixelSnap("Pixel Snap", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ PIXELSNAP_ON

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            fixed4 _Color;

            fixed4 _OutlineColor;
            float _OutlineThickness;
            float4 _OutlineResolution;
            float _OutlineAntiAlias;
            float _AlphaThreshold;

            struct Attributes
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;

                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
                output.color = input.color * _Color;

                #ifdef PIXELSNAP_ON
                output.vertex = UnityPixelSnap(output.vertex);
                #endif

                return output;
            }

            float GetAlpha(float2 uv)
            {
                return tex2D(_MainTex, uv).a;
            }

            float GetOutlineMask(float2 uv, float2 texelSize)
            {
                float centerAlpha = GetAlpha(uv);

                if (centerAlpha > _AlphaThreshold)
                {
                    return 0.0;
                }

                float maxAlpha = 0.0;

                maxAlpha = max(maxAlpha, GetAlpha(uv + float2(texelSize.x, 0.0)));
                maxAlpha = max(maxAlpha, GetAlpha(uv + float2(-texelSize.x, 0.0)));
                maxAlpha = max(maxAlpha, GetAlpha(uv + float2(0.0, texelSize.y)));
                maxAlpha = max(maxAlpha, GetAlpha(uv + float2(0.0, -texelSize.y)));

                maxAlpha = max(maxAlpha, GetAlpha(uv + float2(texelSize.x, texelSize.y)));
                maxAlpha = max(maxAlpha, GetAlpha(uv + float2(-texelSize.x, texelSize.y)));
                maxAlpha = max(maxAlpha, GetAlpha(uv + float2(texelSize.x, -texelSize.y)));
                maxAlpha = max(maxAlpha, GetAlpha(uv + float2(-texelSize.x, -texelSize.y)));

                float antiAlias = max(_OutlineAntiAlias, 0.0001);
                float outlineMask = smoothstep(_AlphaThreshold, _AlphaThreshold + antiAlias * 0.1, maxAlpha);

                return outlineMask;
            }

            fixed4 Frag(Varyings input) : SV_Target
            {
                fixed4 spriteColor = tex2D(_MainTex, input.uv) * input.color;

                float2 resolution = _OutlineResolution.xy;

                if (resolution.x <= 0.0 || resolution.y <= 0.0)
                {
                    resolution = float2(
                        1.0 / _MainTex_TexelSize.x,
                        1.0 / _MainTex_TexelSize.y
                    );
                }

                float thickness = max(_OutlineThickness, 0.0);
                float2 texelSize = thickness / resolution;

                float outlineMask = GetOutlineMask(input.uv, texelSize);

                fixed4 outlineColor = _OutlineColor;
                outlineColor.a *= outlineMask;

                fixed4 result = spriteColor;

                if (spriteColor.a <= _AlphaThreshold)
                {
                    result = outlineColor;
                }

                result.rgb *= result.a;

                return result;
            }

            ENDCG
        }
    }

    Fallback "Sprites/Default"
}