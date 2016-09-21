// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/HeightWarp" {
    Properties {
        _MainTex ("Texture Image", 2D) = "white" {} 
        _EffectScale ("Effect Scale", float) = 0.5
    }
    SubShader {
        Pass {
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
 
            #pragma vertex vert  
            #pragma fragment frag 
            
            #include "UnityCG.cginc"
            
            #pragma multi_compile_fwdbase
            
            #include "AutoLight.cginc"
            
            uniform sampler2D _MainTex;    
            uniform float4 _MainTex_ST;
            uniform float _EffectScale;
 
            struct vertexInput {
                float4 vertex : POSITION;
                float4 texcoord0 : TEXCOORD0;
                float3 normal : NORMAL;
            };
            
            struct fragmentInput {
                float4 pos : SV_POSITION;
                // Removing and packing the scale value into the z component of texcoord0
                // float scaled : PSIZE;
                float4 texcoord0 : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 lightDir : TEXCOORD2;
                LIGHTING_COORDS(3, 4)
            };
 
            fragmentInput vert(appdata_base input) {
                fragmentInput output;
 
                output.pos = mul(UNITY_MATRIX_MVP, input.vertex);
                output.texcoord0 = input.texcoord;
                
                
                // get world position of vertex
                float4 worldPosition = mul(unity_ObjectToWorld, input.vertex);

                // scale this value from 0...1?
                // Packing the scale value into the z component of the texcoord since we're not using
                // 3D texture lookups
                output.texcoord0.z = worldPosition.y / 10.0;
                
                output.normal = normalize(input.normal);
                output.lightDir = normalize(ObjSpaceLightDir(input.vertex));
                                
                TRANSFER_VERTEX_TO_FRAGMENT(output);
                
                return output;
            }
 
            float4 frag(fragmentInput input) : COLOR {
                float2 amount = (input.texcoord0.xy - 0.5);
                input.texcoord0.xy += input.texcoord0.z * amount * _EffectScale;
                float4 diffuse = tex2D(_MainTex, input.texcoord0.xy * _MainTex_ST.xy);
                
                // lighting
                // http://gamasutra.com/blogs/JoeyFladderak/20140416/215612/Let_there_be_shadow.php?print=1
                float3 L = normalize(input.lightDir);
                float3 N = normalize(input.normal);  
                float attenuation = LIGHT_ATTENUATION(input);
                float4 ambient = UNITY_LIGHTMODEL_AMBIENT * 2;
                float NdotL = saturate(dot(N, L));
                float4 diffuseTerm = NdotL * attenuation;

                float4 finalColor = (ambient + diffuseTerm) * diffuse;
                
                return finalColor;
                //return tex2D(_MainTex, input.texcoord0.xy * _MainTex_ST.xy) * attenuation;
            }
 
            ENDCG
        }
    }
    FallBack "VertexLit"
}
