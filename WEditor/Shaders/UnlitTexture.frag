﻿#version 330 

//Interpolated values from the vertex shaders
in vec4 nColor;
in vec2 nTexCoord;


//Output Data
out vec4 finalColor;

uniform sampler2D tex;

void main()
{
	finalColor = vec4(1, 0, 0, 1); //texture(tex, nTexCoord) * nColor;
}