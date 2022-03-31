#version 330 core

out vec4 FragColor;

struct PointLight {
    bool enabled;
    vec3 position;

    float constant;
    float linear;
    float quadratic;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

#define NR_POINT_LIGHTS 4
uniform PointLight pointLights[NR_POINT_LIGHTS];

uniform sampler2D materialDiffuse;
uniform sampler2D materialNormal;
uniform sampler2D materialSpecular;

uniform vec3 sunAmbient;

uniform float shininess;

in VS_OUT {
    vec3 FragPos;
    vec2 TexCoords;
    vec3 TangentViewPos;
    vec3 TangentFragPos;
    vec3 TangentSunDirection;
} fs_in;

vec3 CalcDirLight(vec3 direction, vec3 normal, vec3 viewDir);
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir);

void main()
{
    vec3 norm = normalize(texture(materialNormal, fs_in.TexCoords).rgb * 2.0 - 1.0);
    vec3 viewDir = normalize(fs_in.TangentViewPos - fs_in.TangentFragPos);

    vec3 result = CalcDirLight(fs_in.TangentSunDirection, norm, viewDir);

    for(int i = 0; i < NR_POINT_LIGHTS; i++)
        if (pointLights[i].enabled)
            result += CalcPointLight(pointLights[i], norm, fs_in.TangentFragPos, viewDir);
    FragColor = vec4(result, 1.0);
}

vec3 CalcDirLight(vec3 dir, vec3 normal, vec3 viewDir)
{
    vec3 lightDir = normalize(-dir);
    float diff = max(dot(normal, lightDir), 0.0);

    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), shininess);
    
    vec3 color = texture(materialDiffuse, fs_in.TexCoords).rgb;

    vec3 ambient  = 0.1  * color * sunAmbient;
    vec3 diffuse  = diff * color * sunAmbient;
    vec3 specular = spec * texture(materialSpecular, fs_in.TexCoords).rgb;
    return (ambient + diffuse + specular);
}

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    vec3 lightDir = normalize(light.position - fragPos);
    float diff = max(dot(normal, lightDir), 0.0);

    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), shininess);

    float distance    = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance +
    light.quadratic * (distance * distance));

    vec3 color = texture(materialDiffuse, fs_in.TexCoords).rgb;

    vec3 ambient  = 0.1 * color;
    vec3 diffuse  = diff * color;
    vec3 specular = spec * vec3(texture(materialSpecular, fs_in.TexCoords));
    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;
    return (ambient + diffuse + specular);
}