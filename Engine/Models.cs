using Editor.Engine.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace Editor.Engine
{
    class Models : ISerializable
    {
        //Members
        private Vector3 m_position;
        private Vector3 m_rotation;

        // Accessors
        public Model Mesh { get; set; }
        public Effect Shader { get; set; }
        public Vector3 Position { get => m_position; set { m_position = value; } }
        public Vector3 Rotation { get => m_rotation; set { m_rotation = value; } }
        public float Scale { get; set; }

        public Vector3 RelativePos { get; set; }
        public Models ParentPlanet { get; set; }
        public float orbitSpeed { get; set; }


        // Texturing
        public Texture Texture { get; set; }

        public Models()
        {
        }

        public Models(ContentManager _content,
                      string _model,
                      string _texture,
                      string _effect,
                      Vector3 _position,
                      float _scale,
                      float _rotation)
        {
            Create(_content, _model, _texture, _effect, _position, _scale, _rotation);
        }

        public void Create(ContentManager _content,
                            string _model,
                            string _texture,
                            string _effect,
                            Vector3 _position,
                            float _scale,
                            float _rotation)
        {
            Mesh = _content.Load<Model>(_model);
            Mesh.Tag = _model;
            Texture = _content.Load<Texture>(_texture);
            Texture.Tag = _texture;
            Shader = _content.Load<Effect>(_effect);
            Shader.Tag = _effect;
            SetShader(Shader);
            m_position = _position;
            Scale = _scale;
            m_rotation.Y = _rotation ;


        }

        public void SetShader(Effect _effect)
        {
            Shader = _effect;
            foreach (ModelMesh mesh in Mesh.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = Shader;
                }
            }
        }

        public Matrix GetTransform()
        {
            return Matrix.CreateScale(Scale) *
                Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) *
                Matrix.CreateTranslation(Position);
        }

		public void SetTranslation(Vector3 translation)
		{
			Position = translation;
		}

		public void Render(Matrix _view, Matrix _projection, Vector3 _rotation, Vector3 _direction)
        {
            //m_rotation.X += 0.001f;
            //m_rotation.Y += 0.005f;
            // m_rotation.Z += 0.05f;

            m_rotation += new Vector3(_rotation.X, _rotation.Y, 0);
            m_position += new Vector3(_direction.X / 100, _direction.Y / 100, 0);

            Shader.Parameters["World"].SetValue(GetTransform());
            Shader.Parameters["WorldViewProjection"].SetValue(GetTransform() * _view * _projection);
            Shader.Parameters["Texture"].SetValue(Texture);

            foreach (ModelMesh mesh in Mesh.Meshes)
            {
                mesh.Draw();
            }

			// Update planet's orbit
			//m_position.X = (float)Math.Cos(m_rotation.Y * orbitSpeed) * 150;
			//m_position.Y = (float)Math.Sin(m_rotation.Y * orbitSpeed) * 90;
		}

        public void Serialize(BinaryWriter _stream)
        {
            _stream.Write(Mesh.Tag.ToString());
            _stream.Write(Texture.Tag.ToString());
            _stream.Write(Shader.Tag.ToString());
            HelpSerialize.Vec3(_stream, Position);
            HelpSerialize.Vec3(_stream, Rotation);
            _stream.Write(Scale);
        }

        public void Deserialize(BinaryReader _stream, ContentManager _content)
        { 
            string mesh = _stream.ReadString();
            string texture = _stream.ReadString();
            string shader = _stream.ReadString();
            Position = HelpDeserialize.Vec3(_stream);
            Rotation = HelpDeserialize.Vec3(_stream);
            Scale = _stream.ReadSingle();
            Create(_content, mesh, texture, shader, Position, Scale, m_rotation.Y);
        }
    }
}
