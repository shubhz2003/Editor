using Editor.Engine.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Editor.Engine
{
    class Models : ISerializable , INotifyPropertyChanged
    {

        //Members
        private Vector3 m_position;
        private Vector3 m_rotation;
        private ContentManager m_content;
        private string m_diffuseTexture;

        // Accessors
        [Browsable(false)]
        public Model Mesh { get; set; }

        [Browsable(false)]
        public Effect Shader { get; set; } 

        [Browsable(false)]
        public Texture Texture { get; set; }

        [Category("Appearance")]
        [Description("Diffuse texture of the model.")]
        [TypeConverter(typeof(TextureConverter))]
        public string DiffuseTexture
        {
            get
            {
                if (m_diffuseTexture == null)
                    return "Metal";

                return m_diffuseTexture;
            }
            set
            {
                if (value == "HeightMap")
                {
                    m_diffuseTexture = value;

                }
                else if (value == "Grass")
                {
                    m_diffuseTexture = value;

                }
                else
                {
                    //Set Metal as the default texture
                    m_diffuseTexture = "Metal";

                }

                Texture = m_content.Load<Texture>(m_diffuseTexture);
                Texture.Tag = DiffuseTexture;
                OnPropertyChanged("DiffuseTexture");
            }
        }

        [Category("State")]
        [Description("Selection status.")]
        public bool Selected { get; set; } = false;

        [Category("Transformation")]
        [Description("Position of the model in world space.")]
        public Vector3 Position { get => m_position; set { m_position = value; } }

        [Category("Transformation")]
        [Description("Rotation of the model.")]
        public Vector3 Rotation { get => m_rotation; set { m_rotation = value; } }

        [Category("Transformation")]
        [Description("Scale of the model.")]
        public float Scale { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public Models()
        {
        }

        public Models(ContentManager _content,
                      string _model,
                      string _texture,
                      string _effect,
                      Vector3 _position,
                      float _scale)
        {
            m_content = _content;
            Create(_content, _model, _effect, _position, _scale);
        }

        public void Create(ContentManager _content,
                            string _model,
                            string _effect,
                            Vector3 _position,
                            float _scale)
        {
            Mesh = _content.Load<Model>(_model);
            Mesh.Tag = _model;
            Texture = _content.Load<Texture>(DiffuseTexture);
            Texture.Tag = DiffuseTexture;
            Shader = _content.Load<Effect>(_effect);
            Shader.Tag = _effect;
            SetShader(Shader);
            m_position = _position;
            Scale = _scale;
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

        public void Translate(Vector3 _translate, Camera _camera)
        {
            float distance = Vector3.Distance(_camera.Target, _camera.Position);
            Vector3 forward = _camera.Target - _camera.Position;
            forward.Normalize();
            Vector3 left = Vector3.Cross(forward, Vector3.Up);
            left.Normalize();
            Vector3 up = Vector3.Cross(left, forward);
            up.Normalize();
            Position += left * _translate.X * distance;
            Position += up * _translate.Y * distance;
            Position += forward * _translate.Z * 100f;
        }

        public void Rotate(Vector3 _rotate)
        {
            Rotation += _rotate;
        }

        public Matrix GetTransform()
        {
            return Matrix.CreateScale(Scale) *
                Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) *
                Matrix.CreateTranslation(Position);
        }

        public void Render(Matrix _view, Matrix _projection)
        {
            //m_rotation.X += 0.001f;
            //m_rotation.Y += 0.005f;
            // m_rotation.Z += 0.05f;

            Shader.Parameters["World"].SetValue(GetTransform());
            Shader.Parameters["WorldViewProjection"].SetValue(GetTransform() * _view * _projection);
            Shader.Parameters["Texture"].SetValue(Texture);
            Shader.Parameters["Tint"].SetValue(Selected);

            foreach (ModelMesh mesh in Mesh.Meshes)
            {
                mesh.Draw();
            }
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
            Create(_content, mesh, shader, Position, Scale);
        }
    }
}
