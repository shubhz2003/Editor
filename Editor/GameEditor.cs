﻿using Editor.Editor;
using Editor.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Forms;

namespace Editor
{
    public class GameEditor : Game
    {
        internal Project Project { get; set; }
        internal Texture DefaultTexture { get; set; }
        internal Texture2D DefaultGrass { get; set; }
        internal Texture2D DefaultHeightMap { get; set; }
        internal Effect DefaultEffect { get; set; }

        private GraphicsDeviceManager _graphics;
        private FormEditor _formEditor;
        private SpriteBatch _spriteBatch;
        private FontController _fonts;
        RasterizerState _rasterizerState = new RasterizerState();
        DepthStencilState _depthStencilState = new DepthStencilState();

        public GameEditor()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _rasterizerState = new RasterizerState();
            _rasterizerState.CullMode = CullMode.None;
            _depthStencilState = new DepthStencilState();
            _depthStencilState.DepthBufferWriteEnable = true;
        }

        public GameEditor(FormEditor formEditor) : this()
        {
            _formEditor = formEditor;
            Form gameForm = Control.FromHandle(Window.Handle) as Form;
            gameForm.TopLevel = false;
            gameForm.Dock = DockStyle.Fill;
            gameForm.FormBorderStyle = FormBorderStyle.None;
            formEditor.splitContainer.Panel1.Controls.Add(gameForm);
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _fonts = new();
            _fonts.LoadContent(Content);
            DefaultTexture = Content.Load<Texture>("DefaultTexture");
            DefaultGrass = Content.Load<Texture2D>("DefaultGrass");
            DefaultHeightMap = Content.Load<Texture2D>("DefaultHeightMap");
            DefaultEffect = Content.Load<Effect>("DefaultShader");
        }

        public void AdjustAspectRatio()
        {
            if (Project == null) return;
            Camera camera = Project.CurrentLevel.GetCamera();
            camera.Viewport = _graphics.GraphicsDevice.Viewport;
            camera.Update(camera.Position, _graphics.GraphicsDevice.Viewport.AspectRatio);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Project != null)
            {
                Content.RootDirectory = Project.ContentFolder + "\\bin";
                Project?.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                InputController.Instance.Clear();
                UpdateSelected();
            }
            base.Update(gameTime);
        }

        private void UpdateSelected()
        {
            if (Models.SelectedDirty)
            {
                var models = Project.CurrentLevel.GetSelectedModels();
                if (models.Count == 0)
                {
                    _formEditor.propertyGrid.SelectedObject = null;
                }
                else if (models.Count > 1)
                {
                    _formEditor.propertyGrid.SelectedObject = models.ToArray();
                }
                else
                {
                    _formEditor.propertyGrid.SelectedObject = models[0];
                }
            }
            Models.SelectedDirty = false;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            if (Project == null) return;
            GraphicsDevice.RasterizerState = _rasterizerState;
            GraphicsDevice.DepthStencilState = _depthStencilState;
            Project?.Render();
            _spriteBatch.Begin();
            _fonts.Draw(_spriteBatch, 20, InputController.Instance.ToString(), new Vector2(20, 20), Color.White);
            _fonts.Draw(_spriteBatch, 16, Project.CurrentLevel.ToString(), new Vector2(20, 80), Color.Yellow);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}