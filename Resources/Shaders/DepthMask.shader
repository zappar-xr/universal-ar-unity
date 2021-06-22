Shader "Zappar/DepthMask"
{
    Properties
    {
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry-1"
        }
        Pass
        {
            ColorMask 0
            ZWrite On
        }
    }
}