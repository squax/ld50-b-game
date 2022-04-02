Shader "Squax/Image Effect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EdgeBrightness("Edge Brightness",range(0.0,30.0)) = 1.0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            sampler2D _OcclusionSource;
			sampler2D _LightingSource;
            sampler2D _GlowSource;
            float _EdgeBrightness;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 occlusion = tex2D(_OcclusionSource, i.uv);
                fixed4 lighting = tex2D(_LightingSource, i.uv);
                fixed4 glow = tex2D(_GlowSource, i.uv);
                
                float _PixelBorder = 10;

                // Right.
                fixed4 rightDirectionTex = tex2D(_LightingSource, i.uv + half2(_MainTex_TexelSize.x*_PixelBorder, 0.0));
                float rightDirection = (rightDirectionTex.r + rightDirectionTex.g + rightDirectionTex.b) / 3;
                
                fixed4 leftDirectionTex = tex2D(_LightingSource, i.uv + half2(-_MainTex_TexelSize.x*_PixelBorder, 0.0));
                float leftDirection = (leftDirectionTex.r + leftDirectionTex.g + leftDirectionTex.b) / 3;
                
                fixed4 upDirectionTex = tex2D(_LightingSource, i.uv + half2(0, _MainTex_TexelSize.y*_PixelBorder));
                float upDirection = (upDirectionTex.r + upDirectionTex.g + upDirectionTex.b) / 3;
                
                fixed4 downDirectionTex = tex2D(_LightingSource, i.uv + half2(0, -_MainTex_TexelSize.y*_PixelBorder));
                float downDirection = (downDirectionTex.r + downDirectionTex.g + downDirectionTex.b) / 3;
                
                half2 lightDir = half2(1, 1) - (normalize(half2((rightDirection - leftDirection),(upDirection - downDirection))));
                float dotP = max((dot((normalize(half2(-occlusion.r, -occlusion.g))), lightDir)), 0);

                //return (fixed4(lightDir.r,lightDir.g,0, 1)) + fixed4(occlusion.r,occlusion.g,0, 1);
                //return fixed4(dotP,dotP,dotP, 1);
                //return fixed4(occlusion.r,occlusion.g,occlusion.b, 1);
                return col * lighting + lighting * dotP * _EdgeBrightness + glow;
            }
            ENDCG
        }
    }
}
