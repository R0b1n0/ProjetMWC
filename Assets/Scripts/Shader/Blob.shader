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

            float _LightFactor;
            half4 _InnerColor;
            half4 _EdgeColor;
            int _innerRenderMethod;
            int _outerRenderMethod;
            float _auraF;
            float _auraRange;
            float _auraWidth;
            float _uvLengthFactor;
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

            //-------------------------Utils--------------------------------
            float SmoothUnionQuadraticPolynomial(float distA, float distB, float k)
            {
                float h = max(k - abs(distA - distB), 0.0) / k;
                return min(distA, distB) - h*h*k*(1.0/4.0);
            }
            float2 VecLerp(float2 start, float2 finish, float lerpValue)
            {
                return (finish - start) * lerpValue; 
            }
            half4 ColorLerp(half4 a, half4 b, float t)
            {
                return a + half4(b.x-a.x,b.y-a.y,b.z-a.z, 1) * t;
            }
            float Length(half2 vec)
            {
                return sqrt(vec.x*vec.x + vec.y*vec.y);
            }
            //-------------------------SDF--------------------------------
            float SDCircle(half2 inpoint, half2 inorigin, float inradius)
            {
                return distance(inpoint, inorigin) - inradius;
            }
            float GetCircleSd(float2 uv)
            {
                //Blend the two first circles 
                float sd = SmoothUnionQuadraticPolynomial(
                    SDCircle(
                        uv,
                        _Circles[0].xy,
                        _Circles[0].z
                        ),
                    SDCircle(
                        uv,
                        _Circles[1].xy,
                        _Circles[1].z
                        ),
                    BLEND_FACTOR
                    );
                //Blend any other circle 
                for (int i = 2; i <_CircleCount; i++)
                {
                    sd = SmoothUnionQuadraticPolynomial(
                        sd,
                        SDCircle(
                            uv,
                            _Circles[i].xy,
                            _Circles[i].z
                            ),
                        BLEND_FACTOR
                        );
                }

                return sd;
            }
            float SDSpikeCircle(half2 inpoint, half2 inorigin, float inradius )
            {
                float sd = SDCircle(inpoint,inorigin,inradius);

                float teta = atan(abs(inpoint.y - inorigin.y) / abs(inpoint.x - inorigin.x)) ;
                //return teta / 3.14/2;

                //float amplitube = smoothstep(0,1, min(Length(inpoint) - 0.5, 0.08 ));
                return cos(teta*70) + sd * 5 + 0.9/Length(inpoint);
            }

            //-------------------------Rendeeeeer--------------------------------
            half4 frag(v2f IN) : SV_Target
            { 
                //Center the coordinates
                half2 uv = (IN.positionHCS * 2 - _ScreenParams.xy)/_ScreenParams.x;

                float sd = GetCircleSd(uv);
                
                if (sd < 0) //Inner Blob
                {
                    if (_innerRenderMethod == 0)
                    {
                        //Edge/Inner 
                        if (abs(sd) > 0.01)
                            return _InnerColor ;
                        else
                            return _EdgeColor;
                    }
                    else if (_innerRenderMethod == 1)
                    {
                        //Toon like effect 
                        if (abs(sd) > 0.1)
                            return _InnerColor ;
                        else
                            return _EdgeColor;
                    }
                    else if (_innerRenderMethod == 2)
                    {
                        //Boring cell like effect
                        sd = smoothstep(0.0, 0.05,abs(sd) );
                        return sd * _InnerColor;
                    }
                    else if (_innerRenderMethod == 3)
                    {
                        //Weird trippy effect, kinda cool tho 
                        half4 color = half4(ColorLerp(_EdgeColor, _InnerColor,(abs(sd) * 10)%1.2 ).xyz,0) ;



                        return max(((1 - abs(sd * 30)) * _LightFactor) , 0) + color;

                        return color  ;
                    }
                    
                    return half4 (0,0,0,0);
                }   
                else //Outer Blob
                {
                    if (_outerRenderMethod == 0)
                    {
                        //Diffuse outline 
                        sd =  smoothstep(0.0, 0.2, sd); 
                        return sd * half4(1,1,1,0);
                    }
                    else if (_outerRenderMethod == 1)
                    {
                        //Dynamic diffuse outline, catching outside  
                        return half4(1,1,1,0) * (sd*2) / (pow(Length(uv),2) );
                    }
                    else if (_outerRenderMethod == 2)
                    {
                        //Dynamic diffuse
                        return ( half4(1,1,1,0) * pow((sd*2),2) / (pow(Length(uv),2)) );
                    }
                    else if (_outerRenderMethod == 3)
                    {
                        //Wave effect, surely that's what drug feels like
                        return abs(cos(10*(sd-_UnityTime/5)) * _EdgeColor);
                    }
                    else if (_outerRenderMethod == 4)
                    {
                        //Drugs, but cooler
                        return ColorLerp(
                        half4(0,0,0,0),
                        abs(cos(70*(sd - _UnityTime/5)) * _EdgeColor),
                        Length(sd));
                    }
                    else if (_outerRenderMethod == 5)
                    {
                        //Fade along the sd, weird af
                        return ColorLerp(
                        (cos(15*(sd-_UnityTime/5)) * _EdgeColor),
                        half4(0,0,0,0),
                        Length(sd));
                    }
                    else if (_outerRenderMethod == 6)
                    {
                        //Vibrating edges 
                        half4 bckg = half4(0,0,0,0);

                        if (sd < 0.1)
                            return ColorLerp(bckg,_EdgeColor,cos(500 * sd * (Length(uv) ))  );
                        else 
                            return bckg;
                    }
                    else if (_outerRenderMethod == 7)
                    {
                        //Vibrating edges 
                        half4 bckg = half4(0,0,0,0);

                        if (sd > 0.01 && sd < 0.05)
                            return ColorLerp(bckg,_EdgeColor,cos(400 * sd * (Length(uv)) - _UnityTime*5));
                        else 
                            return bckg;
                    }
                    else if (_outerRenderMethod == 8)
                    {
                        half4 bckg = half4(0,0,0,0);
                        float wave = cos(400 * sd * (Length(uv)) - _UnityTime*5);

                        return wave;

                        /*float spike = GetSpikeCircleSd(uv);
                        spike =  (1 - min(spike,1))  ;

                        if (wave < 0.5)
                            return spike ;
                            else return bckg;



                        if (sd > 0.01 && sd < 0.05)
                            return ColorLerp(bckg,_EdgeColor,  wave );
                        else 
                            return bckg;*/
                    }
                    else if (_outerRenderMethod == 9)
                    {
                        half4 bckg = half4(0,0,0,0);


                        float lerp = cos(200 * _auraF * sd * (Length(uv) * _uvLengthFactor) - _UnityTime*5) + _auraWidth;


                        half4 color = ColorLerp(bckg,_EdgeColor,lerp) + (_LightFactor * half4(1,1,1,0) * lerp);

                        //half4 dimedLight = max((-1*lerp) , 0) * ColorLerp(1, bckg, min(sd * 12, 1)) * _LightFactor;

                        //return dimedLight + ColorLerp(color, bckg, min(sd * 12, 1)) ;

                        return ColorLerp(color, bckg, min(sd * (100/_auraRange), 1)) ;


                        return (0.2 * length(uv)) /sd;
                    }

                    return half4(0,0,0,0);
                }
            }
            ENDHLSL
        }
    }
}
