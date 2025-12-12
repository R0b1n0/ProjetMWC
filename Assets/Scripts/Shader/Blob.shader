Shader "Custom/Blob"
{
    
    Properties
    {
        //[MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white"

    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            
            HLSLPROGRAM //Shader code starts here

            //Define functions
            #pragma vertex vert
            #pragma fragment frag

            //Usefull stuff
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            //--------------------------------------Variables--------------------------------------------------
            #define MAX_CIRCLES 32
            #define BLEND_FACTOR 0.14

            float _UnityTime;
            int _CircleCount;
            float4 _Circles[MAX_CIRCLES];
            float _RotationSpeeds[MAX_CIRCLES];
            half4 _InnerColor;
            half4 _EdgeColor;
            //--------------------------------------DataStruct declaration--------------------------------------------------
            struct MeshData
            {
                //Per vertex data 
                float4 positionOS : POSITION; // vertex pos, world pos? idfk
                float2 uv : TEXCOORD0; //UV channels are there so you can just shove data in it, whatever it is 
            };
            struct v2f
            {
                //Vertex shader to frag shader 
                //Sometimes called interpolator, cuz that's how they're treated later on
                float4 positionHCS : SV_POSITION; //SV_POSITION = clip space position of the vertex
                float2 uv : TEXCOORD0;
            };

            //--------------------------------------Functions--------------------------------------------------
            v2f vert(MeshData IN)
            {
                v2f OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);//Local space to clip  space (MVP matrix multiplication)
                return OUT;
            }

            float SDCircle(half2 inpoint, half2 inorigin, float inradius)
            {
                return distance(inpoint, inorigin) - inradius;
            }
            float SmoothUnionQuadraticPolynomial(float distA, float distB, float k)
            {
                float h = max(k - abs(distA - distB), 0.0) / k;
                return min(distA, distB) - h*h*k*(1.0/4.0);
            }
            float2 VecLerp(float2 start, float2 finish, float lerpValue)
            {
                return (finish - start) * lerpValue; 
            }
            float2 GetCirclePos(float orbitRadius, float lerpPhase, float angleOffset, float rotationSpeed)
            {
                angleOffset = angleOffset * 3.14 / 180.0;
                float rotationDelta = (_UnityTime + angleOffset)*rotationSpeed;
                
                float2 start = normalize(float2(cos(rotationDelta), sin(rotationDelta))) * orbitRadius ;
                float2 finish = -start;
                
                return VecLerp(start,finish, cos(lerpPhase));
            }
            half4 ColorLerp(half4 a, half4 b, float t)
            {
                return a + half4(b.x-a.x,b.y-a.y,b.z-a.z, 0) * t;
            }
            float Length(half2 vec)
            {
                return sqrt(vec.x*vec.x + vec.y*vec.y);
            }

            //We work on fragments here, not vertices
            half4 frag(v2f IN) : SV_Target
            {
                //Center the coordinates
                half2 uv = (IN.positionHCS * 2 - _ScreenParams.xy)/_ScreenParams.x;

                float sd = 0;

                sd = SmoothUnionQuadraticPolynomial(
                    SDCircle(
                        uv,
                        GetCirclePos(_Circles[0].y,_Circles[0].z,_Circles[0].w, _RotationSpeeds[0]),
                        _Circles[0].x
                        ),
                    SDCircle(
                        uv,
                        GetCirclePos(_Circles[1].y,_Circles[1].z,_Circles[1].w, _RotationSpeeds[1]),
                        _Circles[1].x
                        ),
                    BLEND_FACTOR
                    );

                for (int i = 2; i <_CircleCount; i++)
                {
                    sd = SmoothUnionQuadraticPolynomial(
                        sd,
                        SDCircle(
                            uv,
                            GetCirclePos(_Circles[i].y,_Circles[i].z,_Circles[i].w, _RotationSpeeds[i]),
                            _Circles[i].x
                            ),
                        BLEND_FACTOR
                        );
                }
                
                if (sd < 0) //Inner Blob
                {
                    //Weird trippy effect, kinda cool tho 
                    //return half4(ColorLerp(_EdgeColor, _InnerColor,(abs(sd) * 10)%1 ).xyz,0) ;

                    //Edge/Inner 
                    /*if (abs(sd) > 0.01)
                        return _InnerColor ;
                    else
                        return _EdgeColor;*/

                    //Toon like effect 
                    if (abs(sd) > 0.1)
                        return _InnerColor ;
                    else
                        return _EdgeColor;

                    //Boring cell like effect
                    /*sd = smoothstep(0.0, 0.05,abs(sd) );
                    return sd * _InnerColor;*/
                }
                else //Outer Blob
                {
                    //Diffuse outline 
                    /*sd =  smoothstep(0.0, 0.2, sd); 
                    return sd * half4(1,1,1,0);*/

                    //Dynamic diffuse outline, catching outside  
                    //return half4(1,1,1,0) * (sd*2) / (pow(Length(uv),2) );

                    //Dynamic diffuse
                    return ( half4(1,1,1,0) * pow((sd*2),2) / (pow(Length(uv),2)) );
                    
                    //Wave effect, surely that's what drug feels like
                    //return abs(cos(10*(sd-_UnityTime/5)) * _EdgeColor);

                    //Drugs, but cooler
                    /*return ColorLerp(
                        half4(0,0,0,0),
                        abs(cos(70*(sd - _UnityTime/5)) * _EdgeColor),
                        Length(sd));*/

                    //Fade along the sd, weird af
                    /*return ColorLerp(
                        (cos(15*(sd-_UnityTime/5)) * _EdgeColor),
                        half4(0,0,0,0),
                        Length(sd));*/

                    //Vibrating edges 
                    half4 bckg = half4(0,0,0,0);

                    if (sd < 0.1)
                        return ColorLerp(bckg,_EdgeColor,cos(500 * sd * (Length(uv) ))  );
                    else 
                        return bckg;
                }
            }
            ENDHLSL
        }
    }
}
