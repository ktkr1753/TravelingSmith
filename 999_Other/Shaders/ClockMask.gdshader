shader_type canvas_item;

uniform vec4 selfColor : source_color = vec4(1.0);
uniform vec4 maskColor : source_color = vec4(1.0);
uniform float percent : hint_range(0, 1) = 1.0;

uniform vec2 atlasSize;

varying vec2 offset;

void vertex() {
	// Called for every vertex the material is visible on.
	offset = CUSTOM0.xy;
}

void fragment() {
	
	float angle = 2.0 * PI * percent;
	vec2 tempUV = vec2(mod(UV, vec2(1.0, 1.0) / atlasSize)) + (offset / atlasSize);

	vec2 uv = tempUV + (vec2(-0.5, -0.5) / atlasSize);
	float uvAngle = atan(-uv.x/ uv.y);
	
	vec4 inputColor = selfColor * texture(TEXTURE, UV);
	vec4 mixColor = inputColor * maskColor;
	vec4 color = mixColor;
	if(uv.x >= 0.0)
	{
		if(uv.y >= 0.0)
		{
			if((uvAngle + 1.0 * PI) < angle)
			{
				color = inputColor;
			}
		}
		else
		{
			if((uvAngle + 0.0 * PI) < angle)
			{
				color = inputColor;
			}
		}
	}
	else
	{
		if(uv.y >= 0.0)
		{
			if((uvAngle + 1.0 * PI) < angle)
			{
				color = inputColor;
			}
		}
		else
		{
			if((uvAngle + 2.0 * PI) < angle)
			{
				color = inputColor;
			}
		}
	}
	
	COLOR = color;
}