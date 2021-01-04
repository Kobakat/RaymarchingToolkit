Shader "RaymarchToolkit/Raymarch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            sampler2D _MainTex;
            uniform sampler2D _CameraDepthTexture;
            uniform float4x4 _Frustum;
            uniform float4x4 _CamMatrix;
            uniform float4 _MainColor;
            uniform float3 _Light;

            //How many times each ray is marched
            //Higher values give higher resolution (and potentially longer draw distances) but lower performance
            static const int maxSteps = 100;

            //How close does a ray have to get to a shape surface to be considered a hit
            //Lower values give a sharper definition of shape but lower performance
            static const float epsilon = 0.001;

            //The maximum distance we want a ray to be from the nearest surface before giving up
            //Higher values give a longer draw distance but lower performance
            static const float maxDist = 100;

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

            //Describes the distance from a sphere centered on P
            float sdSphere(float3 p, float r)
            {
                return length(p) - r;
            }

            //Returns the distance to the raymarched shapes surface
            float SurfaceDistance(float3 p)
            {
                return(sdSphere(p, 1));
            }

            //For a signed distances field, the normal of any given point is defined as the gradient of the distance field
            //As such, subtracting the distance field of a slight smaller value by a slight large value produces a good approximation
            //This function is exceptionally expensive as it requires 6 more calls of a sign distance function PER PIXEL hit
            float3 CalculateNormal(float3 p)
            {

                float x = SurfaceDistance(float3(p.x + epsilon, p.y, p.z)) - SurfaceDistance(float3(p.x - epsilon, p.y, p.z));
                float y = SurfaceDistance(float3(p.x, p.y + epsilon, p.z)) - SurfaceDistance(float3(p.x, p.y - epsilon, p.z));
                float z = SurfaceDistance(float3(p.x, p.y, p.z + epsilon)) - SurfaceDistance(float3(p.x, p.y, p.z - epsilon));

                return normalize(float3(x, y, z));
            }

            //For each pixel on the screen
            fixed4 raymarch(ray r)
            {
                //Start with a completely transparent pixel
                fixed4 pixelColor = fixed4(0, 0, 0, 0);
                //Cast out a ray at the pixel's UV coordinate
                float dst = 0;

                //For a maximum of <maxStep> times,
                for (int i = 0; i < maxSteps; i++)
                {
                    //Determine the distance from the nearest shape in the scene
                    r.position = r.origin + r.direction * dst;
                    float surfDist = SurfaceDistance(r.position);

                    
                    //If the distance is sufficently small...
                    if (surfDist < epsilon)
                    {
                        //We "hit" the surface. Calculate the normal vector of the pixel and shade it based on the angle from the rays of light
                        float3 n = CalculateNormal(r.position);

                        //This uses the lambertian model of lighting https://en.wikipedia.org/wiki/Lambertian_reflectance
                        float l = dot(-_Light.xyz, n);
                        

                        //Color the pixel using the "color" control variable attached to the rendering camera as its base
                        pixelColor = fixed4(_MainColor.rgb * l, 1);

                        break;
                    }

                    //If the distance is not sufficently small, we missed.
                    //Move the ray's position forward and try again
                    dst += surfDist;


                    //If the distance is very large or the camera renders a standard polygonal mesh first we give up and break early
                    if (dst > maxDist || dst >= r.depth)
                        break;
                }

                //Give the frag function the color we want the pixel to be
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
                o.ray /= abs(o.ray.z);

                o.ray = mul(_CamMatrix, o.ray);
                return o;
            }


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
