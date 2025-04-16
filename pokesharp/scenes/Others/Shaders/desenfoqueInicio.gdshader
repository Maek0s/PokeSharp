shader_type canvas_item;

uniform float blur_strength : hint_range(1.0, 10.0) = 3.0;  // Ajusta la cantidad de desenfoque
uniform vec2 screen_size = vec2(1920, 1080);  // Ajusta al tamaño de tu pantalla

void fragment() {
    vec2 texel_size = vec2(1.0 / screen_size.x, 1.0 / screen_size.y);  // Calcula el tamaño del píxel

    vec4 color = vec4(0.0);

    // Aplicar desenfoque simple
    for (float i = -blur_strength; i <= blur_strength; i++) {
        for (float j = -blur_strength; j <= blur_strength; j++) {
            // Accedemos a la textura actual y sumamos sus colores vecinos
            color += texture(TEXTURE, FRAGCOORD.xy + vec2(i, j) * texel_size);
        }
    }

    // Promediamos los colores para obtener el desenfoque
    color /= (blur_strength * 2.0 + 1.0) * (blur_strength * 2.0 + 1.0);

    COLOR = color;  // El resultado final se asigna a la variable COLOR
}
