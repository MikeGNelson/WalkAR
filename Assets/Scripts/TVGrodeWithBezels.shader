Shader "Custom/TVGridWithBezels"
{
    Properties
    {
        _GridSize ("Grid Size (Columns, Rows)", Vector) = (4, 3, 0, 0)
        _BezelWidth ("Bezel Width", Float) = 0.05
        _TVColor ("TV Screen Color", Color) = (0.1, 0.1, 0.1, 1)
        _BezelColor ("Bezel Color", Color) = (0, 0, 0, 1)
    }

    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

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

            float2 _GridSize; // Columns and rows
            float _BezelWidth;
            float4 _TVColor;
            float4 _BezelColor;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                float2 cell = uv * _GridSize;
                float2 cellUV = frac(cell);

                float bezel = step(_BezelWidth, cellUV.x) * step(cellUV.x, 1.0 - _BezelWidth) *
                              step(_BezelWidth, cellUV.y) * step(cellUV.y, 1.0 - _BezelWidth);

                float4 color = lerp(_BezelColor, _TVColor, bezel);
                return color;
            }
            ENDHLSL
        }
    }
}
