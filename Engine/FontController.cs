using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Editor.Engine
{
    internal class FontController
    {
        private SpriteFont m_fontArial16 = null;
        private SpriteFont m_fontArial18 = null;
        private SpriteFont m_fontArial20 = null;

        public FontController() 
        { 
        }

        public void LoadContent(ContentManager _content)
        {
            m_fontArial16 = _content.Load<SpriteFont>("Arial16");
			m_fontArial18 = _content.Load<SpriteFont>("Arial18");
			m_fontArial20 = _content.Load<SpriteFont>("Arial20");
		}

        public void Draw(SpriteBatch _spriteBatch, int _size, string _text, Vector2 _position, Color _color)
        {
            switch (_size)
            {
                case 16:
                    _spriteBatch.DrawString(m_fontArial16, _text, _position, _color);
                    break;
				case 18:
					_spriteBatch.DrawString(m_fontArial18, _text, _position, _color);
					break;
				case 20:
					_spriteBatch.DrawString(m_fontArial20, _text, _position, _color);
					break;
                default:
                    Debug.Assert(true);
                    break;
			}
        }
    }
}
