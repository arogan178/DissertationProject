Shader "Custom/UnderwaterShader"
{
    Properties
    {
        WaterColor("WaterColor", Color) = (0.4,0.8,0.9,0.3)
        WaterLevel("WaterLevel", Float) = 100
        WaterDepth("WaterDepth", Float) = 30
    }
       
    SubShader
    {
        Tags { "Queue" = "Overlay" "ForceNoShadowCasting" = "True" "IgnoreProjector" = "True" }
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        //Fog { Mode Off }

        CGPROGRAM
        #pragma surface surf Lambert keepalpha noshadow

        fixed4 TopColor;
        fixed4 WaterColor;
        fixed4 ColorRes;
        float WaterLevel;
        float WaterDepth;

        struct Input
        {
            float3 worldPos;            
        };

        
        void surf (Input IN, inout SurfaceOutput o)
        {
            float h = (WaterLevel - IN.worldPos.y) / (WaterLevel - (WaterLevel - WaterDepth));
            ColorRes.a = lerp(TopColor.a, WaterColor.a, h);
            o.Albedo = WaterColor.rgb;
            o.Alpha = ColorRes.a;
            if (IN.worldPos.y >= WaterLevel)
                o.Alpha = TopColor.a;
            if (IN.worldPos.y < (WaterLevel - WaterDepth))
                o.Alpha = WaterColor.a;
        }
        ENDCG
    }
    Fallback "Transparent/VertexLit"
}