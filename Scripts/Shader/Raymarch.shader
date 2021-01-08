Shader "RaymarchToolkit/Raymarch"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }

    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "SignedDistanceFunctions.cginc"
            #include "OperationFunctions.cginc"
            #include "LightingFunctions.cginc"
            #include "FilterFunctions.cginc"

            sampler2D _MainTex;
            uniform float4x4 _Frustum;
            uniform float4x4 _CamMatrix;
            uniform float3 _Light;
            uniform int _OperationCount;
            uniform bool _UseLight;
            uniform bool _DarkMode;

            //Filter           
            uniform int _Filter;
            uniform int _Highlight;
            uniform int _HighlightGradient;
            uniform float _HighlightStrength;
            uniform float _NonHighlightStrength;
            uniform float3 _EmissiveColor;

            //Light
            uniform int _LightMode;
            uniform float _LitMultiplier;
            uniform float _UnlitMultiplier;
            uniform bool _CustomAngle;
            uniform float _FlipAngle;

            struct shape
            {
                float3 position;
                int shape;
                float3 color;

                float sphereRadius;

                float3 boxDimensions;

                float3 roundBoxDimensions;
                float roundBoxFactor;

                float torusInnerRadius;
                float torusOuterRadius;

                float coneHeight;
                float2 coneRatio;
            };

            struct operation
            {
                int operation;
                int childCount;

                float blendStrength;
            };

            StructuredBuffer<operation> operations;
            StructuredBuffer<shape> shapes;

            //How many times each ray is marched
            //Higher values give higher resolution (and potentially longer draw distances) but lower performance
            static const int maxSteps = 100;

            //How close does a ray have to get to be consider a hit
            //Higher values give a sharper definition of shape but lower performance
            static const float epsilon = 0.001;

            //The maximum distance we want a ray to be from the nearest surface before giving up
            //Higher values give a longer draw distance but lower performance
            static const float maxDist = 40;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 ray : TEXCOORD1;
            };

            struct ray
            {
                float3 origin;
                float3 direction;
                float3 position;
                float depth;
            };

            float4 GetShape(float3 p, int index)
            {
                shape s = shapes[index];

                s.position = p - s.position;

                float3 col = s.color;
                float dst = 1;

                switch (s.shape)
                {
                    case 0:
                        dst = sdSphere(s.position, s.sphereRadius);
                        break;
                    case 1:
                        dst = sdBox(s.position, s.boxDimensions);
                        break;
                    case 2:
                        dst = sdTorus(s.position, s.torusInnerRadius, s.torusOuterRadius);
                        break;
                    case 3:
                        dst = sdCone(s.position, s.coneRatio, s.coneHeight);
                        break;
                    case 4:
                        dst = sdRoundBox(s.position, s.roundBoxDimensions, s.roundBoxFactor);
                        break;
                }

                return float4(col, dst);
            }

            float4 GetOperation(float3 p, int index)
            {
                operation o = operations[index];

                int startIndex = 0;


                for (int i = 0; i < index; i++)
                {
                    startIndex += operations[i].childCount;
                }

                float4 shapeValue = GetShape(p, startIndex);

                for (int j = 1; j < o.childCount; j++)
                {
                    switch (o.operation)
                    {
                        case 0:
                            shapeValue = opAdd(shapeValue, GetShape(p, startIndex + j));
                            break;
                        case 1:
                            shapeValue = opSubtract(shapeValue, GetShape(p, startIndex + j));
                            break;
                        case 2:
                            shapeValue = opIntersect(shapeValue, GetShape(p, startIndex + j));
                            break;
                        case 3:
                            shapeValue = opBlend(shapeValue, GetShape(p, startIndex + j), o.blendStrength);
                            break;
                    }
                }

                return shapeValue;
            }


            float4 SurfaceDistance(float3 p)
            {
                float4 surfValue = GetOperation(p, 0);

                for (int i = 1; i < _OperationCount; i++)
                {
                    surfValue = opAdd(surfValue, GetOperation(p, i));
                }

                return surfValue;
            }

            //For a signed distances field, the normal of any given point is defined as the gradient of the distance field
            //As such, subtracting the distance field of a slight smaller value by a slight large value produces a good approximation
            //This function is exceptionally expensive as it requires 6 more calls of a sign distance function PER PIXEL hit
            float3 CalculateNormal(float3 p)
            {

                float x = SurfaceDistance(float3(p.x + epsilon, p.y, p.z)).w - SurfaceDistance(float3(p.x - epsilon, p.y, p.z)).w;
                float y = SurfaceDistance(float3(p.x, p.y + epsilon, p.z)).w - SurfaceDistance(float3(p.x, p.y - epsilon, p.z)).w;
                float z = SurfaceDistance(float3(p.x, p.y, p.z + epsilon)).w - SurfaceDistance(float3(p.x, p.y, p.z - epsilon)).w;

                return normalize(float3(x,y,z));
            }

            fixed4 raymarch(ray r)
            {
                fixed4 pixelColor = fixed4(0, 0, 0, 0);

                if (_DarkMode)
                    pixelColor = fixed4(0, 0, 0, 1);

                float dst = 0;

                for (int i = 0; i < maxSteps; i++)
                {
                    r.position = r.origin + r.direction * dst;
                    float4 surf = SurfaceDistance(r.position);

                    if (surf.w < epsilon)
                    {
                        float light = 1;
                        float3 n = float3(1, 1, 1);

                        if (_UseLight)
                        {
                            n = CalculateNormal(r.position);

                            switch (_LightMode)
                            {
                                case 0:
                                    light = Lambertian(_Light, n);
                                    break;
                                case 1:
                                    if (_CustomAngle)
                                        light = CelShade(_Light, n, _LitMultiplier, _UnlitMultiplier, _FlipAngle);
                                    else
                                        light = CelShade(_Light, n, _LitMultiplier, _UnlitMultiplier);
                                    break;
                            }
                        }

                        switch (_Filter)
                        {
                            case 0:
                                pixelColor = NoFilter(light, surf.rgb);
                                break;
                            case 1:
                                pixelColor = Highlight(light, surf.rgb, i, _Highlight, _HighlightGradient, _NonHighlightStrength, _HighlightStrength, _EmissiveColor);
                                break;
                        }

                        return pixelColor;
                    }

                    //If the distance is not sufficently small, we missed.
                    //Move the ray's position forward and try again
                    dst += surf.w;


                    ///If the distance is very large or the camera renders a standard polygonal mesh first we give up and break early
                    if (dst > maxDist || dst >= r.depth)
                        break;
                }

                return pixelColor;

            }

            v2f vert(appdata v)
            {
                v2f o;

                half index = v.vertex.z;
                v.vertex.z = 0;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                o.ray = _Frustum[(int)index].xyz;

                //Normalize along the z-axis
                //Absval function prevents scene from inverting
                o.ray /= abs(o.ray.z);

                //Places ray in worldspace so the depth buffer is calculated properly
                o.ray = mul(_CamMatrix, o.ray);
                return o;
            }

            uniform sampler2D _CameraDepthTexture;

            //Runs for every pixel on the screen
            fixed4 frag(v2f i) : SV_Target
            {
                //Blend the new rendered images with the previous scene view

                ray r;
                r.direction = normalize(i.ray.xyz);
                r.origin = _WorldSpaceCameraPos;

                r.depth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv).r);
                r.depth *= length(i.ray.xyz);

                //Previous scene view color
                fixed3 base = tex2D(_MainTex, i.uv);

                //New color obtained from raymarching
                fixed4 col = raymarch(r);

                //Alpha Blend
                return fixed4(base * (1.0 - col.w) + col.xyz * col.w, 1.0);
            }

            ENDCG
        }

    }
}


