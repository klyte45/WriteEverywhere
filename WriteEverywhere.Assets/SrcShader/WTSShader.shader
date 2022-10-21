Shader "Custom/WriteEverything/Default" {
    Properties
    {
  
        _Color("Main Color", Color) = (1,1,1,1)
        _BackfaceColor("Backface Color", Color) = (0.01,0.01,0.01,1)
        _MainTex("Diffuse (RGBA)", 2D) = "transparent" {}
        _SurfProperties("Norm, Gloss, Illum, Em.Str", Vector) = (0,0,0,0)
        _Cutout("Alpha cutoff", Range(0,1)) = 0.004
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
       
        UsePass "Custom/WriteEverything/Default/Back/FORWARD"
        UsePass "Custom/WriteEverything/Default/Front/FORWARD"
        UsePass "Custom/WriteEverything/Default/Back/PREPASS"
        UsePass "Custom/WriteEverything/Default/Front/PREPASS" 
        UsePass "Custom/WriteEverything/Default/Back/DEFERRED"
        UsePass "Custom/WriteEverything/Default/Front/DEFERRED"
        UsePass "Custom/WriteEverything/Default/Both/SHADOWCASTER"
        UsePass "Custom/WriteEverything/Default/Both/META"   

    }
}