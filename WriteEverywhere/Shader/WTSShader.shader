// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'


Shader "Custom/WriteEverything/Default" {
    Properties
    {
  
        _Color("Main Color", Color) = (1,1,1,1)
        _BackfaceColor("Backface Color", Color) = (0.01,0.01,0.01,1)
        _MainTex("Diffuse (RGBA)", 2D) = "transparent" {}
        _SurfProperties("Spec, Gloss, Illum, Em.Str", Vector) = (0,0,0,0)
        _Cutout("Alpha cutoff", Range(0,1)) = 0.5
        _MirrorBack("Mirror backface", Int) = 0
        _Border("Border Offsets", Vector) = (0,0,0,0)
        _PixelsPerMeters("Pixels per meters", float) = 1
        _Dimensions("Width and Height", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags
        {
           "QUEUE" = "AlphaTest"
           "RenderType" = "TransparentCutout"
        }

        Cull Back
        ZTest LEqual
        ZWrite On

        CGPROGRAM
        #pragma surface surf Deferred alphatest:_Cutout vertex:vert
        #include "UnityLightingCommon.cginc"
        #include "WTSShared.cginc"

        uniform 	fixed4 _SimulationTime;
        uniform 	fixed4 _WeatherParams; // Temp, Rain, Fog, Wetness

        half4 LightingDeferred_PrePass(inout SurfaceOutput s, half4 light)
        {
            half4 c;
            c.rgb = s.Albedo * clamp(light.rgb, 0, 1) * clamp(light.rgb, 0, 1);
            c.a = round(s.Alpha);
            return c;
        }
    
         half4 LightingDeferred(inout SurfaceOutput s)
        {
            return (1,1,1,1);
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            surfFront(IN, o);        
        }
        ENDCG
        Cull Front
        ZTest LEqual
        ZWrite On

        CGPROGRAM
        #pragma surface surf Deferred alphatest:_Cutout vertex:vert
        #include "UnityLightingCommon.cginc"
        #include "WTSShared.cginc"

        uniform 	fixed4 _SimulationTime;
        uniform 	fixed4 _WeatherParams; // Temp, Rain, Fog, Wetness

        half4 LightingDeferred_PrePass(inout SurfaceOutput s, half4 light)
        {
            half4 c;
            c.rgb = s.Albedo * clamp(light.rgb, 0, 1) * clamp(light.rgb, 0, 1);
            c.a = round(s.Alpha);
            return c;
        }       
         half4 LightingDeferred(inout SurfaceOutput s)
        {
            return (1,1,1,1);
        }


        void surf(Input IN, inout SurfaceOutput o)
        {
            surfBack(IN, o);           
        }
        ENDCG
    }



}