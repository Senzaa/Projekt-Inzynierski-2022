#version 330 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoords;
layout(location = 2) in vec3 aNormal;
layout(location = 3) in vec3 aTangent;

uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;

uniform vec3 viewPos;
uniform vec3 sunDir;

out VS_OUT {
    vec3 FragPos;
    vec2 TexCoords;
    vec3 TangentViewPos;
    vec3 TangentFragPos;
    vec3 TangentSunDirection;
} vs_out;

void main()
{
    gl_Position = vec4(aPosition, 1.0) * model * view * projection;

    vs_out.FragPos = vec3(vec4(aPosition, 1.0) * model);
    vs_out.TexCoords = aTexCoords;

    mat3 normalMatrix = mat3(transpose(inverse(model)));
    vec3 T = normalize(aTangent * normalMatrix);
    vec3 N = normalize(aNormal * normalMatrix);
    T = normalize(T - dot(T, N) * N);
    vec3 B = cross(N, T);

    // right handedness fix
    if (dot(cross(N, T), B) < 0.0)
        T = T * -1.0;
    
    mat3 TBN = transpose(mat3(T, B, N));
    vs_out.TangentViewPos = TBN * viewPos;
    vs_out.TangentFragPos = TBN * vs_out.FragPos;
    vs_out.TangentSunDirection = TBN * sunDir;
}