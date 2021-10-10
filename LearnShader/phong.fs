#version 330 core
out vec4 FragColor;
in vec2 TexCoords;
in vec3 WorldPos;
in vec3 Normal;

// material parameters
uniform sampler2D albedoMap;
uniform sampler2D normalMap;
uniform sampler2D specularMap;
uniform sampler2D emissiveMap;

// lights
uniform vec3 lightPosition;
uniform vec3 lightColor;

uniform vec3 camPos;

vec3 getNormalFromMap()
{
    vec3 tangentNormal = texture(normalMap, TexCoords).xyz * 2.0 - 1.0;

    vec3 Q1  = dFdx(WorldPos);
    vec3 Q2  = dFdy(WorldPos);
    vec2 st1 = dFdx(TexCoords);
    vec2 st2 = dFdy(TexCoords);

    vec3 N   = normalize(Normal);
    vec3 T  = normalize(Q1*st2.t - Q2*st1.t);
    vec3 B  = -normalize(cross(N, T));
    mat3 TBN = mat3(T, B, N);

    return normalize(TBN * tangentNormal);
}
// ----------------------------------------------------------------------------
void main()
{
    vec3 albedo       = texture(albedoMap, TexCoords).rgb;
    vec3 emission     = 0.5f * texture(emissiveMap, TexCoords).rgb;

    vec3 lightColor = vec3(1.0f);

    // diffuse
    vec3 norm = getNormalFromMap();
    vec3 lightDir = normalize(lightPosition - WorldPos);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * lightColor;
    
    // specular
    float specularStrength = texture(specularMap, TexCoords).r;
    vec3 viewDir = normalize(camPos - WorldPos);
    vec3 halfwayDir = normalize(lightDir + viewDir);  
    float spec = pow(max(dot(norm, halfwayDir), 0.0), 256.0);
    vec3 specular = specularStrength * spec * lightColor;  

    // ambient
    float NdotL = max(dot(norm, lightDir), 0.0);   
  	vec3 ambientAndEmission = emission * pow(1.0f - NdotL, 3.0f);

    vec3 color = (ambientAndEmission + diffuse + specular) * albedo;
    FragColor = vec4(color, 1.0);
}