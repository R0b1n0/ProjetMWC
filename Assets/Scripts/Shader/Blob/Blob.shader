Shader "Custom/Blob"
{
    
    Properties
    {
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

            #pragma shader_feature _ENABLE_DEBUGVIEW

            //Usefull stuff
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            //--------------------------------------Render Variables--------------------------------------------------
            #define MAX_CIRCLES 32
            //#define BLEND_FACTOR 0.14
            #define BLEND_FACTOR 0.05
            float _UnityTime;

            int _CircleCount;
            float4 _Circles[MAX_CIRCLES];
            float4 _CirclesColors[MAX_CIRCLES];

            float _LightFactor;
            half4 _InnerColor;
            half4 _EdgeColor;
            half4 bckg = half4(0,0,0,0);

            float _auraF;
            float _auraRange;
            float _auraWidth;
            float _uvLengthFactor;
            float _auraOffset;

            float _xOffset;
            float _yOffset;
            float _lightSdScale;
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
            struct SdfResult
            {
                float sd;
                float alpha;
                half4 color;
            };

            //--------------------------------------Functions--------------------------------------------------
            v2f vert(MeshData IN)
            {
                v2f OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);//Local space to clip  space (MVP matrix multiplication)
                return OUT;
            }

            //-------------------------Utils--------------------------------
            float2 SmoothUnionQuadraticPolynomialBlend(float a, float b, float k)
            {
                //X is the actual sdf value
                //Y is a lerp value that is used for color blending
                float h = 1.0 - min( abs(a-b)/(4.0*k), 1.0 );
                float w = h*h;
                float m = w*0.5;
                float s = w*k;
                return (a<b) ? float2(a-s,m) : float2(b-s,1.0-m);
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
            
            SdfResult GetCircleSdf(float2 uv)
            {
                SdfResult result;
                
                //Blend the two first circles 
                float2 sd = SmoothUnionQuadraticPolynomialBlend(
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
                float alpha = lerp(_Circles[0].w, _Circles[1].w, sd.y);
                half4 color = ColorLerp(_CirclesColors[0], _CirclesColors[1], sd.y );

                //Blend any other circle 
                for (int i = 2; i <_CircleCount; i++)
                {
                    sd = SmoothUnionQuadraticPolynomialBlend(
                        sd.x,
                        SDCircle(
                            uv,
                            _Circles[i].xy,
                            _Circles[i].z
                            ),
                        BLEND_FACTOR
                        );
                    alpha = lerp(alpha, _Circles[i].w, sd.y);
                    color = ColorLerp(color, _CirclesColors[i], sd.y );
                }
                result.sd = sd.x;
                result.alpha = alpha;
                result.color = color;
                return result;
            }

            //-------------------------Rendeeeeer--------------------------------
            half4 frag(v2f IN) : SV_Target
            {
                //Center the coordinates
                half2 uv = (IN.positionHCS * 2 - _ScreenParams.xy)/_ScreenParams.x;
                SdfResult sdf = GetCircleSdf(uv);
                
                if (sdf.sd < 0) //Inner Blob
                {
                    half4 color = half4(ColorLerp(sdf.color, _InnerColor,(abs(sdf.sd) * 10)%1.2 ).xyz,0) ;
                    half4 innerColor = max(((1 - abs(sdf.sd * 30)) * _LightFactor) , 0) + color;

                    return ColorLerp(bckg, innerColor, sdf.alpha);
                }   
                else //Outer Blob
                {
                    float lightValue = (1/(sdf.sd * _lightSdScale + _xOffset) - _yOffset) * _LightFactor * length(uv);

                    //This line was here to make the aura fade with UV length, but it messes the whole marble aura waves so...
                    //float lerp = cos(200 * _auraF * sd * (Length(uv) * _uvLengthFactor) - (_auraOffset)) + _auraWidth;
                    float lerp = cos(200 * _auraF * sdf.sd  - (_auraOffset)) + _auraWidth;
                    half4 color = ColorLerp(bckg,sdf.color,lerp) + (_LightFactor * half4(1,1,1,0) * lerp);
                    half4 outerColor = max(ColorLerp(color, bckg, min(sdf.sd * (100/_auraRange), 1)), lightValue/2);
                    return ColorLerp(bckg, outerColor, sdf.alpha);
                }
            }
            ENDHLSL
        }
    }
}
