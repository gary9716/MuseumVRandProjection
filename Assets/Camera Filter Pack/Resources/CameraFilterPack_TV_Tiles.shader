////////////////////////////////////////////
// CameraFilterPack - by VETASOFT 2016 /////
////////////////////////////////////////////


Shader "CameraFilterPack/TV_Tiles" { 
Properties 
{
_MainTex ("Base (RGB)", 2D) = "white" {}
_TimeX ("Time", Range(0.0, 1.0)) = 1.0
_ScreenResolution ("_ScreenResolution", Vector) = (0.,0.,0.,0.)
}
SubShader
{
Pass
{
ZTest Always
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#pragma target 3.0
#pragma glsl
#include "UnityCG.cginc"
uniform sampler2D _MainTex;
uniform float _TimeX;
uniform float _Value;
uniform float _Value2;
uniform float _Value3;
uniform float _Value4;
uniform float Fade;
uniform float4 _ScreenResolution;
struct appdata_t
{
float4 vertex   : POSITION;
float4 color    : COLOR;
float2 texcoord : TEXCOORD0;
};
struct v2f
{
half2 texcoord  : TEXCOORD0;
float4 vertex   : SV_POSITION;
fixed4 color    : COLOR;
};
v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;
return OUT;
}
float4 frag (v2f i) : COLOR
{
float2 pos = 256.0*i.texcoord.xy/1.0 + _TimeX;

float dist2 = 1.0 - smoothstep(_Value3,_Value3-_Value4, length(float2(0.5,0.5) - i.texcoord));
float4 txt = tex2D(_MainTex, i.texcoord);
float3 col = float3(0.0,0.0,0.0);
for( int i=0; i<6; i++ ) 
{
float2 a = floor(pos);
float2 b = frac(pos);
float4 w = frac((sin(a.x*7.0+31.0*a.y + 0.01*_TimeX)+float4(0.035,0.01,0.0,0.7))*13.54517);

col += w.xyz * smoothstep(0.45,0.55,w.w) * sqrt(16.0*b.x*b.y*(1.0-b.x)*(1.0-b.y) ); 
pos /= 2.0*_Value; 
col /= 2.0;
}

col = pow( 2.5*col, float3(1.0,1.0,0.7));
col =txt *(col*_Value2);
float3 ret=lerp(txt.rgb,col,dist2*Fade);
return  float4( ret, 1.0 );
}
ENDCG
}
}
}
