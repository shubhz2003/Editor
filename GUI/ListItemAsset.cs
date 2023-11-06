using Editor.Editor;
using Editor.Engine;

namespace Editor.GUI
{
    internal class ListItemAsset
    {
        public string Name { get; set; }
        public AssetTypes Type { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
