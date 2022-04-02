// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Squax/CameraImageEffect" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader 
	{
        // Pixel ramp shader.
		Pass
		{
			Tags {
                "IgnoreProjector" = "True"
                "Queue" = "Transparent"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
			}
			ZTest Always
			ZWrite Off
            Cull Off
            Lighting Off
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members scrPos)
//#pragma exclude_renderers d3d11 xbox360
            #pragma fragmentoption ARB_precision_hint_fastest 
			#pragma vertex vert
			#pragma fragment frag
			//#pragma target 2.0
			#include "UnityCG.cginc"
            
            #define COLORS 32.0

			uniform sampler2D _MainTex;
            uniform sampler2D _OcclusionSource;
			uniform sampler2D _LightingSource;
            
            /*
            sampler2D _LUT;
            float4 _LUT_TexelSize;
            float _Contribution;*/
	 
			struct v2f {
			   float4 position : SV_POSITION;
			   float2 uv : TEXCOORD0;
			};
            
			float3 rgb2hsv(float3 c)
			{
			    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			    float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
			    float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

			    float d = q.x - min(q.w, q.y);
			    float e = 1.0e-10;
			    
			    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

			float3 hsv2rgb(float3 c)
			{
			    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
			    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
			    
			    return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
			}
			
			v2f vert(appdata_img v)
			{
			   	v2f o;
				o.position = UnityPixelSnap(UnityObjectToClipPos(v.vertex));
				o.uv = v.texcoord.xy;

			   	return o;
			}
            
			float rand(float2 coord) {
				coord = fmod(coord, float2(2.0,1.0)*round(64));
				return frac(sin(dot(coord.xy ,float2(12.9898,78.233))) * 15.5453 * 2);
			}
            
			float noise(float2 coord){
				float2 i = floor(coord);
				float2 f = frac(coord);
				
				float a = rand(i);
				float b = rand(i + float2(1.0, 0.0));
				float c = rand(i + float2(0.0, 1.0));
				float d = rand(i + float2(1.0, 1.0));

				float2 cubic = f * f * (3.0 - 2.0 * f);

				return lerp(a, b, cubic.x) + (c - a) * cubic.y * (1.0 - cubic.x) + (d - b) * cubic.x * cubic.y;
			}
            
			half4 frag (v2f input) : COLOR
			{
                half4 diffuse = tex2D(_MainTex, input.uv);
				//half4 occlusion = tex2D(_OcclusionSource, input.uv);
                //half4 lighting = tex2D(_LightingSource, input.uv);
                
                return diffuse;
			}
			
			ENDCG
		}
	} 
    
	FallBack "Diffuse"
}