using Editor.Engine.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using System.IO;

namespace Editor.Engine
{
    internal class Level
    {
        // Members
        // Lists for keep track of added models
        private List<Models> m_suns = new();
		private List<Models> m_worlds = new();
		private List<Models> m_moons = new();

		private List<Models> m_models = new();

		//Random number generator 
		private Random m_rand = new Random();

		private Camera m_camera = new(new Vector3(0, 0, 300), 16 / 9);

        // Accessors
        public Camera GetCamera() { return m_camera; }

        public Level()
        {
        }

        public void LoadContent(ContentManager _content)
        {
            // Models sunModel = new(_content, "Sun", "SunDiffuse", "MyShader", Vector3.Zero, 2.0f);
            //Models worldModel = new(_content, "World", "WorldDiffuse", "MyShader",new Vector3(50, 30, 0), 0.75f);
            //Models moonModel = new(_content, "Moon", "MoonDiffuse", "MyShader",new Vector3(50, 60, 0), 0.2f);

		}

        public void LoadSun(ContentManager _content)
        {
            // Adding a sun
            if(m_suns.Count == 0)
            {
                Models sunModel = new(_content, "Sun", "SunDiffuse", "MyShader",
                    Vector3.Zero, // Position on origin
                    2.0f,         // Scale of 2x
                    0.005f);      // Roates 0.005 in its axis
                m_suns.Add(sunModel);
			}
        }

		public void LoadPlanet(ContentManager _content)
		{
			// Adding planets
			if (m_worlds.Count < 5)
			{
				Models planetModel = new(_content, "World", "WorldDiffuse", "MyShader",
					new Vector3(m_rand.Next(-150,150), m_rand.Next(-90,90), 0), // Randomized Position 
					0.75f,         // Scale of 0.75x
					(float)m_rand.Next(2,4)/100);      // Roatation speed around the sun
				m_worlds.Add(planetModel);
			}
		}

		public void LoadMoon(ContentManager _content)
		{
			// Adding moons
            foreach (Models model in m_worlds)
			{
                Models moonModel = new(_content, "Moon", "MoonDiffuse", "MyShader",
                    new Vector3(model.Position.X + 20, model.Position.Y, model.Position.Z), // Position
                    (float)m_rand.Next(2, 5) / 10,         // Scale of 2x
                    (float)m_rand.Next(5, 10) / 1000);    // Rotation
                moonModel.RelativePos = new Vector3(20, 0, 0);
				moonModel.ParentPlanet = model;
				m_moons.Add(moonModel);
			}
		}

		public void Render()
        {
            // Rendering Sun
            foreach (Models sun in m_suns)
            {
                sun.Render(m_camera.View, m_camera.Projection, new Vector3(0, 0.005f,0), Vector3.Zero);
            }

            // Rendering Planets
            if (m_worlds != null)
            {
                foreach(Models planet in m_worlds)
                {
                    // Getting direction of next position
                    Vector3 planetDir = Vector3.Cross((planet.Position - Vector3.Zero), planet.Position + new Vector3(0, 0, 1));
                    planetDir.Normalize();
                    planetDir *= 10;

                    planet.Render(m_camera.View, m_camera.Projection, new Vector3(0, planet.Speed, 0), planetDir);
                }
            }

            //Rendering Moons
            foreach (Models moon in m_moons)
            {
				// Getting direction of next position
				Vector3 moonDir = Vector3.Cross(moon.RelativePos, moon.Position + new Vector3(0, 0, 1));

                // Setting the position the moon should be after planet has moved
                moon.SetTranslation(moon.ParentPlanet.Position + moon.RelativePos);

                // Rendering and rotating around the planet
                moon.Render(m_camera.View, m_camera.Projection, new Vector3(0, moon.Speed, 0), moonDir);

                // Setting the relativePos after Translation
                moon.RelativePos = moon.Position - moon.ParentPlanet.Position;
			}

        }

        public void Serialize(BinaryWriter _stream)
        {
            _stream.Write(m_models.Count);
            foreach (var model in m_models)
            {
                model.Serialize(_stream);
            }
            m_camera.Serialize(_stream);

        }

        public void Deserialize(BinaryReader _stream, ContentManager _content)
        {
            int modelCount = _stream.ReadInt32();
            for (int count  = 0; count < modelCount; count++)
            {
                Models m = new();
                m.Deserialize(_stream, _content);
                m_models.Add(m);
            }
            m_camera.Deserialize(_stream, _content);
        }
    }
}
