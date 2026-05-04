Shader "ChopChop/Food/Obi Fluid Vertex Color"
{
    Properties
    {
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _RimColor ("Rim Color", Color) = (1, 1, 1, 0.22)
        _RimPower ("Rim Power", Range(0.5, 8)) = 3
        _LightWrap ("Light Wrap", Range(0, 1)) = 0.55
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                float4 color : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Tint;
                float4 _RimColor;
                float _RimPower;
                float _LightWrap;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(positionInputs.positionWS);
                output.color = input.color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                Light mainLight = GetMainLight();
                float wrappedLight = saturate((dot(normalWS, mainLight.direction) + _LightWrap) / (1.0 + _LightWrap));
                float rim = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _RimPower);

                float vertexLuma = dot(input.color.rgb, float3(0.299, 0.587, 0.114));
                float3 baseColor = vertexLuma < 0.02 ? _Tint.rgb : input.color.rgb;
                baseColor = saturate(baseColor);
                float3 litColor = baseColor * (0.72 + wrappedLight * 0.38);
                litColor += _RimColor.rgb * rim * _RimColor.a;

                return half4(saturate(litColor), 1);
            }
            ENDHLSL
        }
    }
}
