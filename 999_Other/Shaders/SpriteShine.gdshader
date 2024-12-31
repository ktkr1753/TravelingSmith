shader_type canvas_item;

uniform float rate : hint_range(0, 1) = 1.0;
uniform vec4 finalColor : source_color = vec4(1.0, 1.0, 1.0, 1.0);

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec4 inputColor = texture(TEXTURE, UV);
	COLOR = vec4(inputColor.r * (1.0 - rate) + finalColor.r * rate, inputColor.g * (1.0 - rate) + finalColor.g * rate,
	inputColor.b * (1.0 - rate) + finalColor.b * rate, inputColor.a);
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
