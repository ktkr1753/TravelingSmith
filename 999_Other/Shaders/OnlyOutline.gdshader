shader_type canvas_item;

uniform vec4 outLineColor : source_color = vec4(1.0);
uniform vec4 insideColor : source_color = vec4(1.0);
uniform float width : hint_range(0, 10) = 1.0;
uniform int pattern : hint_range(0, 2) = 0; // diamond, circle, square
uniform vec2 number_of_images = vec2(1.0); // number of horizontal and vertical images in the sprite sheet

void vertex() {
}

bool hasContraryNeighbour(vec2 uv, vec2 texture_pixel_size, vec2 image_top_left, vec2 image_bottom_right, sampler2D texture) {
	for (float i = -ceil(width); i <= ceil(width); i++) {
		float x = abs(i) > width ? width * sign(i) : i;
		float offset;

		if (pattern == 0) {
			offset = width - abs(x);
		} else if (pattern == 1) {
			offset = floor(sqrt(pow(width + 0.5, 2) - x * x));
		} else if (pattern == 2) {
			offset = width;
		}

		for (float j = -ceil(offset); j <= ceil(offset); j++) {
			float y = abs(j) > offset ? offset * sign(j) : j;
			vec2 xy = uv + texture_pixel_size * vec2(x, y);

			if ((xy != clamp(xy, image_top_left, image_bottom_right) || texture(texture, xy).a <= 0.0)) {
				return true;
			}
		}
	}

	return false;
}

void fragment() {
	vec2 uv = UV;
	vec2 image_top_left = floor(uv * number_of_images) / number_of_images;
	vec2 image_bottom_right = image_top_left + vec2(1.0) / number_of_images;

	COLOR = texture(TEXTURE, uv);

	if ((COLOR.a > 0.0)) {
		if(hasContraryNeighbour(uv, TEXTURE_PIXEL_SIZE, image_top_left, image_bottom_right, TEXTURE))
		{
			COLOR.rgb = mix(COLOR.rgb, outLineColor.rgb, outLineColor.a);
			COLOR.a += (1.0 - COLOR.a) * outLineColor.a;
		}
		else
		{
			COLOR.rgb = mix(COLOR.rgb, insideColor.rgb, insideColor.a);
			COLOR.a += (1.0 - COLOR.a) * insideColor.a;
		}
	}
}