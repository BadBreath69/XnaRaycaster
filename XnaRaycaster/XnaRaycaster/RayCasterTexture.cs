using System;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace XnaRaycaster
{
    public class RayCasterTexture
    {
        private const int Darknesslevels = 48;

        // First parameter is darkness then Color X, Color Y
        private Int32[][,] m_TextureData;

        public Int32[][,] Texture
        {
            get { return m_TextureData; }
            set { m_TextureData = value; }
        }

        public RayCasterTexture(Texture2D texture)
        {
            var sw = new Stopwatch();
            sw.Start();

                LoadTexture(texture);
            
            sw.Stop();
            Debug.WriteLine("Time taken to load texture: " + sw.ElapsedMilliseconds + "ms");
        }

        private void LoadTexture(Texture2D texture)
        {
            var data = new Int32[texture.Width*texture.Height];
            texture.GetData(data);

            // Make levels of darkness
            m_TextureData = new Int32[Darknesslevels][,];

            float darkness = 1.0f;
            for (int d = 0; d < Darknesslevels; d++)
            {
                m_TextureData[d] = new Int32[texture.Width,texture.Height];
                for (int x = 0; x < texture.Width; x++)
                {
                    for (int y = 0; y < texture.Height; y++)
                    {
                        var index = x + (texture.Width*y);
                        var pixel = data[index];
                        var bytes = BitConverter.GetBytes(pixel);

                        pixel = 255 << 24 | (Byte) (bytes[2]/darkness) << 16 | (Byte) (bytes[1]/darkness) << 8 |
                                (Byte) (bytes[0]/darkness);

                        m_TextureData[d][x, y] = pixel;
                    }
                }

                // TODO find the perfect co-efficents for the equation of darkness
                darkness += 0.00f + (d*0.0125f) + (d*d*0.00025f);
            }
        }
    }
}
