#version 400

#define NUM_MATERIALS 8
#define NUM_SPHERES         4
#define NUM_PLANES          1
// soft shadows parameters
//#define SOFT_SHADOW_STEPS 20    // level of soft shadow detail
#define SUN_RADIUS 0.1
// depth of field parameters
#define FOCUS_STEPS 20    // level of blur quality
#define FOCUS_BLUR 0.1    // use lower when using higher number of FOCUS_STEPS and vice versa

#define PI 3.1415926536
#define M_1_PI     0.318309886183790671538
#define M_1_PI_2    M_1_PI * 0.5

uniform float iGlobalTime;
uniform int iWidth;
uniform int iHeight;
uniform int iMouseX;
uniform int iMouseY;
uniform int iTexturedMode;
uniform float iFocusDistance;
uniform int iSoftShadowMode;
uniform int iNumberBounces;
uniform int iDOFMode;

uniform sampler2D map;
uniform sampler2D tex;

int softShadowSteps;
//#define SOFT_SHADOW_STEPS iSoftShadowMode ? 30:1


struct Ray
{
    vec3 origin;
    vec3 direction;
};

struct Plane
{
    float offset;
    vec3 N;
    int material;
};

struct Sphere
{
    float   radius;
    vec3    position;
    int     material;
};

struct AABox
{
    vec3 scale;
    vec3 position;
    int material;
};

struct Material
{
    vec3 diffuse;
    vec3 specular;    
};

struct Hit
{
    float t;    // ray solution
    vec3 n;     // normal
    int m;      // material
};

const Hit noHit = Hit(1e10, vec3(0.0), -1);

Sphere      spheres[NUM_SPHERES];           //TODO
Plane       planes[NUM_PLANES];             //TODO

/// <summary>
/// Compute sphere's normal at intersection point.
/// </summary>
/// <param name="pos">position</param>
/// <param name="s">sphere</param>
/// <return>normal</return>
vec3 nSphere(in vec3 pos, in Sphere s)
{
    return normalize(pos - s.position);
}

/// <summary>
/// Ray-Plane intersection test.
/// </summary>
/// <param name="p">plane</param>
/// <param name="r">ray</param>
Hit intersectPlane(Plane p, Ray r)
{
    float NdotD = -dot(p.N, r.direction);
    
    //plane wasn't hit
    if (NdotD < 0.0)
    {
        return noHit;
    }

    float t = (p.offset + dot(p.N, r.origin)) / NdotD;
    
    return Hit(t, p.N, p.material);
}

bool isInside(vec2 a, vec2 b)
{
    return a.x < b.x && a.y < b.y;
}

void AAboxPlaneIntersection(vec3 o, vec3 d, vec3 s, inout float t, out float ndir)
{
    ndir = 0.0;
    if (d.x != 0.)
    {
        float tmin = (-0.5 * s.x - o.x) / d.x;
        if (tmin >= 0. && tmin < t && isInside(abs(o.yz + tmin * d.yz), 0.5 * s.yz))
        {
            t = tmin;
            ndir = -1.;
        }

        float tmax = (0.5 * s.x - o.x) / d.x;
        if (tmax >= 0. && tmax < t && isInside(abs(o.yz + tmax * d.yz), 0.5 * s.yz))
        {
            t = tmax;
            ndir = 1.;
        }
    }
}
    
Hit intersectBox(AABox b, Ray r)
{
    Hit hit = noHit;
    vec3 ro = r.origin - b.position;
    float ndir = 0.0;

    AAboxPlaneIntersection(ro.xyz, r.direction.xyz, b.scale.xyz, hit.t, ndir);
    if (ndir != 0.0)
    {
        hit.n = vec3(ndir, 0.0, 0.0);
        hit.m = b.material;
    }

    AAboxPlaneIntersection(ro.yzx, r.direction.yzx, b.scale.yzx, hit.t, ndir);
    if (ndir != 0.0)
    {
        hit.n = vec3(0.0, ndir, 0.0);
        hit.m = b.material;
    }

    AAboxPlaneIntersection(ro.zxy, r.direction.zxy, b.scale.zxy, hit.t, ndir);
    if (ndir != 0.0){
        hit.n = vec3(0.0, 0.0, ndir);
        hit.m = b.material;
    }

    return hit;
}

/// <summary>
/// Ray-sphere intersection.
/// </summary>
/// <param name="s">sphere</param>
/// <param name="r">ray</param>
Hit iSphere(Sphere s, Ray r)
{
    vec3 origin = r.origin - s.position;
    
    float b = dot(origin, r.direction);
    float c = dot(origin, origin) - pow(s.radius, 2.0);
    float h = b*b - c;

    if (h < 0.0)
    {
        return noHit;
    }
    
    float t = (-b - sqrt(h));
    
    if (t < 0.0) return noHit;
    
    return Hit(t, (r.origin + t*r.direction - s.position) / s.radius, s.material);
}

/// <summary>
/// Ray-Plane intersection test.
/// </summary>
/// <param name="p">plane</param>
/// <param name="r">ray</param>
/// <return>ray solution, plane normal and material</return>
Hit iPlane(Plane p, Ray r)
{
    float t = (p.offset - r.origin.y) / r.direction.y;

    if(t > 0.0 )
        return Hit(t, p.N, p.material);
    else
        return noHit;
}

Hit intersectScene(Ray r)
{
    Sphere s1 = Sphere(1.5, vec3(-3.5, 1.5, 1.0), 0);
    Sphere s2 = Sphere(1.8, vec3(-1., 4.8, 0.2), 3);
    Sphere s3 = Sphere(0.8, vec3(1.0, 0.8, 2.8), 4);
    Sphere s4 = Sphere(2.2, vec3(7.5, 2.2, 1.0), 7);
    //Sphere s5 = Sphere(1.2, vec3(cos(iGlobalTime)*10, 1.2, cos(iGlobalTime)*10.0), 4);

    Plane p = Plane(0.0, vec3(0.0, 1.0, 0.0), 1);

    spheres[0] = s1;
    spheres[1] = s2;
    spheres[2] = s3;
    spheres[3] = s4;

    planes[0] = p;
    
    AABox b = AABox(vec3(3.0, 3.0, 3.0), vec3(0.0, 1.5, 0.0), 2);

    Hit hit = Hit(1e5, vec3(0.), -1);

    Hit hitp = iPlane(p, r);
    if (hitp.m != -1 && hitp.t < hit.t) { hit = hitp; }

    Hit hits1 = iSphere(s1, r);
    if (hits1.m != -1 && hits1.t < hit.t) { hit = hits1; }
    
    Hit hits2 = iSphere(s2, r);
    if (hits2.m != -1 && hits2.t < hit.t) { hit = hits2; }
    
    Hit hits3 = iSphere(s3, r);
    if (hits3.m != -1 && hits3.t < hit.t) { hit = hits3; }
    
    Hit hits4 = iSphere(s4, r);
    if (hits4.m != -1 && hits4.t < hit.t) { hit = hits4; }

    //Hit hits5 = iSphere(s5, r);
    //if (hits5.m != -1 && hits5.t < hit.t) { hit = hits5; }
    
    Hit hitb = intersectBox(b, r);
    if (hitb.m != -1 && hitb.t < hit.t) { hit = hitb; }

    return hit;
}

Material materials[NUM_MATERIALS];

Material getMaterial(Hit hit)
{
    // FIXME: what would be the correct way to do this?
    Material m = Material(vec3(0.0), vec3(0.0));
    for (int i = 0; i < NUM_MATERIALS; ++i)
    {
        if (i == hit.m) m = materials[i];
    }
    return m;
}

vec3 sunCol = vec3(1e3);
vec3 sunDir = normalize(vec3(cos(iGlobalTime*0.9),sin(iGlobalTime/1.6),sin(iGlobalTime)));//*vec3(.6,1.0,.6)+vec3(0,.2,0);

/// <summary>
/// Determine sky color based on sun's position.
/// </summary>
/// <param name="d">ray direction</param>
/// <return>sky color</return>
vec3 skyColor(vec3 d)
{
    float transition = pow(smoothstep(0.8, 0.5, d.y), 0.4);

    vec3 sky = 1e2*mix(vec3(1), vec3(0.12, 0.43, 1), transition);
	//vec3 sky = 1e2*mix(vec3(1), vec3(0.02, 0.03, 0.05), transition);
    vec3 sun = vec3(2e10) * pow(abs(dot(d, sunDir)), 5e4);

    return sky + sun;
}

/// <summary>
/// Fresnel-Schlick approximation for reflection.
/// </summary>
/// <param name="spec">specular color of material</param>
/// <param name="E">vector 1</param>
/// <param name="H">vector 2</param>
vec3 fresnel(vec3 spec, vec3 E, vec3 H)
{
    return spec + (1.0 - spec) * pow(1.0 - clamp(dot(E, H), 0.0, 1.0), 5.0);
}

/// <summary>
/// Pseudo-random number generator. Returns number from interval <0,1).
/// </summary>
/// <param name="co">seed</param>
highp float rand(vec2 co)
{
    highp float a = 12.9898;
    highp float b = 78.233;
    highp float c = 43758.5453;
    highp float dt= dot(co.xy ,vec2(a,b));
    highp float sn= mod(dt,3.14);
    return fract(sin(sn) * c);
}

vec2 vec2rand(vec2 co)
{
    highp float a = 12.9898;
    highp float b = 78.233;
    highp float c = 43758.5453;
    highp float dt= dot(co.xy ,vec2(a,b));
    highp float sn= mod(dt,3.14);
    return fract(vec2(sin(sn), sin(sn + 1.0)) * c);
}

vec3 computeShadow(Ray r, Hit hit, Material m, vec3 filter, vec3 f, vec3 bouncedRayOrigin, vec3 randomPoint)
{
	float step = SUN_RADIUS / softShadowSteps;
	int steps2 = softShadowSteps * 2;
	int stepsDiv = (steps2 + 1) * 3;

	vec3 accum = vec3(0.0);
	if(intersectScene(Ray(bouncedRayOrigin, randomPoint)).m == -1)
    {
        accum += (1.0 - f) * filter * m.diffuse * clamp(dot(hit.n, sunDir), 0.0, 1.0) * sunCol / stepsDiv;   

		if(hit.m == 1)
        {
            vec3 P = r.origin + hit.t*r.direction;
                    
            if(iTexturedMode != 0)
                accum += (1.0 - f) * filter *texture2D(tex, 0.5 * P.xz).xyz * clamp(dot(hit.n, sunDir), 0.0, 1.0) * sunCol / stepsDiv;
        }
                    
        if(hit.m == 7)
        {
            vec3 P = r.origin + hit.t*r.direction;
			vec3 N = nSphere(P, spheres[3]);

			float uu = 0.5 + atan(N.x,N.z) * M_1_PI_2;
			float vv = 0.5 - asin(N.y) * M_1_PI;
			vec2 uvs = vec2(uu,vv);
                    
            if(iTexturedMode != 0)
                accum += (1.0 - f) * filter *texture2D(map,uvs).xyz * clamp(dot(hit.n, sunDir), 0.0, 1.0) * sunCol / stepsDiv;
        }
    }

	return accum;
}

vec3 diffuse(Ray r, Hit hit, Material m, float epsilon, vec3 filter, vec3 f)
{
	vec3 accum = vec3(0.0);
		
	vec3 bouncedRayOrigin = r.origin + hit.t * r.direction + epsilon * sunDir;
	
	if(softShadowSteps == 0)
	{
		if(intersectScene(Ray(bouncedRayOrigin, sunDir)).m == -1)
		{
			accum += (1.0 - f) * filter * m.diffuse * clamp(dot(hit.n, sunDir), 0.0, 1.0) * sunCol;   

			if(hit.m == 1)
			{
				vec3 P = r.origin + hit.t*r.direction;
                    
				if(iTexturedMode != 0)
					accum += (1.0 - f) * filter *texture2D(tex, 0.5 * P.xz).xyz * clamp(dot(hit.n, sunDir), 0.0, 1.0) * sunCol;
			}
                    
			if(hit.m == 7)
			{
				vec3 P = r.origin + hit.t*r.direction;
				vec3 N = nSphere(P, spheres[3]);

				float uu = 0.5 + atan(N.x,N.z) * M_1_PI_2;
				float vv = 0.5 - asin(N.y) * M_1_PI;
				vec2 uvs = vec2(uu,vv);
                    
				if(iTexturedMode != 0)
					accum += (1.0 - f) * filter *texture2D(map,uvs).xyz * clamp(dot(hit.n, sunDir), 0.0, 1.0) * sunCol;
			}
		}
	}
	else
	{
		float step = SUN_RADIUS / softShadowSteps;
		int steps2 = softShadowSteps * 2;
		// lower bound of the interval, in which the random point coordinates can be
		vec3 lowerBound = vec3(sunDir.x - SUN_RADIUS, sunDir.y - SUN_RADIUS, sunDir.z - SUN_RADIUS);	
		int i;

		vec3 randomPoint = vec3(lowerBound.x, sunDir.y, sunDir.z);
		for(i = 0; i <= steps2; i++) {
			accum += computeShadow(r, hit, m, filter, f, bouncedRayOrigin, randomPoint);
			randomPoint.x += step;
		}
		randomPoint = vec3(sunDir.x, lowerBound.y, sunDir.z);
		for(i = 0; i <= steps2; i++) {
			accum += computeShadow(r, hit, m, filter, f, bouncedRayOrigin, randomPoint);
			randomPoint.y += step;
		}
		randomPoint = vec3(sunDir.x, sunDir.y, lowerBound.z);
		for(i = 0; i <= steps2; i++) {
			accum += computeShadow(r, hit, m, filter, f, bouncedRayOrigin, randomPoint);
			randomPoint.z += step;
		}
	}

	return accum;
}

vec3 radiance(Ray r)
{
    float epsilon = 4e-4;

    vec3 accum = vec3(0.0);
    vec3 filter = vec3(1.0);

    for (int i = 0; i <= iNumberBounces; ++i)
    {
        Hit hit = intersectScene(r);
		Material m = getMaterial(hit);

        if (hit.m >= 0)
        {
            vec3 f = fresnel(m.specular, hit.n, -r.direction);
			
			accum += diffuse(r, hit, m, epsilon, filter, f);

            // Specular: next bounce
            filter *= f;
            vec3 d = reflect(r.direction, hit.n);
            r = Ray(r.origin + hit.t * r.direction + epsilon * d, d);
        }
        else
        {
            accum += filter * skyColor(r.direction);
            break;
        }
    }
    return accum;
}

vec3 depthOfField(Ray r, vec3 u, vec3 v, vec3 w, vec2 iResolution, vec2 p, vec2 uv, float l) {
	vec3 color = vec3(0.0);
	float seed = iGlobalTime + (p.x + iResolution.x * p.y) * 1.51269341231;

	for(int i=0; i<FOCUS_STEPS; i++) {
		float fi = float(i);
		//vec2 rnd = vec2rand(vec2(24.4316544311 * fi + gl_FragCoord.xy));   // makes blur more random, but more granular
		vec2 rnd = vec2rand(vec2(24.4316544311 * fi));
		vec3 er = normalize(vec3(p.xy, l));
		r.direction = er.x * u + er.y * v + er.z * w;

		vec3 go = FOCUS_BLUR * vec3((rnd - vec2(0.5)) * 2.0, 0.0);
		vec3 gd = er * iFocusDistance - go;
		if(gd != vec3(0.0))
			gd = normalize(gd);

		//r.origin += go.x * u + go.y * v;
		r.direction += gd.x * u + gd.y * v;
		r.direction = normalize(r.direction);

		vec3 offset = vec3(uv / 720.0, 0.0);
		color += radiance(r) / 4.0;
	}
	return color / float(FOCUS_STEPS);
}

/// <summary>
/// Gamma correction (gamma = 2.2).
/// </summary>
/// <param name="color">actual fragment color</param>
vec3 gammaCorrection(vec3 color)
{
    return pow(color, vec3(1.0 / 2.2));
}

/// <summary>
/// Tone mapping. http://filmicgames.com/archives/75
/// </summary>
/// <param name="color">actual fragment color</param>
vec3 Uncharted2ToneMapping(vec3 color)
{
    float A = 0.15;
    float B = 0.50;
    float C = 0.10;
    float D = 0.20;
    float E = 0.02;
    float F = 0.30;
    float W = 11.2;

    float exposure = 0.01;
    color *= exposure;
    color = ((color * (A * color + C * B) + D * E) / (color * (A * color + B) + D * F)) - E / F;
    float white = ((W * (A * W + C * B) + D * E) / (W * (A * W + B) + D * F)) - E / F;
    color /= white;

    return gammaCorrection(color);
}

/// <summary>
/// Create materials
/// </summary>
void initMaterials()
{
    materials[0] = Material(vec3(1.0, 0.0, 0.2), vec3(0.0));
    materials[1] = Material(vec3(0.0, 0.0, 0.0), vec3(0.0));
    materials[2] = Material(vec3(0.1), vec3(0.95, 0.64, 0.54));
    materials[3] = Material(vec3(0.0), vec3(0.55, 0.56, 0.55));
    materials[4] = Material(vec3(0.0), vec3(1., 0.77, 0.34));
    materials[5] = Material(vec3(0.1,0.3,0.1), vec3(0.95, 0.64, 0.54));
    materials[6] = Material(vec3(1.0,0.1,0.1), vec3(0.95, 0.64, 0.54));
    materials[7] = Material(vec3(0.0), vec3(0.0));
}

//Fragment color output
out vec4 finalColor;

void main(void)
{
	softShadowSteps = 0;

    if(iSoftShadowMode == 1)
    {
        softShadowSteps = 50;

    }
    else {
		softShadowSteps = 0;
	}   
	
    //initialize scene
    initMaterials();

    //get fragment coords
    vec2 uv = 2.0 * gl_FragCoord.xy / vec2(1280.0,720.0) - 1.0;
    
    //store window resolution in vector
    vec2 iResolution = vec2(iWidth, iHeight);   
    float xo = (iMouseX * 2.0 / iResolution.x - 1.0) * PI;
    float yo = (0.5 - (iMouseY / iResolution.y)) * 4.0 + 2.01;

    float o1 = 0.25;
    float o2 = 0.75;
    
    vec2 msaa[4];
    msaa[0] = vec2( o1,  o2);
    msaa[1] = vec2( o2, -o1);
    msaa[2] = vec2(-o1, -o2);
    msaa[3] = vec2(-o2,  o1);

    vec3 color = vec3(0.0);

    /* setup camera
     * create ray for current FS invocation
     */

    vec2 p = (2.0 * gl_FragCoord.xy - iResolution.xy) / iResolution.y; //transform pixel

    Ray r;
    r.origin = vec3(cos(iGlobalTime * 0.1 + xo) * 10.0, yo * 10.0 + 3.0, sin(iGlobalTime * 0.1 + xo) * 10.0 + 5.0);   //camera position
	//r.origin = vec3(15.0, 6.3, -15.0);
    float l = 1.8;     //length
    vec3 ta = vec3(0.0,0.0,0.0);
    vec3 w = normalize(ta - r.origin);
    vec3 u = normalize(cross(w,vec3(0.0,1.0,0.0)));
    vec3 v = normalize(cross(u,w));

	if(iDOFMode == 1) {
		color = depthOfField(r, u, v, w, iResolution, p, uv, l);
	}
	else {
		r.direction = normalize(p.x*u + p.y*v + l*w );
		//for (int i = 0; i < 4; ++i)
		//{
			vec3 offset = vec3(uv / 720.0, 0.0);
			//vec3 d = normalize(vec3(1280.0/720.0 * uv.x, uv.y, -1.5) + offset);
			color += radiance(r) / 4.0;
		//}
	}

	//finalColor = vec4(color, 1.0);
    finalColor = vec4(Uncharted2ToneMapping(color), 1.0);
}