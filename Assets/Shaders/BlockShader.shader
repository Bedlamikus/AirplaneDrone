Shader "Custom/BlockShader"
{
    Properties
    {
        _MainTex ("Texture Atlas", 2D) = "white" {}
        _AtlasSize ("Atlas Size", Int) = 4
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        
        sampler2D _MainTex;
        int _AtlasSize;
        
        struct Input
        {
            float2 uv_MainTex;
        };
        
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Просто используем UV координаты напрямую
            // Они уже вычислены в ChunkRenderer для texture atlas
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

