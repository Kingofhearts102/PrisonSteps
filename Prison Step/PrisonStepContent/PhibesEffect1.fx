float4x4 World;
float4x4 View;
float4x4 Projection;
float3 DiffuseColor;
float3 LightAmbient = float3(.05, .05, .10);


float3 Light1Location = float3(568, 246, 1036);
float3 Light1Color = float3(1, 1, 1);
// Light 2 Decleration
float3 Light2Color = float3(14.29, 45, 43.94);
float3 Light2Location = float3(821, 224, 941);
//Light 3 Decleration
float3 Light3Location = float3(824, 231, 765);
float3 Light3Color = float3(82.5, 0, 0);
// TODO: add effect parameters here.

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL0;
    // TODO: add input channels such as texture
    // coordinates and vertex colors here.
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
	float4 Pos1 : TEXCOORD1;

    // TODO: add vertex shader outputs such as colors and texture
    // coordinates here. These values will automatically be interpolated
    // over the triangle, and provided as input to your pixel shader.
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	float3 color = LightAmbient;

	float3 L3 = Light3Location - worldPosition;
	float L3distance = length(L3);
	L3 /= L3distance;

	float3 L2 = Light2Location - worldPosition;
	float L2distance = length(L2);
	L2 /= L2distance;

	float3 normal = normalize(mul(input.Normal, World));
	float3 L1 = normalize(Light1Location - worldPosition);
	color += saturate(dot(L1, normal)) * Light1Color;
	color += saturate(dot(L2, normal)) / L2distance * Light2Color;
	color += saturate(dot(L3, normal)) / L3distance * Light3Color;
	color *= DiffuseColor;
	
	output.Color = float4(color, 1);

    // TODO: add your vertex shader code here.
	output.Pos1 = output.Position;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // TODO: add your pixel shader code here.
	float y = input.Pos1.y / input.Pos1.w;
    	
	if(y > 0.15) 
		return float4(1, 0, 0, 1);
	else
		return input.Color;
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
