using Microsoft.Xna.Framework.Audio;

namespace Editor.Engine
{
    internal class SFXInstance
    {
        public string Name { get; set; }
        public SoundEffectInstance Instance { get; set; }

        public static SFXInstance Create(GameEditor game, string assetName)
        {
            SoundEffect ef = game.Content.Load<SoundEffect>(assetName);
            SoundEffectInstance efi = ef.CreateInstance();
            efi.Volume = 1;
            efi.IsLooped = false;
            return new SFXInstance() { Name = assetName, Instance = efi };
        }
    };
}
