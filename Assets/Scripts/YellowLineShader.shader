Shader "Custom/FourCornersBlackEdgesRandomLines"
{
    Properties
    {
        _YellowThickness ("Yellow Segment Thickness", Float) = 0.05
        _BlackThickness ("Black Segment Thickness", Float) = 0.02
        _CornerLength ("Corner Length", Float) = 0.2
        _YellowColor ("Yellow Color", Color) = (1, 1, 0, 1)
        _BlackColor ("Black Color", Color) = (0, 0, 0, 1)
        _LineColor ("Line Color", Color) = (1, 0, 0, 1)
        _LineThickness ("Line Thickness", Float) = 0.01
        _LineDensity ("Line Density (Number of Lines)", Float) = 5.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float _YellowThickness;
            float _BlackThickness;
            float _CornerLength;
            float4 _YellowColor;
            float4 _BlackColor;

            float _LineThickness;
            float4 _LineColor;
            float _LineDensity;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                float isYellowHorizontal = ((uv.y <= _YellowThickness) * (uv.x <= _CornerLength)) +
                                           ((uv.y <= _YellowThickness) * (uv.x >= 1.0 - _CornerLength)) +
                                           ((uv.y >= 1.0 - _YellowThickness) * (uv.x <= _CornerLength)) +
                                           ((uv.y >= 1.0 - _YellowThickness) * (uv.x >= 1.0 - _CornerLength));

                float isYellowVertical = ((uv.x <= _YellowThickness) * (uv.y <= _CornerLength)) +
                                         ((uv.x >= 1.0 - _YellowThickness) * (uv.y <= _CornerLength)) +
                                         ((uv.x <= _YellowThickness) * (uv.y >= 1.0 - _CornerLength)) +
                                         ((uv.x >= 1.0 - _YellowThickness) * (uv.y >= 1.0 - _CornerLength));

                float isYellow = saturate(isYellowHorizontal + isYellowVertical);

                float isBlackHorizontal = ((uv.y <= _BlackThickness) * (uv.x > _CornerLength) * (uv.x < 1.0 - _CornerLength)) +
                                          ((uv.y >= 1.0 - _BlackThickness) * (uv.x > _CornerLength) * (uv.x < 1.0 - _CornerLength));

                float isBlackVertical = ((uv.x <= _BlackThickness) * (uv.y > _CornerLength) * (uv.y < 1.0 - _CornerLength)) +
                                        ((uv.x >= 1.0 - _BlackThickness) * (uv.y > _CornerLength) * (uv.y < 1.0 - _CornerLength));

                float isBlack = saturate(isBlackHorizontal + isBlackVertical) * (1.0 - isYellow);

                float4 color = _YellowColor * isYellow + _BlackColor * isBlack;
                float alpha = saturate(isYellow + isBlack);

                return float4(color.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
