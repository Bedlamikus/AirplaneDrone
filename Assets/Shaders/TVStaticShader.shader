Shader "Custom/TVStaticShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _NoiseScale ("Noise Scale", Float) = 100.0
        _NoiseSpeed ("Noise Speed", Float) = 1.0
        _NoiseIntensity ("Noise Intensity", Range(0, 1)) = 0.5
        _ScanlineFrequency ("Scanline Frequency", Float) = 200.0
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.3
        _CenterPosition ("Center Position", Vector) = (0, 0, 0, 0)
        _MinRadius ("Min Radius", Float) = 5.0
        _MaxRadius ("Max Radius", Float) = 20.0
        _PlayerPosition ("Player Position", Vector) = (0, 0, 0, 0)
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
                float3 worldPos : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _NoiseScale;
            float _NoiseSpeed;
            float _NoiseIntensity;
            float _ScanlineFrequency;
            float _ScanlineIntensity;
            float4 _CenterPosition;
            float _MinRadius;
            float _MaxRadius;
            float4 _PlayerPosition;
            
            // Функция для генерации случайного шума
            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }
            
            // Функция для генерации шума (простой вариант)
            float noise(float2 st)
            {
                float2 i = floor(st);
                float2 f = frac(st);
                
                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }
            
            // Функция для генерации телевизионной ряби (белые и черные точки)
            float tvStatic(float2 uv, float time)
            {
                // Создаем дискретный шум для эффекта телевизионной ряби
                float2 nuv = uv * _NoiseScale;
                nuv += time * _NoiseSpeed;
                
                // Генерируем случайные значения для каждого пикселя
                float noiseValue = random(floor(nuv) + time);
                
                // Делаем шум более резким (белые и черные точки)
                noiseValue = step(0.5, noiseValue);
                
                // Добавляем несколько слоев для более реалистичного эффекта
                float noise2 = step(0.5, random(floor(nuv * 2.0) + time * 0.7));
                float noise3 = step(0.5, random(floor(nuv * 4.0) + time * 0.5));
                
                // Комбинируем слои
                float combinedNoise = (noiseValue + noise2 * 0.5 + noise3 * 0.25) / 1.75;
                
                return combinedNoise;
            }
            
            // Функция для редких горизонтальных линий
            float scanlines(float2 uv)
            {
                // Создаем редкие горизонтальные линии
                float lineY = floor(uv.y * _ScanlineFrequency);
                float lineNoise = random(float2(lineY, _Time.y * 0.1));
                
                // Делаем линии редкими (только некоторые линии появляются)
                float scanlineValue = step(0.98, lineNoise) * _ScanlineIntensity;
                
                return scanlineValue;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Вычисляем расстояние от позиции игрока до центра
                float3 playerPos = _PlayerPosition.xyz;
                float3 centerPos = _CenterPosition.xyz;
                
                // Вычисляем расстояние от игрока до центра
                float distFromCenter = distance(playerPos, centerPos);
                
                // Вычисляем прозрачность на основе расстояния
                // На старте (близко к центру, dist < MinRadius) - максимально прозрачный (alpha = 0)
                // При удалении от центра (MinRadius <= dist <= MaxRadius) - alpha от 0 до 1
                // После MaxRadius - полностью непрозрачный (alpha = 1)
                float alpha = 0.0;
                if (distFromCenter < _MinRadius)
                {
                    // Близко к центру - полностью прозрачный
                    alpha = 0.0;
                }
                else if (distFromCenter > _MaxRadius)
                {
                    // Дальше максимального радиуса - полностью непрозрачный
                    alpha = 1.0;
                }
                else
                {
                    // Между MinRadius и MaxRadius - интерполируем от 0 до 1
                    alpha = saturate((distFromCenter - _MinRadius) / (_MaxRadius - _MinRadius));
                }
                
                // Генерируем телевизионную рябь (белые и черные точки)
                float staticNoise = tvStatic(i.uv, _Time.y);
                
                // Генерируем редкие горизонтальные линии
                float scanline = scanlines(i.uv);
                
                // Комбинируем рябь и линии
                float effect = staticNoise * _NoiseIntensity + scanline;
                
                // Создаем цвет телевизионной ряби (белые и черные точки)
                fixed3 staticColor = lerp(fixed3(0, 0, 0), fixed3(1, 1, 1), staticNoise);
                
                // Получаем основной цвет текстуры
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // Применяем телевизионную рябь к цвету
                col.rgb = lerp(col.rgb, staticColor, effect);
                
                // Добавляем горизонтальные линии
                col.rgb += scanline;
                col.rgb = saturate(col.rgb);
                
                // Применяем прозрачность на основе расстояния
                // Чем дальше от центра, тем меньше прозрачность (больше непрозрачность)
                col.a = alpha;
                
                return col;
            }
            ENDCG
        }
    }
    
    FallBack "Transparent/Diffuse"
}

