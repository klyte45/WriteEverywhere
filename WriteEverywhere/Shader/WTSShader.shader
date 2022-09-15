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
            //"Queue" = "AlphaTest+10"
            //"RenderType" = "Vehicle"
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
        }

        //Cull Back
        //Lighting On
        //ZTest LEqual
        //ZWrite On
		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Deferred alphatest:_Cutout
        #include "UnityLightingCommon.cginc"
        #include "WTSShared.cginc"


        half4 LightingDeferred_PrePass(inout SurfaceOutput s, half4 light)
        {
            half4 c;
            c.rgb = s.Albedo.rgb * light * 0.375;
            c.a = s.Alpha;
            s.Gloss = light;
            s.Specular = light;
            return c;
        }

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed2 uv = calculateUV(IN.uv_MainTex);
            fixed4 t = tex2D(_MainTex, uv);
            o.Albedo = t.rgb * _Color;
            o.Alpha = t.a;
            o.Emission = t * _Color * _SurfProperties.z * t.a * 10;


            normalPass(t, IN.uv_MainTex, o);
        }
        ENDCG

        Cull Front
        Lighting On
        ZTest LEqual
        ZWrite On

        CGPROGRAM
        #pragma surface surf Deferred alphatest:_Cutout
        #include "UnityLightingCommon.cginc"
        #include "WTSShared.cginc"


        half4 LightingDeferred_PrePass(inout SurfaceOutput s, half4 light)
        {
            half4 c;
            c.rgb = s.Albedo.rgb * light;
            c.a = s.Alpha;
            return c;
        }

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed2 uv =  IN.uv_MainTex;
            if(_MirrorBack){
                uv.x = 1 - uv.x;
            }
            uv = calculateUV(uv);
            fixed4 t = tex2D(_MainTex, uv);
            o.Albedo = _BackfaceColor;
            o.Alpha = t.a;
            normalPass(t, uv, o);
        }
        ENDCG

    }
}