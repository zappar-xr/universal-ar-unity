Shader "Zappar/UnlitTextureUV"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("Texture", 2D) = "white" {}
        [Toggle(FlipTexU)]
        _FlipTexX ("Flip U", float) = 0
        [Toggle(FlipTexV)]
        _FlipTexY ("Flip V", float) = 0
        [Enum(Off,0,Front,1,Back,2)] _Culling("Cull", Int) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull[_Culling]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature FlipTexU
            #pragma shader_feature FlipTexV

            struct appdata
            {
                float4 vertex : POSITION;   // vertex position
                float2 uv : TEXCOORD0;      // texture coordinate
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;      // texture coordinate
                float4 vertex : SV_POSITION;    // clip space position
            };

            sampler2D _MainTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                #if FlipTexU
                o.uv = float2(1-o.uv[0],o.uv[1]);
                #endif
                #if FlipTexV
                o.uv = float2(o.uv[0],1-o.uv[1]);
                #endif
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
