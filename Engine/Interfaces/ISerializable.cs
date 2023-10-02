using Microsoft.Xna.Framework.Content;
using System.IO;

namespace Editor.Engine.Interfaces
{
    internal interface ISerializable
    {
        public void Serialize(BinaryWriter _stream);
        public void Deserialize(BinaryReader _stream, ContentManager _content);
    }
}
