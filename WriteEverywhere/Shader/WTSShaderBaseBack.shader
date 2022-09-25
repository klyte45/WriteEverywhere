Shader "Custom/WriteEverything/Default/Back" {
    Properties
    {
  
        _Color("Main Color", Color) = (1,1,1,1)
        _BackfaceColor("Backface Color", Color) = (0.01,0.01,0.01,1)
        _MainTex("Diffuse (RGBA)", 2D) = "transparent" {}
        _SurfProperties("Spec, Gloss, Illum, Em.Str", Vector) = (0,0,0,0)
        _Cutout("Alpha cutoff", Range(0,1)) = 0.25
        _Border("Border Offsets", Vector) = (0,0,0,0)
        _PixelsPerMeters("Pixels per meters", float) = 1
        _Dimensions("Width and Height", Vector) = (1,1,1,0)
    }

    SubShader
    {
        Tags
        {
           "QUEUE" = "AlphaTest"
        }

        Cull Front
        ZTest LEqual
        ZWrite On

        CGPROGRAM
        #pragma surface surf Lambert addshadow alphatest:_Cutout vertex:vert
        #include "WTSShared.cginc"

        uniform 	fixed4 _SimulationTime;
        uniform 	fixed4 _WeatherParams; // Temp, Rain, Fog, Wetness
    
        
        void vert(inout appdata_full v, out Input o)
        {
            commonVert(v);
            Unity_RotateAboutAxis_Degrees_float(v.normal, float3 (0,1,0), 90,  v.normal.xyz);
            Unity_RotateAboutAxis_Degrees_float(v.tangent, float3 (0,1,0), 90,  v.tangent.xyz);
            UNITY_INITIALIZE_OUTPUT(Input, o);
        }
        void surf(Input IN, inout SurfaceOutput o)
        {
            surfBack(IN, o);    
        }
        ENDCG
    }



}