using Editor.Engine;
using Editor.Engine.Interfaces;
using Editor.Engine.Scripting;
using System.Collections.Generic;
using System.IO;


namespace Editor.Editor
{
    internal class Project : ISerializable
    {
        public event AssetsUpdated OnAssetsUpdated;

        public Level CurrentLevel { get; set; } = null;
        public List<Level> Levels { get; set; } = new();
        public string Folder { get; set; } = string.Empty;
        public string ContentFolder { get; private set; } = string.Empty;
        public string AssetFolder { get; private set; } = string.Empty;
        public string ObjectFolder { get; private set; } = string.Empty;
        public string ScriptFolder { get; private set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public AssetMonitor AssetMonitor { get; private set; } = null;
        public ScriptMonitor ScriptMonitor { get; private set; } = null;

        public Project() { }
        public Project(GameEditor game, string name)
        {
            Folder = Path.GetDirectoryName(name);
            Name = Path.GetFileName(name);
            if (!Name.ToLower().EndsWith(".oce"))
            {
                Name += ".oce";
            }
            // Create Content folder for assets, and copy the mgcb template
            ContentFolder = Path.Combine(Folder, "Content");
            AssetFolder = Path.Combine(ContentFolder, "bin");
            ObjectFolder = Path.Combine(ContentFolder, "obj");
            ScriptFolder = Path.Combine(Folder, "Scripts");

            char d = Path.DirectorySeparatorChar;
            if (!Directory.Exists(ContentFolder))
            {
                Directory.CreateDirectory(ContentFolder);
                Directory.CreateDirectory(AssetFolder);
                Directory.CreateDirectory(ObjectFolder);
                File.Copy($"ContentTemplate.mgcb", ContentFolder + $"{d}Content.mgcb");
            }

            if(!Directory.Exists(ScriptFolder))
            {
                Directory.CreateDirectory(ScriptFolder);
            }
            CreateScriptFile(ScriptFolder + $"{d}BeforeRender.lua");
            CreateScriptFile(ScriptFolder + $"{d}AfterRender.lua");
            CreateScriptFile(ScriptFolder + $"{d}BeforeUpdate.lua");
            CreateScriptFile(ScriptFolder + $"{d}AfterUpdate.lua");

            AssetMonitor = new(ObjectFolder);
            AssetMonitor.OnAssetsUpdated += AssetMon_OnAssetsUpdated;
            ScriptMonitor = new(ScriptFolder);
            ScriptMonitor.OnScriptUpdated += ScriptMon_OnScriptUpdated;

            // Add a default level
            AddLevel(game);
            ConfigureScripts();
        }

        private void ScriptMon_OnScriptUpdated(string _script)
        {
            ScriptController.Instance.LoadScriptFile(_script);
        }

        private void CreateScriptFile(string _file)
        {
            string funcName = Path.GetFileName(_file);
            if (!File.Exists(_file))
            {
                File.Create(_file).Close();
                File.AppendAllLines(_file, new string[] { "function " + funcName + "Main()", "end" });
            }
        }

        public void ConfigureScripts()
        {
            char d = Path.DirectorySeparatorChar;
            var sc = ScriptController.Instance;
            sc.LoadSharedObjects(this);
            sc.LoadScriptFile(ScriptFolder + $"{d}BeforeRender.lua");
            sc.LoadScriptFile(ScriptFolder + $"{d}AfterRender.lua");
            sc.LoadScriptFile(ScriptFolder + $"{d}BeforeUpdate.lua");
            sc.LoadScriptFile(ScriptFolder + $"{d}AfterUpdate.lua");
        }

        private void AssetMon_OnAssetsUpdated()
        {
            OnAssetsUpdated?.Invoke();
        }

        public void AddLevel(GameEditor game)
        {
            CurrentLevel = new();
            CurrentLevel.LoadContent(game);
            Levels.Add(CurrentLevel);
        }

        public void Update(float delta)
        {
            CurrentLevel?.Update(delta);
        }

        public void Render()
        {
            CurrentLevel.Render();
        }

        public void Serialize(BinaryWriter stream)
        {
            stream.Write(Folder);
            stream.Write(Name);
            stream.Write(ContentFolder);
            stream.Write(AssetFolder);
            stream.Write(ObjectFolder);
            stream.Write(ScriptFolder);
            stream.Write(Levels.Count);
            int clIndex = Levels.IndexOf(CurrentLevel);
            foreach (Level level in Levels)
            {
                level.Serialize(stream);
            }
            stream.Write(clIndex);
        }

        public void Deserialize(BinaryReader stream, GameEditor game)
        {
            Folder = stream.ReadString();
            Name = stream.ReadString();
            ContentFolder = stream.ReadString();
            AssetFolder = stream.ReadString();
            ObjectFolder = stream.ReadString();
            ScriptFolder = stream.ReadString();
            int levelCount = stream.ReadInt32();
            for (int count = 0; count < levelCount; count++)
            {
                Level level = new Level();
                level.Deserialize(stream, game);
                Levels.Add(level);
            }
            int clIndex = stream.ReadInt32();
            CurrentLevel = Levels[clIndex];          
            AssetMonitor = new(ObjectFolder);
            AssetMonitor.OnAssetsUpdated += AssetMon_OnAssetsUpdated;
            ConfigureScripts();
        }
    }
}
