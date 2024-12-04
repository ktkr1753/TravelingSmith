shader_type canvas_item;

uniform vec4 inner_color : source_color = vec4(1.0);
uniform float inner_ridus : hint_range(0, 1000) = 10;
uniform vec4 line_color : source_color = vec4(1.0);
uniform float line_thickness : hint_range(0,10) = 1.0;

const vec2 OFFSETS[4] = {
	vec2(-1,0), vec2(0,-1), vec2(0,1),
	vec2(1,0)
};

void fragment() {
	vec2 uv = UV - 0.5;

	float d = length(uv);
	vec4 color = vec4(0.0);

	float xPixel = round(abs(uv.x) / TEXTURE_PIXEL_SIZE.x);
	float yPixel = round(abs(uv.y) / TEXTURE_PIXEL_SIZE.y);

	float powPixelPos = pow(xPixel, 2) + pow(yPixel,2);
	//float powPixelPos = round(pow(uv.x / TEXTURE_PIXEL_SIZE.x, 2) + pow(uv.y / TEXTURE_PIXEL_SIZE.y, 2));
	float powOutlineR = pow(inner_ridus + line_thickness,2);
	float powR =  pow(inner_ridus,2);

	if(powPixelPos < powOutlineR) {
		if(powPixelPos < powR){
			color = inner_color;
		}
		else {
			color = line_color;
		}
	}
	COLOR = color;
}
