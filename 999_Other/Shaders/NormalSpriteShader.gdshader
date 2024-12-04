shader_type canvas_item;

uniform vec4 addColor : source_color = vec4(1.0, 1.0, 1.0, 1.0);

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec4 inputColor = texture(TEXTURE, UV);
	COLOR = vec4(inputColor.r * addColor.r, inputColor.g * addColor.g, inputColor.b * addColor.b, inputColor.a * addColor.a);
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
