float Waves(float2 worldXZ, sampler2D noiseTex)
{
    float2 uv1 = worldXZ;
    uv1.y += _Time.x;
    float4 noise1 = tex2D(noiseTex, uv1 * 0.025f);

    float2 uv2 = worldXZ;
    uv2.x += _Time.x;
    float4 noise2 = tex2D(noiseTex, uv2 * 0.025f);

    float blendWave = sin((worldXZ.x + worldXZ.y) * 0.1 + (noise1.y + noise2.z) + _Time.y * 2.f);
    blendWave *= blendWave;
            
    float waves = lerp(noise1.z, noise1.w, blendWave) +
                  lerp(noise2.x, noise2.y, blendWave);

    return smoothstep(0.0f, 2.0f, waves) / 2.f;;
            
}

float Foam(float shore, float2 worldXZ, sampler2D noiseTex)
{
    shore = sqrt(shore);
    float foam = sin(shore * 10 - _Time.y);

    foam *= foam * shore;

    return foam;
}