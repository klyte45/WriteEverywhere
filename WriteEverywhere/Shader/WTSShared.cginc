#ifndef SHARED_WTS_SHARED
#define SHARED_WTS_SHARED

struct Input
{
    float2 uv_MainTex;
    float2 uv_BumpMap;
};

sampler2D _MainTex;
fixed4 _Color;
fixed4 _SurfProperties;
fixed4 _BackfaceColor;
bool _MirrorBack;
fixed4 _Border;
float _PixelsPerMeters;
fixed3 _Dimensions;
half3 lightDir;
uniform 	fixed4 hlslcc_mtx4x4unity_ObjectToWorld[4];
uniform 	fixed4 hlslcc_mtx4x4unity_MatrixVP[4];
uniform float4 _MainTex_TexelSize;

 void commonVert(inout appdata_full v){
	//v.vertex.xyz *= _Dimensions;
	#if defined(PIXELSNAP_ON)
		v.vertex = UnityPixelSnap (v.vertex);
	#endif
 }

void normalPass(fixed4 textureColor, fixed2 uv, inout SurfaceOutput o) {
	float sample_l;
	float sample_r;
	float sample_u;
	float sample_d;
	float x_vector;
	float y_vector;

	if (uv.x > 0) { sample_l = tex2D(_MainTex, uv - half2(_MainTex_TexelSize.x,0)).a; }
	else { sample_l = textureColor.a+0.001; }
	if (uv.x < 1) { sample_r = tex2D(_MainTex, uv + half2(_MainTex_TexelSize.x,0)).a; }
	else { sample_r = textureColor.a+0.001; }
	if (uv.y > 0) { sample_u = tex2D(_MainTex, uv - half2(0,_MainTex_TexelSize.y)).a; }
	else { sample_u = textureColor.a+0.001; }
	if (uv.y < 1) { sample_d = tex2D(_MainTex, uv + half2(0,_MainTex_TexelSize.y)).a; }
	else { sample_d = textureColor.a+0.001; }
	x_vector = (((sample_l - sample_r) * (_SurfProperties.x) + 1) * .5f);
	y_vector = (((sample_u - sample_d) * (_SurfProperties.x) + 1) * .5f);

	o.Normal = UnpackNormal(Vector(1,x_vector,y_vector,0));
}

fixed2 calculateUV(fixed2 uvInput){
	if(length(_Border) > 0){
		
		fixed2 ratio = _MainTex_TexelSize.zw/_PixelsPerMeters/_Dimensions.xy;
		fixed2 bT=_Border.xy*ratio;
		fixed2 BT=_Border.zw*ratio;
		fixed2 deltaLgt = (1-_Border.xy-_Border.zw)/(1-bT-BT);

		fixed2 result = fixed2(0,0);
		if(uvInput.x<bT.x){
			result.x= uvInput.x/ratio.x;
		}else if(uvInput.x>1-BT.x){
			result.x= 1-((1-uvInput.x)/ratio.x);
		}else{
			result.x= _Border.x+(uvInput.x-bT.x)*deltaLgt.x;
		}
		if(uvInput.y<bT.y){
			result.y= uvInput.y/ratio.y;
		}else if(uvInput.y>1-BT.y){
			result.y= 1-((1-uvInput.y)/ratio.y);
		}else{
			result.y= _Border.y+(uvInput.y-bT.y)*deltaLgt.y;
		}
		return saturate(result);
	}
	return uvInput;
}

void surfBack(Input IN, inout SurfaceOutput o){
	fixed2 uv = calculateUV(IN.uv_MainTex);
	fixed4 t = tex2D(_MainTex, uv);
	o.Albedo = _BackfaceColor;
	o.Alpha = t.a;
	normalPass(t, uv, o);
}

void surfFront(Input IN, inout SurfaceOutput o){
	fixed2 uv =  calculateUV(IN.uv_MainTex);
	fixed4 t = tex2D(_MainTex, uv);
	o.Albedo = t * _Color;
	o.Alpha = t.a;
	o.Emission = t * _Color * _SurfProperties.z * t.a * 10;
	normalPass(t, uv, o);
}

void Unity_RotateAboutAxis_Degrees_float(float3 In, float3 Axis, float Rotation, out float3 Out)
{
    Rotation = radians(Rotation);
    float s = sin(Rotation);
    float c = cos(Rotation);
    float one_minus_c = 1.0 - c;

    Axis = normalize(Axis);
    float3x3 rot_mat = 
    {   one_minus_c * Axis.x * Axis.x + c, one_minus_c * Axis.x * Axis.y - Axis.z * s, one_minus_c * Axis.z * Axis.x + Axis.y * s,
        one_minus_c * Axis.x * Axis.y + Axis.z * s, one_minus_c * Axis.y * Axis.y + c, one_minus_c * Axis.y * Axis.z - Axis.x * s,
        one_minus_c * Axis.z * Axis.x - Axis.y * s, one_minus_c * Axis.y * Axis.z + Axis.x * s, one_minus_c * Axis.z * Axis.z + c
    };
    Out = mul(rot_mat,  In);
}

#endif // SHARED_WTS_SHARED