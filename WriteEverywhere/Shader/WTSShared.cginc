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

void surfBack(Input IN, inout SurfaceOutput o){
	fixed2 uv =  IN.uv_MainTex;
	if(_MirrorBack){
		uv.x = 1 - uv.x;
	}
	uv = calculateUV(uv);
	fixed4 t = tex2D(_MainTex, uv);
	o.Albedo =saturate(_BackfaceColor+0.01) ;
	o.Alpha = t.a;
	normalPass(t, IN.uv_MainTex, o);
}

void surfFront(Input IN, inout SurfaceOutput o){
	fixed4 t = tex2D(_MainTex, IN.uv_MainTex);
	fixed4 effectiveColor = saturate(_Color+0.01);
	o.Albedo = t * effectiveColor;
	o.Alpha = t.a;
	o.Emission = t * _Color * _SurfProperties.z * t.a * 10;

	normalPass(t, IN.uv_MainTex, o);
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