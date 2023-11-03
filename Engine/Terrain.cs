using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace Editor.Engine
{
    internal class Terrain
    {
        public VertexPositionNormalTexture[] Vertices { get; set; } // Vertex array
        public VertexBuffer VertexBuffer { get; set; } // Vertex Buffer
        public int[] Indices { get; set; } // Index array
        public IndexBuffer IndexBuffer { get; set; } // Index buffer
        public float[,] Heights { get; set; } // Array of vertex heights
        public int Width { get; set; } // Number of vertices on x axis
        public int Length { get; set; } // Number of vertices on z axis
        public int Height { get; set; } // Terrain height factor
        public int VertexCount { get; set; } // Number of vertices
        public int IndexCount { get; set; } // Number of indices
        public GraphicsDevice Device { get; set; } // The graphicsdevice for rendering
        public Vector3 LightDirection { get; set; } // Direction light is emanating from
        public Texture2D HeightMap { get; set; } // Heightmap texture
        public Texture2D BaseTexture { get; set; } // The terrain difuse texture

        public Terrain(Texture2D heightMap, Texture2D baseTexture, int height, GraphicsDevice device)
        {
            HeightMap = heightMap;
            BaseTexture = baseTexture;
            Device = device;
            Width = heightMap.Width;
            Length = heightMap.Height;
            Height = height;
            LightDirection = new Vector3(0, 1, 1);
            // 1 vertex per pixel
            VertexCount = Width * Length;
            // (Width-1) * (Length-1) cells, 2 triangles per cell, 3 indices per triangle
            IndexCount = (Width - 1) * (Length - 1) * 6;

            GetHeights();
            CreateVertices();
            CreateIndices();
            GenNormals();

            VertexBuffer.SetData<VertexPositionNormalTexture>(Vertices);
            IndexBuffer.SetData<int>(Indices);
        }

        private void GetHeights()
        {
            //Extract pixel data
            Color[] heightMapData = new Color[HeightMap.Width * HeightMap.Height];
            HeightMap.GetData<Color>(heightMapData);
            //Create heights[,] array
            Heights = new float[Width, Length];
            // For each pixel
            for (int y = 0; y < Length; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    //Get color value( 0 - 255)
                    float amt = heightMapData[y * Width + x].R;
                    //Scale to (0 - 1)
                    amt /= 255.0f;
                    //Multiply by max height to get final height
                    Heights[x, y] = amt * Height;
                }
            }

        }

        private void CreateVertices()
        {
            VertexBuffer = new VertexBuffer(Device, typeof(VertexPositionNormalTexture),
                                            VertexCount, BufferUsage.WriteOnly);

            Vertices = new VertexPositionNormalTexture[VertexCount];
            for (int y = 0; y < Length; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int index = y * Width + x;
                    Vertices[index] = new VertexPositionNormalTexture();
                    Vertices[index].Position = new Vector3(x, Heights[x, y], y);
                    Vertices[index].Normal = new Vector3(0, 0, 0);
                    Vertices[index].TextureCoordinate = new Vector2((float)x / Width, (float)y / Length);
                }
            }
        }

        private void CreateIndices()
        {
            IndexBuffer = new IndexBuffer(Device, IndexElementSize.ThirtyTwoBits,
                                          IndexCount, BufferUsage.WriteOnly);

            Indices = new int[IndexCount];
            int i = 0;
            // For each cell
            for (int y = 0; y < Length - 1; y++)
            {
                for (int x = 0; x < Width - 1; x++)
                {
                    //Find the indices of the corners
                    int upperLeft = y * Width + x;
                    int upperRight = upperLeft + 1;
                    int lowerLeft = upperLeft + Width;
                    int lowerRight = lowerLeft + 1;
                    //Specify upper triangle
                    Indices[i++] = upperLeft;
                    Indices[i++] = upperRight;
                    Indices[i++] = lowerLeft;
                    // Specify lower triangle
                    Indices[i++] = lowerLeft;
                    Indices[i++] = upperRight;
                    Indices[i++] = lowerRight;
                }
            }
        }

        private void GenNormals()
        {
            //For each triangle
            for (int i = 0; i < IndexCount; i += 3)
            {
                //Find the position of each corner of the triangle
                Vector3 v1 = Vertices[Indices[i]].Position;
                Vector3 v2 = Vertices[Indices[i + 1]].Position;
                Vector3 v3 = Vertices[Indices[i + 2]].Position;
                // Cross the vector between the corners to get the normal
                Vector3 normal = Vector3.Cross(v1 - v3, v1 - v2);
                normal.Normalize();
                // Add the influence of the normal to each vertex in the triangle
                Vertices[Indices[i]].Normal += normal;
                Vertices[Indices[i + 1]].Normal += normal;
                Vertices[Indices[i + 2]].Normal += normal;
            }
            // Average the influence of the triangles touching each vertex
            for (int i = 0; i < VertexCount; i++)
            {
                Vertices[i].Normal.Normalize();
            }
        }

        public void Draw(Effect effect, Matrix view, Matrix projection)
        {
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["BaseTexture"].SetValue(BaseTexture);
            effect.Parameters["TextureTiling"].SetValue(15.0f);
            effect.Parameters["LightDirection"].SetValue(LightDirection);

            Device.SetVertexBuffer(VertexBuffer);
            Device.Indices = IndexBuffer;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, IndexCount / 3);

            }

        }

    }
}
