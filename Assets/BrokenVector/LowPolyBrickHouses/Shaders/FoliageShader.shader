Shader "Custom/FoliageShader_URP"
{
    Properties
    {
       _Color("Color", Color) = (1,1,1,1)
       _MainTex("Albedo", 2D) = "white" {}
       _BumpMap("Normal Map", 2D) = "bump" {}
       _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
       _Glossiness("Smoothness", Range(0,1)) = 0.5
       _Metallic("Metallic", Range(0,1)) = 0.0
       _Intensity("Intensity", Range(0, 1.0)) = 1.0
       _WindSpeed("WindSpeed", Range(0, 10.0)) = 1.0
       _Randomness("Randomness", Range(0, 5.0)) = 1.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "TransparentCutout" 
            "RenderPipeline" = "UniversalPipeline" 
            "Queue" = "AlphaTest"
            "IgnoreProjector" = "True"
        }
        LOD 200

        // 共通のHLSLコード（風の計算ロジック）を定義
        HLSLINCLUDE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // ランダム関数
            float random(float3 p)
            {
                return frac(43758.5453 * sin(dot(p, float3(12.9898, 78.233, 45.5432)) % 3.14159));
            }

            // 風の計算関数
            // positionOS: オブジェクト空間の頂点位置
            // positionWS: ワールド空間の頂点位置（ノイズの種として使用）
            float3 ApplyWind(float3 positionOS, float3 positionWS, float intensity, float speed, float randomness)
            {
                // _Time.y は Unityの組み込み時間変数
                float3 offset = intensity * (sin(positionWS + _Time.y * speed) + randomness * random(positionWS));
                return positionOS + offset;
            }
        ENDHLSL

        // ------------------------------------------------------------------
        //  Forward Lit Pass (メイン描画)
        // ------------------------------------------------------------------
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            // キーワード定義
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ _NORMALMAP // ノーマルマップ用

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT; // ノーマルマップに必要
                float2 uv           : TEXCOORD0;
                float2 lightmapUV   : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 positionWS   : TEXCOORD0;
                float3 normalWS     : TEXCOORD1;
                float3 tangentWS    : TEXCOORD3;
                float3 bitangentWS  : TEXCOORD4;
                float2 uv           : TEXCOORD5;
                half4  color        : COLOR;
                DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 6);
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float4 _MainTex_ST;
                half _Glossiness;
                half _Metallic;
                half _Cutoff;
                float _Intensity;
                float _WindSpeed;
                float _Randomness;
            CBUFFER_END

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                // 1. まず現在のワールド座標を取得（風のノイズ計算用）
                float3 initialPositionWS = TransformObjectToWorld(input.positionOS.xyz);

                // 2. 風の計算を適用してオブジェクト座標をずらす
                float3 animatedPositionOS = ApplyWind(input.positionOS.xyz, initialPositionWS, _Intensity, _WindSpeed, _Randomness);

                // 3. ずらした座標を使って正式な計算を行う
                VertexPositionInputs vertexInput = GetVertexPositionInputs(animatedPositionOS);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;

                // 法線・タンジェントの計算
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;
                
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.staticLightmapUV);
                OUTPUT_SH(output.normalWS, output.vertexSH);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // テクスチャサンプリングとAlpha Cutoff
                half4 albedoAlpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
                clip(albedoAlpha.a - _Cutoff); // 透明度チェック

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedoAlpha.rgb;
                surfaceData.alpha = albedoAlpha.a;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Glossiness;
                surfaceData.occlusion = 1;
                surfaceData.specular = 0;

                // ノーマルマップの適用
                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv));
                surfaceData.normalTS = normalTS;

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;

                // 法線マップを考慮したワールド法線の再構築
                inputData.normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS, input.bitangentWS, input.normalWS));
                inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);

                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                
                #if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
                    float4 clipPos = TransformWorldToHClip(inputData.positionWS);
                    inputData.shadowCoord = ComputeScreenPos(clipPos);
                #else
                    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
                #endif

                inputData.fogCoord = ComputeFogFactor(input.positionCS.z);
                inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);

                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                
                return color;
            }
            ENDHLSL
        }

        // ------------------------------------------------------------------
        //  Shadow Caster Pass (影用パス：ここにも風が必要)
        // ------------------------------------------------------------------
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Off // 葉っぱなので両面描画推奨

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            // プロパティ再定義（CBUFFER_START外でアクセスするため）
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float4 _MainTex_ST;
                half _Glossiness;
                half _Metallic;
                half _Cutoff;
                float _Intensity;
                float _WindSpeed;
                float _Randomness;
            CBUFFER_END
            
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            float3 _LightDirection;

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                
                // 風の計算（メインパスと同じロジック）
                float3 initialPositionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 animatedPositionOS = ApplyWind(input.positionOS.xyz, initialPositionWS, _Intensity, _WindSpeed, _Randomness);
                
                float3 positionWS = TransformObjectToWorld(animatedPositionOS);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif

                output.positionCS = positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                // 影の形も葉っぱの形にする
                half4 albedoAlpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
                clip(albedoAlpha.a - _Cutoff);
                return 0;
            }
            ENDHLSL
        }

        // ------------------------------------------------------------------
        //  Depth Only Pass (ポストプロセス等で必要)
        // ------------------------------------------------------------------
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float4 _MainTex_ST;
                half _Cutoff;
                float _Intensity;
                float _WindSpeed;
                float _Randomness;
                // 他の変数は不要
            CBUFFER_END

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                // 風の計算
                float3 initialPositionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 animatedPositionOS = ApplyWind(input.positionOS.xyz, initialPositionWS, _Intensity, _WindSpeed, _Randomness);

                output.positionCS = TransformObjectToHClip(animatedPositionOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 DepthOnlyFragment(Varyings input) : SV_Target
            {
                half4 albedoAlpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
                clip(albedoAlpha.a - _Cutoff);
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}