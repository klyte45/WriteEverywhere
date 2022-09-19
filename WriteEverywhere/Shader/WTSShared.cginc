#ifndef SHARED_WTS_SHARED
#define SHARED_WTS_SHARED

struct Input
{
    float2 uv_MainTex;
    float2 uv_BumpMap;
};

sampler2D _MainTex;
fixed4 _Color;
float4 _MainTex_TexelSize;
fixed4 _SurfProperties;
fixed4 _BackfaceColor;
bool _MirrorBack;
fixed4 _Border;
float _PixelsPerMeters;
fixed2 _Dimensions;
half3 lightDir;
uniform 	fixed4 hlslcc_mtx4x4unity_ObjectToWorld[4];
uniform 	fixed4 hlslcc_mtx4x4unity_MatrixVP[4];

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
	if(length(_Border)>0){
		return fixed2(0,0);
	}
	return uvInput;
}

void vert(inout appdata_full v, out Input o)
{
	#if defined(PIXELSNAP_ON)
		v.vertex = UnityPixelSnap (v.vertex);
	#endif
	UNITY_INITIALIZE_OUTPUT(Input, o);
}

void surfBack(Input IN, inout SurfaceOutput o){
	fixed2 uv =  IN.uv_MainTex;
	if(_MirrorBack){
		uv.x = 1 - uv.x;
	}
	uv = calculateUV(uv);
	fixed4 t = tex2D(_MainTex, uv);
	o.Albedo = _BackfaceColor;
	o.Alpha = t.a;
	normalPass(t, IN.uv_MainTex, o);
}

void surfFront(Input IN, inout SurfaceOutput o){
	fixed4 t = tex2D(_MainTex, IN.uv_MainTex);
	fixed4 effectiveColor = saturate(_Color+0.01);
	o.Albedo = t * _Color;
	o.Alpha = t.a;
	o.Emission = t * _Color * _SurfProperties.z * t.a * 10;
	o.Specular = 0;
	o.Gloss = 50;
	normalPass(t, IN.uv_MainTex, o);
}

#endif // SHARED_WTS_SHARED