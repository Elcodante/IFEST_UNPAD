Shader "UI/CCTV_Screen_Overlay"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Screen Curvature)]
        _Curvature ("Curvature (Makin kecil makin cembung)", Range(2.0, 15.0)) = 5.5
        _VignetteStrength ("Vignette Darkness", Range(0.2, 1.5)) = 0.85
        _VignetteSmoothness ("Vignette Smoothness", Range(0.1, 1.0)) = 0.45

        [Header(Curved Scanlines)]
        _ScanlineDensity ("Scanline Density", Float) = 450.0
        _ScanlineSpeed ("Scanline Speed", Float) = 1.0
        _ScanlineStrength ("Scanline Darkness", Range(0, 0.5)) = 0.16

        [Header(Noise and Glitches)]
        _NoiseStrength ("Grain / Noise", Range(0, 0.2)) = 0.035
        _GlitchSpeed ("Rolling Bar Speed", Float) = 0.25
        _GlitchStrength ("Rolling Bar Glitch", Range(0, 0.4)) = 0.12

        [Header(Intermittent Flicker)]
        _FlickerStrength ("Flicker Intensity", Range(0, 0.2)) = 0.05
        _FlickerInterval ("Flicker Interval (Seconds)", Float) = 4.0
        _FlickerDuration ("Flicker Burst Duration (Sec)", Float) = 0.10

        [Header(Tint)]
        _TintDarkness ("Dark Tint Opacity", Range(0, 0.3)) = 0.05
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _Curvature;
            float _VignetteStrength;
            float _VignetteSmoothness;
            float _ScanlineDensity;
            float _ScanlineSpeed;
            float _ScanlineStrength;
            float _NoiseStrength;
            float _GlitchSpeed;
            float _GlitchStrength;
            float _FlickerStrength;
            float _FlickerInterval;
            float _FlickerDuration;
            float _TintDarkness;

            float random(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233)) + _Time.y * 10.0) * 43758.5453);
            }

            float2 CurveScreen(float2 uv)
            {
                uv = uv * 2.0 - 1.0;
                float2 offset = abs(uv.yx) / _Curvature;
                uv = uv + uv * offset * offset;
                uv = uv * 0.5 + 0.5;
                return uv;
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = CurveScreen(i.texcoord);

                // Bingkai bezel tabung CRT melengkung di sudut layar
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                {
                    return fixed4(0, 0, 0, 1.0);
                }

                // 1. Scanlines
                float scan = sin((uv.y + _Time.y * _ScanlineSpeed * 0.02) * _ScanlineDensity);
                scan = (scan + 1.0) * 0.5;
                float scanDarkness = scan * _ScanlineStrength;

                // 2. Rolling Glitch Bar
                float barPos = frac(_Time.y * _GlitchSpeed);
                float barDist = abs(uv.y - barPos);
                float glitchBar = smoothstep(0.12, 0.0, barDist) * _GlitchStrength;

                // 3. Noise / Grain
                float noise = (random(uv) * 2.0 - 1.0) * _NoiseStrength;

                // 4. Intermittent Flicker
                float safeInterval = max(0.1, _FlickerInterval);
                float isFlickering = step(frac(_Time.y / safeInterval), _FlickerDuration / safeInterval);
                float flicker = (random(float2(_Time.y * 30.0, 0)) * 2.0 - 1.0) * _FlickerStrength * isFlickering;

                // 5. Vignette
                float dist = length(uv - 0.5);
                float vignette = smoothstep(_VignetteStrength - _VignetteSmoothness, _VignetteStrength, dist) * 0.75;

                // Total Transparansi
                float totalAlpha = clamp(scanDarkness + glitchBar + vignette + _TintDarkness + abs(noise) + abs(flicker), 0.0, 0.95);

                // Warna gelap CRT
                float3 darkColor = float3(0.01, 0.03, 0.01) + noise;

                return fixed4(darkColor, totalAlpha * i.color.a);
            }
            ENDCG
        }
    }
}