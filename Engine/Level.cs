using Editor.Engine.Interfaces;
using Editor.Engine.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Editor.Engine
{
    internal class Level : ISerializable
    {
        // Accessors
        public Camera GetCamera() { return m_camera; }
        public Terrain GetTerrain() { return m_terrain; }
        public Light GetLight() { return m_light; }

        // Members
        private List<Models> m_models = new();
        private Camera m_camera = new(new Vector3(0, 400, 500), 16 / 9);
        private Light m_light = new Light() { Position = new(0, 400, -500), Color = new(0.9f, 0.9f, 0.9f) };
        private Effect m_terrainEffect = null;
        private Terrain m_terrain = null;

        public Level() { }

        public void LoadContent(GameEditor game)
        {
            m_terrain = new(game.DefaultEffect, game.DefaultHeightMap, game.DefaultGrass, 200, game.GraphicsDevice);
        }

        public void AddModel(Models model)
        {
            m_models.Add(model);
        }

        public List<Models> GetModelsList()
        {
            return m_models;
        }

        public void ClearSelectedModels()
        {
            foreach (var model in m_models) 
            {
                model.Selected = false;
            }
            if (m_terrain != null)
            {
                m_terrain.Selected = false;
            }
        }

        public List<ISelectable> GetSelectedModels()
        {
            List<ISelectable> models = new List<ISelectable>();
            foreach (var model in m_models)
            {
                if (model.Selected) models.Add(model);
            }
            if (m_terrain != null && m_terrain.Selected) models.Add(m_terrain);

            return models;
        }

        public void HandleTranslate()
        {
            InputController ic = InputController.Instance;
            Vector3 translate = Vector3.Zero;
            if (ic.IsKeyDown(Keys.Left)) translate.X += -10;
            if (ic.IsKeyDown(Keys.Right)) translate.X += +10;

            if (ic.IsKeyDown(Keys.Menu))
            {
                if (ic.IsKeyDown(Keys.Up)) translate.Z += 1;
                if (ic.IsKeyDown(Keys.Down)) translate.Z += -1;
            }
            else
            {
                if (ic.IsKeyDown(Keys.Up)) translate.Y += 10;
                if (ic.IsKeyDown(Keys.Down)) translate.Y += -10;
            }

            if (ic.IsButtonDown(MouseButtons.Middle))
            {
                Vector2 dir = ic.MousePosition - ic.LastPosition;
                translate.X = dir.X;
                translate.Y = -dir.Y;
            }

            if (ic.GetWheel() != 0)
            {
                translate.Z = ic.GetWheel() * 2;
            }

            if (translate != Vector3.Zero)
            {
                bool modelTranslated = false;
                foreach (Models model in m_models)
                {
                    if (model.Selected)
                    {
                        modelTranslated = true;
                        model.Translate(translate / 1000, m_camera);
                    }
                }
                if (!modelTranslated)
                {
                    m_camera.Translate(translate * 0.001f);
                }
            }
        }

        public void HandleRotate(float delta)
        {
            InputController ic = InputController.Instance;
            if (ic.IsButtonDown(MouseButtons.Right) && (!ic.IsKeyDown(Keys.Menu)))
            {
                Vector2 dir = ic.MousePosition - ic.LastPosition;
                if (dir != Vector2.Zero)
                {
                    Vector3 movement = new Vector3(dir.Y, dir.X, 0) * delta;
                    bool modelRoated = false;
                    foreach (Models model in m_models)
                    {
                        if (model.Selected)
                        {
                            modelRoated = true;
                            model.Rotation += movement;
                        }
                    }
                    if (!modelRoated)
                    {
                        m_camera.Rotate(movement);
                    }
                }
            }
        }

        private void HandleScale(float delta)
        {
            InputController ic = InputController.Instance;
            if (ic.IsButtonDown(MouseButtons.Right) && ic.IsKeyDown(Keys.Menu))
            {
                float l = ic.MousePosition.X - ic.LastPosition.X;
                if (l != 0)
                {
                    l += delta;
                    foreach (Models model in m_models)
                    {
                        if (model.Selected)
                        {
                            model.Scale += l;
                        }
                    }
                }
            }
        }

        internal ISelectable HandlePick(bool selected = true)
        {
            float? f;
            Matrix transform = Matrix.Identity;
            InputController ic = InputController.Instance;
            if (ic.IsButtonDown(MouseButtons.Left) || !selected)
            {
                Ray r = HelpMath.GetPickRay(ic.MousePosition, m_camera);
                // Check for Models
                foreach (Models model in m_models)
                {
                    if (selected) model.Selected = false;
                    transform = model.GetTransform();
                    foreach (ModelMesh mesh in model.Mesh.Meshes)
                    {
                        BoundingSphere s = mesh.BoundingSphere;
                        s.Transform(ref transform, out s);
                        f = r.Intersects(s);
                        if (f.HasValue)
                        {
                            f = HelpMath.PickTriangle(in mesh, ref r, ref transform);
                            if (f.HasValue)
                            {
                                if (!selected) return model;
                                model.Selected = true;
                            }
                        }
                    }
                }
                // Check for Terrain
                if (m_terrain != null)
                {
                    transform = Matrix.Identity;
                    f = HelpMath.PickTriangle(in m_terrain, ref r, ref transform);
                    m_terrain.Selected = false;
                    if (f.HasValue)
                    {
                        if (!selected) return m_terrain;
                        m_terrain.Selected = true;
                    }
                }
            }
            return null;
        }

        public void HandleAudio()
        {
            foreach (Models m in m_models)
            {
                if ((Models.SelectedDirty) &&
                    m.Selected)
                {
                    var sfi = m.SoundEffects[(int)SoundEffectTypes.OnSelect];
                    if (sfi?.Instance.State == SoundState.Stopped)
                    {
                        sfi.Instance.Play();
                    }
                }
            }
        }

        public void Update(float delta)
        {
            HandleTranslate();
            HandleRotate(delta);
            HandleScale(delta);
            HandlePick();
            HandleAudio();
        }

        public void Render()
        {
            Renderer r = Renderer.Instance;
            r.Camera = m_camera;
            r.Light = m_light;
            foreach (Models model in m_models)
            {
                r.Render(model);
            }
            if (m_terrain != null)
            {
                r.Render(m_terrain);
            }
        }

        public void Serialize(BinaryWriter stream)
        {
            stream.Write(m_models.Count);
            foreach (Models model in m_models)
            {
                model.Serialize(stream);
            }
            m_camera.Serialize(stream);
        }

        public void Deserialize(BinaryReader stream, GameEditor game)
        {
            int modelCount = stream.ReadInt32();
            for (int count = 0; count < modelCount; count++)
            {
                Models model = new Models();
                model.Deserialize(stream, game);
                m_models.Add(model);
            }
            m_camera.Deserialize(stream, game);
        }

        public override string ToString()
        {
            string s = string.Empty;
            foreach (Models model in m_models)
            {
                if (model.Selected)
                {
                    s += "\nModel: Pos: " + model.Position.ToString() + " Rot: " + model.Rotation.ToString();
                }
            }
            return m_camera.ToString() + s;
        }
    }
}
