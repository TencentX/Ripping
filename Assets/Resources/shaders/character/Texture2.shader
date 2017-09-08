Shader "Custom/Texture2"     
{    
    Properties     
    {     
        _MainTex ("Base (RGB)", 2D) = "white" {}    
        _MainColor("MainColor",Color) = (1,1,1,1)  
        _Color("Color",Color) = (1,1,1,1)  
		_Min ("Min",Range(-10,0)) = -1
		_Max ("Max",Range(0,10)) = 1  
    }    
        
    SubShader     
    {    
        LOD 300    
        Tags { "Queue" = "Geometry" "RenderType"="Opaque" }
		
        Pass    
        {    
//        	Tags { "LightMode"="ForwardAdd" }
            ZWrite On  
            ZTest Less   
            Blend SrcAlpha OneMinusSrcAlpha 
  
            CGPROGRAM    
            #pragma vertex vert    
            #pragma fragment frag    
            sampler2D _MainTex;    
            float4 _MainTex_ST;  
            float4 _MainColor; 
                
            struct appdata {    
                float4 vertex : POSITION;    
                float2 texcoord : TEXCOORD0;    
            };    
              
            struct v2f  {    
                float4 pos : POSITION;    
                float2 uv : TEXCOORD0;    
            };    
              
            v2f vert (appdata v)   
            {    
                v2f o;    
                o.pos = mul(UNITY_MATRIX_MVP,v.vertex);    
                o.uv = v.texcoord;    
                return o;    
            }   
               
            float4 frag (v2f i) : COLOR    
            {    
                float4 texCol = tex2D(_MainTex, i.uv);    
                texCol = _MainColor * texCol;
                return texCol;    
            }    
            ENDCG    
        } 
    }   
    FallBack "Diffuse"   
}