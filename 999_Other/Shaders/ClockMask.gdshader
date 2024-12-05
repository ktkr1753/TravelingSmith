shader_type canvas_item;

uniform vec4 mask_color : source_color = vec4(1.0);
uniform float percent : hint_range(0, 1) = 1.0;

uniform vec2 atlas_size;

varying vec2 offset;

void vertex() {
	// Called for every vertex the material is visible on.
	offset = CUSTOM0.xy;
}

void fragment() {
	
	float angle = 2.0 * PI * percent;
	
	vec2 tempUV = UV + (vec2(-0.5, -0.5) / atlas_size);
	//vec2 tempUV = UV;
	//vec2 uv = vec2(mod(tempUV, vec2(1.0, 1.0) / atlas_size)) + (offset / atlas_size);
	vec2 uv = UV + vec2(-0.5, -0.5);
	float uvAngle = atan(-uv.x/ uv.y);
	
	vec4 inputColor = texture(TEXTURE, UV);
	vec4 mixColor = inputColor * mask_color;
	vec4 color = mixColor;
	if(uv.x > 0.0)
	{
		if(uv.y > 0.0)
		{
			if((uvAngle + PI) < angle)
			{
				color = inputColor;
			}
			//color = vec4(1.0,0.0,0.0,1.0);
		}
		else if(uv.y < 0.0) 
		{
			if(uvAngle < angle)
			{
				color = inputColor;
			}
			//color = vec4(0.0,1.0,0.0,1.0);
		}
	}
	else if(uv.x < 0.0)
	{
		if(uv.y > 0.0)
		{
			if((uvAngle + PI) < angle)
			{
				color = inputColor;
			}
			//color = vec4(0.0,0.0,1.0,1.0);
		}
		else if(uv.y < 0.0) 
		{
			if((uvAngle + 2.0 * PI) < angle)
			{
				color = inputColor;
			}
			//color = vec4(1.0,1.0,1.0,1.0);
		}
	}
	
	COLOR = color;
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
