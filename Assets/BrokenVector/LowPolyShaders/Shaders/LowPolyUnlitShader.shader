Shader "LowPolyShaders/LowPolyUnlitShader_URP"
{
    Properties
    {
        _MainTex ("Color Scheme", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline" // URPであることを明示
            "Queue" = "Geometry"
        }
        LOD 200

        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // URPのコアライブラリをインクルード
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
            };

            // テクスチャとサンプラーの定義
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // SRP Batcher対応のためのCBUFFER
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float4 _MainTex_ST;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // 頂点位置をクリップ空間へ変換
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);

                // 元のシェーダーと同じロジック：
                // 頂点シェーダー内でテクスチャをサンプリングする (Vertex Texture Fetch)
                // LOD 0を指定してサンプリングします
                float2 uv = TRANSFORM_TEX(input.uv, _MainTex);
                half4 texColor = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, uv, 0);
                
                // テクスチャカラーとTintカラーを乗算して出力
                output.color = texColor * _Color;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 頂点から補間された色をそのまま返す (Unlit)
                return input.color;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}