Shader "Custom/FogOfWar"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Mask ("Fog Mask", 2D) = "black" {} // đen = che phủ, trắng = sáng rõ
        _FogColor ("Fog Color", Color) = (0, 0, 0, 1)
        _FogIntensity ("Fog Intensity", Range(0, 1)) = 0.8
        _RevealedIntensity ("Revealed Intensity", Range(0, 1)) = 0.3 // Độ mờ của vùng đã khám phá
        _FadeDistance ("Fade Distance", Range(0, 1)) = 0.2 // Khoảng cách fade
        _Tiling ("Texture Tiling", Vector) = (1, 1, 0, 0)
        _Offset ("Texture Offset", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _Mask;
            float4 _MainTex_ST;
            float4 _FogColor;
            float _FogIntensity;
            float _RevealedIntensity;
            float _FadeDistance;
            float4 _Tiling;
            float4 _Offset;

            struct appdata 
            { 
                float4 vertex : POSITION; 
                float2 uv : TEXCOORD0; 
            };

            struct v2f 
            { 
                float4 pos : SV_POSITION; 
                float2 uv : TEXCOORD0;
                float2 maskUV : TEXCOORD1;
                UNITY_FOG_COORDS(2)
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex) * _Tiling.xy + _Offset.xy;
                o.maskUV = v.uv;
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Đọc mask: 0 = che phủ hoàn toàn, 1 = sáng rõ
                float mask = tex2D(_Mask, i.maskUV).r;
                
                // Tạo gradient fade ở vùng biên
                float fadeStart = 1.0 - _FadeDistance;
                float fadeMask = smoothstep(fadeStart, 1.0, mask);
                
                // Tính toán độ mờ:
                // - Vùng chưa khám phá (mask = 0): che phủ hoàn toàn với _FogIntensity
                // - Vùng đã khám phá (mask = 1): mờ với _RevealedIntensity
                float fogAlpha = lerp(_FogIntensity, _RevealedIntensity, fadeMask);
                
                // Đọc texture chính
                float3 baseColor = tex2D(_MainTex, i.uv).rgb;
                
                // Kết hợp màu: vùng che phủ dùng _FogColor, vùng sáng dùng baseColor
                float3 finalColor = lerp(_FogColor.rgb, baseColor, fadeMask);
                
                // Tạo alpha dựa trên mask và fog intensity
                float alpha = fogAlpha * (1.0 - fadeMask * 0.5); // Vùng sáng có alpha thấp hơn
                
                fixed4 col = fixed4(finalColor, alpha);
                
                UNITY_APPLY_FOG(i.fogCoord, col);
                
                return col;
            }
            ENDCG
        }
    }
    
    FallBack "Transparent/Diffuse"
}
