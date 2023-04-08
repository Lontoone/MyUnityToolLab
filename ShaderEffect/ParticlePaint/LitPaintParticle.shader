Shader "Unlit/LitPaintParticle"
{
    Properties
    {
        _ShadowColor ("Shadow Color", color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" { }
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        LOD 100

        Pass
        {
            //Tags { "LightMode" = "UniversalForward" }// Pass specific tags.
            Tags { 
                //"RenderType"="Transparent"
                "RenderPipeline" = "UniversalPipeline"
                "LightMode"="UniversalForward"
            }
            // "UniversalForward" tells Unity this is the main lighting pass of this shader
            ZWrite Off
            HLSLPROGRAM // Begin HLSL code
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl"
            
            #define _SPECULAR_COLOR
            #define _MAIN_LIGHT_SHADOWS
            
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            // Register our programmable stage functions
            #pragma vertex Vertex
            #pragma fragment Fragment
            
            float3 transform_center;
            struct particle
            {
                float3 position;
                float3 rotation;
                float3 scale;
                float4 color;
            };
            void rotate2D(inout float2 v, float r)
            {
                float s, c;
                sincos(radians(r), s, c);
                v = float2(v.x * c - v.y * s, v.x * s + v.y * c);
            }
            StructuredBuffer<particle> _PointsBuffer;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL; // Normal in object space

            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 positionWS : TEXCOORD1; // Position in world space
                float3 normalWS : TEXCOORD2; // Normal in world space
                float4 color :COLOR;

            };

            //sampler2D _MainTex;
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float4 _ShadowColor;
            v2f Vertex(appdata v  , uint inst:SV_INSTANCEID)
            {
                v2f o;
                //o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                VertexPositionInputs posInputs = GetVertexPositionInputs(v.vertex);
                VertexNormalInputs normInputs = GetVertexNormalInputs(v.normalOS);
                
                particle p = _PointsBuffer[inst];     
                rotate2D(p.position.xy, p.rotation);    
                float3 localPos = mul(unity_WorldToObject , p.position + transform_center).xyz;                
                localPos +=v.vertex * p.scale; 
                //localPos +=v.vertex ; 
                //o.vertex = UnityObjectToClipPos(localPos);
                VertexPositionInputs obj2Clip = GetVertexPositionInputs(localPos);
                o.vertex = obj2Clip.positionCS; 
                //o.vertex = posInputs.positionCS;
                o.color =p.color;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                //Need For Lighting
                o.normalWS = normInputs.normalWS;
                o.positionWS = obj2Clip.positionWS;
                return o;
            }

            float4 Fragment(v2f i) : SV_TARGET
            {
                // sample the texture
                //float4 col = tex2D(_MainTex, i.uv);
                float4 colorSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * i.color;
                clip(colorSample.a -0.01);
                Light mainLight = GetMainLight();
                real4 LightColor = real4(mainLight.color, 1);                     //获取主光源的颜色
                float3 LightDir = normalize(mainLight.direction);                //获取光照方向
                float LdotN = dot(LightDir, i.normalWS) * 0.5 + 0.5;                        //LdotN
                // For lighting, create the InputData struct, which contains position and orientation data
                InputData lightingInput = (InputData)0; // Found in URP/ShaderLib/Input.hlsl
                lightingInput.positionWS = i.positionWS;
                lightingInput.normalWS = normalize(i.normalWS);
                lightingInput.viewDirectionWS = GetWorldSpaceNormalizeViewDir(i.positionWS); // In ShaderVariablesFunctions.hlsl
                lightingInput.shadowCoord = TransformWorldToShadowCoord(i.positionWS); // In Shadows.hlsl
                
                #ifdef _MAIN_LIGHT_SHADOWS
                    VertexPositionInputs vertexInput = (VertexPositionInputs)0;
                    vertexInput.positionWS = i.positionWS;
                    
                    float4 shadowCoord = GetShadowCoord(vertexInput); 
                    half shadowAttenutation = MainLightRealtimeShadow(shadowCoord) *0.5 + 0.5;
                    //colorSample = lerp(colorSample, _ShadowColor, (1.0 - shadowAttenutation) * _ShadowColor.a);
                    colorSample *= shadowAttenutation;
                    //color.rgb = MixFogColor(color.rgb, half3(1,1,1), input.fogCoord);
                    //return color * shadowAttenutation;
                #endif

                MixRealtimeAndBakedGI(mainLight, lightingInput.normalWS, lightingInput.bakedGI, half4(0, 0, 0, 0));                    //获取场景主光源
                float4 diffuseColor = colorSample;

                #ifdef _ADDITIONAL_LIGHTS
                    uint pixelLightCount = GetAdditionalLightsCount();
                    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                    {
                        Light light = GetAdditionalLight(lightIndex, inputData.positionWS);
                        half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
                        diffuseColor += LightingLambert(attenuatedLightColor, light.direction, inputData.normalWS);
                        float4 specularColor += LightingSpecular(attenuatedLightColor, light.direction, inputData.normalWS, inputData.viewDirectionWS, specularGloss, smoothness);
                    }
                #endif

                #ifdef _ADDITIONAL_LIGHTS_VERTEX
                    diffuseColor += inputData.vertexLighting;
                #endif

                half3 finalColor =  colorSample;
                return half4(finalColor , colorSample.a );

            }
            ENDHLSL
        }
    }
    Fallback "Specular"
}
