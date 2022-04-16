// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unity Answers/InvertColor2"
{
	Properties
	{ 
		_MainTex("Alpha (A) only", 2D) = "white" {} 
		_AlphaCutOff("Alpha cut off", Range(0,1)) = 1 
		_AlphaTransparency("Alpha transparency", Range(0,1)) = 1 
		_Color ("Color (RGBA)", Color) = (1, 1, 1, 1)
	}

	SubShader
	{ 
		Tags { "Queue" = "Transparent+10" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Pass 
		{
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			Cull front 
			LOD 100

			CGPROGRAM

			#pragma vertex vert alpha
			#pragma fragment frag alpha

			#include "UnityCG.cginc"

			struct appdata_t 
			{
				float4 vertex   : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f 
			{
				float4 vertex  : SV_POSITION;
				half2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;

			v2f vert (appdata_t v)
			{
				v2f o;

				o.vertex     = UnityObjectToClipPos(v.vertex);
				v.texcoord.x = 1 - v.texcoord.x;
				o.texcoord   = TRANSFORM_TEX(v.texcoord, _MainTex);

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.texcoord) * _Color; // multiply by _Color
				return col;
			}

			ENDCG
		}

		Pass { 
			Fog { Mode Off } 
			Blend OneMinusDstColor Zero ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			float _AlphaCutOff;
			float _AlphaTransparency;

			struct appdata
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};
			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
			};
			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = float4(v.texcoord.xy, 0, 0);
				return o;
			}
			half4 frag(v2f i) : COLOR
			{
				half4 c = 1;
				c.a = (1 - (tex2D(_MainTex, i.uv.xy).a));
				clip(_AlphaCutOff - c.a);
				
				return c;
			}
			ENDCG
		}

	}

}