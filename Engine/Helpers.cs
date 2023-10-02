using Microsoft.Xna.Framework;
using System.IO;

namespace Editor.Engine
{
    internal class HelpSerialize
    {
        public static void Vec3(BinaryWriter _stream, Vector3 _vector)
        {
            _stream.Write(_vector.X);
            _stream.Write(_vector.Y);
            _stream.Write(_vector.Z);
        }
    }

    internal class HelpDeserialize
    {
        public static Vector3 Vec3(BinaryReader _stream)
        {
            Vector3 v = Vector3.Zero;
            v.X = _stream.ReadSingle();
            v.Y = _stream.ReadSingle();
            v.Z = _stream.ReadSingle();
            return v;
        }
    }
}
