Shader "UI/CircleWipe"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Color", Color) = (0,0,0,1)
        _Progress ("Progress", Range(0,1)) = 0
        _Softness ("Softness", Range(0.001,0.5)) = 0.05
        _Aspect ("Aspect (w/h)", Float) = 1.777
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };

            fixed4 _Color;
            float _Progress, _Softness, _Aspect;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 p = i.uv - 0.5;
                p.x *= _Aspect;                                   // 縦横比補正で真円にする
                float maxR = length(float2(0.5 * _Aspect, 0.5));  // 中心→角の距離
                float r = _Progress * (maxR + _Softness);         // 進行度0〜1で画面全体を覆い切る
                float a = 1.0 - smoothstep(r - _Softness, r, length(p));
                return fixed4(_Color.rgb * i.color.rgb, _Color.a * i.color.a * a);
            }
            ENDCG
        }
    }
}
