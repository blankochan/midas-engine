shader_type spatial;
render_mode unshaded;

vec3 normal_from_depth(vec2 uv, mat4 inv_proj_mat, float depth){

    //Normal recontruction from Depth:
    float x = uv.x * 2.0 - 1.0;
    float y = (1.0 - uv.y) * 2.0 - 1.0;

    vec4 position_s = vec4(x, y, depth, 1.0);
    vec4 position_v = inv_proj_mat * position_s;

    vec3 P = position_v.xyz / position_v.w;

    //inverting P's y axis and calculating the cross product of dFdy first and dFdx later fixes the y axis
    P.y = -P.y;
    return normalize(cross(dFdy(P), dFdx(P)));
}

void vertex() {

}

void fragment() {
    vec3 normal = normal_from_depth(SCREEN_UV, INV_PROJECTION_MATRIX, FRAGCOORD[2]);
    ALBEDO = vec3(normal);
}
