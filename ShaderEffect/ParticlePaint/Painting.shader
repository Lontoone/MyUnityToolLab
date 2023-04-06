// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Unlit/Painting"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Tags {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "RenderQueue" = "Transparent"
        }
        Tags { "LightMode" = "UniversalForward" }
        CGPROGRAM

        #pragma multi_compile_instancing
        #pragma target 4.5
        #pragma vertex vert
        #pragma fragment frag
        // make fog work
        #pragma multi_compile_fog   
        #include "UnityLightingCommon.cginc"            
        #include "AutoLight.cginc"
        /*
        */
        #include "UnityCG.cginc"

        float3 transform_center;
        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            float2 uv : TEXCOORD0;
            UNITY_FOG_COORDS(1)
            float4 vertex : SV_POSITION;
            float3 worldPos:TEXCOORD1;
            float3 ray :TEXCOORD2;
            float4 screenUV : TEXCOORD3;
            float4 color :COLOR;

        };
        struct particle {
            float3 position;
            float3 rotation;
            float3 scale;
            float4 color;
        };
        void rotate2D(inout float2 v, float r)
        {
            float s, c;
            sincos(radians(r)  , s, c);
            v = float2(v.x * c - v.y * s, v.x * s + v.y * c);
        }
        StructuredBuffer<particle> _PointsBuffer;
        sampler2D _CameraDepthTexture; //é˘C#ê›íË camera.depthTextureMode |= DepthTextureMode.Depth;            
        sampler2D _MainTex;
        float4 _MainTex_ST;

        v2f vert(appdata v , uint inst:SV_INSTANCEID)
        {
            v2f o;
            particle p = _PointsBuffer[inst];
            rotate2D(p.position.xz, p.rotation);
            float3 localPos = mul(unity_WorldToObject , p.position + transform_center).xyz;
            localPos += v.vertex * p.scale;
            o.vertex = UnityObjectToClipPos(localPos);
            o.ray = UnityObjectToViewPos(v.vertex).xyz * float3(-1,-1,1);

            o.screenUV = ComputeScreenPos(o.vertex); //ôíçŸãÛä‘=> ViewportãÛä‘ÅB ç∂â∫(0,0) ~ âEè„(1,1)                
            
            o.color = p.color;
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            UNITY_TRANSFER_FOG(o,o.vertex);
            return o;
        }

        fixed4 frag(v2f i) : SV_Target
        {            
            fixed shadow = SHADOW_ATTENUATION(i);
            float attenuation = LIGHT_ATTENUATION(i);
            attenuation = attenuation * 0.5 + 0.5;
            fixed4 col = tex2D(_MainTex, i.uv) * i.color * shadow * attenuation;
            UNITY_APPLY_FOG(i.fogCoord, col);

            return col;
        }
        ENDCG
    }
    }
        Fallback "Specular"
}