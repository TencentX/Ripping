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
		
		// 第一次绘制，处理被物体遮挡住的角色部分
        Pass  
        {  
            Blend SrcAlpha OneMinusSrcAlpha 
			AlphaToMask On  
            ZWrite On  
            ZTest Greater

  
            CGPROGRAM  
            #pragma vertex vert  
            #pragma fragment frag  
            #include "UnityCG.cginc"  
  
            float4 _Color;  
            sampler2D _MainTex;    
            float4 _MainTex_ST;  
			fixed _Min;
			fixed _Max;
              
            struct appdata_t {  
                float4 vertex : POSITION;  
                float2 texcoord : TEXCOORD0;  
                float4 color:COLOR;  
                float4 normal:NORMAL;  
            };  
  
            struct v2f {  
                float4  pos : SV_POSITION;  
                float4  color:COLOR;  
				float2 uv : TEXCOORD0;  
            } ;  
            v2f vert (appdata_t v)  
            {  
                v2f o;  
				o.uv = v.texcoord; 
				
				if(v.vertex.x <= _Max && v.vertex.x >= _Min)
				{
					o.color = _Color;
				}
				else
				{
					o.color = fixed4(1,1,1,1);
				}
                o.pos = mul(UNITY_MATRIX_MVP,v.vertex);
                return o;  
            }  
            float4 frag (v2f i) : COLOR  
            {  
				if(i.color.a <= _Color.w)
				{
					discard;
				}
                return i.color * tex2D(_MainTex,i.uv);   
            }  
            ENDCG  
        }  
		// 第二次绘制，显示未被遮挡的部分
        Pass    
        {    
            ZWrite On  
            ZTest Less   
  
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