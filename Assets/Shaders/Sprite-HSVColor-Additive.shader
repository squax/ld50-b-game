// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Squax/Sprite/HSVColor/Additive" {
	Properties {
		[PerRendererData] _MainTex ("Base (RGB)", 2D) = "white" {}
		_Color ("Main Color", COLOR) = (1,1,1,1)
	}
	SubShader {
        Tags
        {
                "IgnoreProjector" = "True"
                "Queue" = "Transparent"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
        }

        Lighting Off
        ZWrite Off
        Fog { Mode Off }
        Blend SrcAlpha One
        ColorMask RGB
		AlphaTest Off

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma target 2.0
			//#pragma fragmentoption ARB_precision_hint_fastest

			#include "UnityCG.cginc"

			sampler2D _MainTex;

			struct vertexInput {
				half4 vertex : POSITION;
				half2 texcoord0 : TEXCOORD0;
				half4 color : COLOR;
			};

			struct fragmentInput{
				half4 position : SV_POSITION;
				half2 texcoord0 : TEXCOORD0;
				half4 hsv : COLOR;
			};
			
			float3 rgb2hsv(half3 c)
			{
			    fixed4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			    fixed4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
			    fixed4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

			    fixed d = q.x - min(q.w, q.y);
			    fixed e = 1.0e-10;
			    
			    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

			half3 hsv2rgb(half3 c)
			{
			    fixed4 K = fixed4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
			    fixed3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
			    
			    return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
			}
			
			fragmentInput vert(vertexInput i)
			{
		        fragmentInput o;
		        
		        o.position = UnityPixelSnap(UnityObjectToClipPos(i.vertex));
		        
		        o.texcoord0 = i.texcoord0;
		        o.hsv = i.color;
		        
		        return o;
			}

			fixed4 frag(fragmentInput i) : COLOR
			{
				half4 c = tex2D (_MainTex, i.texcoord0);
				
	    		half3 fragHSV = rgb2hsv(c.rgb).xyz;
	    		fragHSV.x = fragHSV.x + i.hsv.x;
	    		fragHSV.yz *= i.hsv.yz * 2;
	    		
				return clamp(2.0 * float4(hsv2rgb(fragHSV), c.a * i.hsv.a), 0, 1);
			}
			ENDCG
		}
	} 
}
