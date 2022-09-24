Shader "Custom/WriteEverything/Frame" {
    Properties
    {
  
        _Color("Main Color", Color) = (1,1,1,1)
        _BackfaceColor("Backface Color", Color) = (0.01,0.01,0.01,1)
        _MainTex("Diffuse (RGBA)", 2D) = "transparent" {} 
        _SurfProperties("Spec, Gloss, Illum, Em.Str", Vector) = (0,0,0,0)
        _Cutout("Alpha cutoff", Range(0,1)) = 0.5
        _Border("Border Offsets LRTB", Vector) = (0,0,0,0)
        _PixelsPerMeters("Pixels per meters", float) = 100
        _Dimensions("Size", Vector) = (1,1,1,0)
    }

    SubShader
    {
        Tags
        {
           "QUEUE" = "AlphaTest"
        }        
       
        UsePass "Custom/WriteEverything/Frame/Back/FORWARD"
        UsePass "Custom/WriteEverything/Frame/Back/PREPASS"
        UsePass "Custom/WriteEverything/Frame/Back/DEFERRED"
        UsePass "Custom/WriteEverything/Frame/Back/SHADOWCASTER"
        UsePass "Custom/WriteEverything/Frame/Back/META"

        
        UsePass "Custom/WriteEverything/Frame/Front/FORWARD"
        UsePass "Custom/WriteEverything/Frame/Front/PREPASS"
        UsePass "Custom/WriteEverything/Frame/Front/DEFERRED"
        UsePass "Custom/WriteEverything/Frame/Front/SHADOWCASTER"
        UsePass "Custom/WriteEverything/Frame/Front/META"
    }
}