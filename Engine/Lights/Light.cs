using Microsoft.Xna.Framework;

namespace Editor.Engine.Lights
{
    internal class Light
    {
        public Light() { }

        public Vector3 Position { get; set; }
        public Vector3 Color { get; set; }

        public void SetColor(float _r, float _g, float _b)
        {
            Vector3 color = Color;
            color.X = _r;
            color.Y = _g;
            color.Z = _b;
            Color = color;
        }
    }
}
