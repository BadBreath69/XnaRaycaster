using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

// Don't think this is available in silverlight or XBox

namespace XnaRaycaster
{
    public class RayCaster
    {
        readonly Texture2D m_RenderTarget;   // Main Screen

        readonly Int32 m_ScreenWidth;
        readonly Int32 m_ScreenHeight;

        private const Int32 TextureWidth = 64;
        private const Int32 TextureHeight = 64;

        readonly List<RayCasterTexture> m_TextureLibrary;

        readonly Double[] m_LookupTableFloorDist;   // Some cached lookup tables
        readonly Int32[,] m_FIndexLookup;           // Note: Calculations might actually be faster
        readonly Int32[,] m_CIndexLoopup;

        RayCasterTexture m_FloorTexture;
        RayCasterTexture m_CeilingTexture;
        RayCasterTexture m_CurrentTexture;
        readonly Int32[] m_Data;                    // Pixel Data to be written to the render target
        
        public RayCaster(GraphicsDevice device, Int32 screenWidth, Int32 screenHeight)
        {
            m_ScreenWidth = screenWidth;
            m_ScreenHeight = screenHeight;
            m_RenderTarget = new Texture2D(device, screenWidth, screenHeight);
            m_Data = new Int32[m_ScreenWidth * m_ScreenHeight];
            m_TextureLibrary = new List<RayCasterTexture>();

            m_LookupTableFloorDist = new double[m_ScreenHeight];
            for (int y = 0; y < m_ScreenHeight; y++)
                m_LookupTableFloorDist[y] = m_ScreenHeight / (2.0 * y - m_ScreenHeight);
            
            m_FIndexLookup = new Int32[m_ScreenWidth, screenHeight];
            for (int x = 0; x < m_ScreenWidth; x++)
                for (int y = 0; y < screenHeight; y++)
                    m_FIndexLookup[x,y] = x + (m_ScreenWidth * y);

            m_CIndexLoopup = new Int32[m_ScreenWidth, screenHeight];
            for (int x = 0; x < m_ScreenWidth; x++)
                for (int y = 0; y < screenHeight; y++)
                    m_CIndexLoopup[x, y] = x + (m_ScreenWidth * (m_ScreenHeight - y)); 
        }

        public void AddRayCasterTexture(RayCasterTexture texture)
        {
            m_TextureLibrary.Add(texture);
        }

        public Boolean EnableRenderDarkness { get; set; }

        public Texture2D Render(RayCasterCamera rayCasterCamera, RayCasterMap rayCasterMap)
        {
            // Based on the raycaster tutorial at
            // http://www.student.kuleuven.be/~m0216922/CG/raycasting.html


            // Clear the screen
            for (int i = 0; i < m_ScreenWidth * m_ScreenHeight; i++) 
                m_Data[i] = 0;

            
            // For each horizontal pixel
            for(int x=0;x<m_ScreenWidth;x++)
            {
                double cameraX = 2 * x / (double)(m_ScreenWidth) - 1; //x-coordinate in RayCasterCamera space
                double rayPosX = rayCasterCamera.Position.X;
                double rayPosY = rayCasterCamera.Position.Y;
                double rayDirX = rayCasterCamera.Direction.X + rayCasterCamera.VectorPlane.X * cameraX;
                double rayDirY = rayCasterCamera.Direction.Y + rayCasterCamera.VectorPlane.Y * cameraX;

               //which box of the RayCasterMap we're in  
                int mapX = (int)(rayPosX);
                int mapY = (int)(rayPosY);
       
                //length of ray from current position to next x or y-side
                double sideDistX;
                double sideDistY;
       
                //length of ray from one x or y-side to next x or y-side
                double deltaDistX = Math.Sqrt(1 + (rayDirY * rayDirY) / (rayDirX * rayDirX));
                double deltaDistY = Math.Sqrt(1 + (rayDirX * rayDirX) / (rayDirY * rayDirY));
                double perpWallDist;
       
                //what direction to step in x or y-direction (either +1 or -1)
                int stepX;
                int stepY;

                int hit = 0; //was there a wall hit?
                int side = 0; //was a NS or a EW wall hit?

                 //calculate step and initial sideDist
                  if (rayDirX < 0)
                  {
                    stepX = -1;
                    sideDistX = (rayPosX - mapX) * deltaDistX;
                  }
                  else
                  {
                    stepX = 1;
                    sideDistX = (mapX + 1.0 - rayPosX) * deltaDistX;
                  }
                  if (rayDirY < 0)
                  {
                    stepY = -1;
                    sideDistY = (rayPosY - mapY) * deltaDistY;
                  }
                  else
                  {
                    stepY = 1;
                    sideDistY = (mapY + 1.0 - rayPosY) * deltaDistY;
                  }


                //perform DDA
                while (hit == 0)
                {
                    //jump to next RayCasterMap square, OR in x-direction, OR in y-direction
                    if (sideDistX < sideDistY)
                    {
                        sideDistX += deltaDistX;
                        mapX += stepX;
                        side = 0;
                    }
                    else
                    {
                        sideDistY += deltaDistY;
                        mapY += stepY;
                        side = 1;
                    }

                    //Check if ray has hit a wall
                    if (rayCasterMap.WorldMap[mapX,mapY] > 0) hit = 1;
                }

                //Calculate distance projected on RayCasterCamera direction (oblique distance will give fisheye effect!)
                  if (side == 0)
                  perpWallDist = Math.Abs((mapX - rayPosX + (1 - stepX) / 2) / rayDirX);
                  else
                  perpWallDist = Math.Abs((mapY - rayPosY + (1 - stepY) / 2) / rayDirY);

                //Calculate height of line to draw on screen
                  int lineHeight = Math.Abs((int)(m_ScreenHeight / perpWallDist));
       
                  //calculate lowest and highest pixel to fill in current stripe
                  int drawStart = -lineHeight / 2 + m_ScreenHeight / 2;
                  if(drawStart < 0) 
                      drawStart = 0;
                  int drawEnd = lineHeight / 2 + m_ScreenHeight / 2;
                  if (drawEnd >= m_ScreenHeight) 
                      drawEnd = m_ScreenHeight - 1;

                //texturing calculations
                  int texNum = rayCasterMap.WorldMap[mapX,mapY] - 1; //1 subtracted from it so that texture 0 can be used!
       
                  //calculate value of wallX
                  double wallX; //where exactly the wall was hit
                  if (side == 1) 
                  {
                      wallX = rayPosX + ((mapY - rayPosY + (1 - stepY) / 2) / rayDirY) * rayDirX;
                  }
                  else
                  {
                      wallX = rayPosY + ((mapX - rayPosX + (1 - stepX) / 2) / rayDirX) * rayDirY;
                  }
                  wallX -= Math.Floor(wallX);
       
                  //x coordinate on the texture
                  var texX = (int)((wallX) * (double)TextureWidth);
                  if(side == 0 && rayDirX > 0) texX = TextureWidth - texX - 1;
                  if(side == 1 && rayDirY < 0) texX = TextureWidth - texX - 1;

                  m_CurrentTexture = m_TextureLibrary[texNum];

                  int index = x + (m_ScreenWidth * drawStart);
                  for (int y = drawStart; y < drawEnd; y++)
                  {
                      var d = y * 256 - m_ScreenHeight * 128 + lineHeight * 128;
                      var texY = ((d * TextureHeight) / lineHeight) / 256;
                      
                      // Darkness Calculations
                      var dval = (int)(perpWallDist *2);
                      dval = Math.Min(47, dval);
                      var color = m_CurrentTexture.Texture[dval][texX, texY];

                      index += m_ScreenWidth;
                      m_Data[index] = color;
                  }
                  
                // Floor Casting
                  double floorXWall, floorYWall; //x, y position of the floor texel at the bottom of the wall

                  //4 different wall directions possible
                  if(side == 0 && rayDirX > 0)
                  {
                    floorXWall = mapX;
                    floorYWall = mapY + wallX;
                  }
                  else if(side == 0 && rayDirX < 0)
                  {
                    floorXWall = mapX + 1.0;
                    floorYWall = mapY + wallX;
                  }
                  else if(side == 1 && rayDirY > 0)
                  {
                    floorXWall = mapX + wallX;
                    floorYWall = mapY;
                  }
                  else
                  {
                    floorXWall = mapX + wallX;
                    floorYWall = mapY + 1.0;
                  }

                double distWall = perpWallDist;
                  double distPlayer = 0.0;

                  m_FloorTexture = m_TextureLibrary[4];
                  m_CeilingTexture = m_TextureLibrary[4];

                  if (drawEnd < 0) 
                      drawEnd = m_ScreenHeight;
      
                  //Draw the floor from drawEnd to the bottom of the screen
                  for(int y = drawEnd + 1; y < m_ScreenHeight; y++)
                  {
                        double currentDist = m_LookupTableFloorDist[y];

                        double weight = (currentDist - distPlayer) / (distWall - distPlayer);
         
                        double currentFloorX = weight * floorXWall + (1.0 - weight) * rayCasterCamera.Position.X;
                        double currentFloorY = weight * floorYWall + (1.0 - weight) * rayCasterCamera.Position.Y;

                        const int texWidth = 64;
                        const int texHeight = 64;

                        var floorTexX = (int) (currentFloorX * 64) % texWidth;
                        var floorTexY = (int) (currentFloorY * 64) % texHeight;

                        var dval = (int)(currentDist * 2);
                        dval = Math.Min(47, dval);

                        var floorColor = m_FloorTexture.Texture[dval][floorTexY, floorTexX];
                        var ceilingColor = m_CeilingTexture.Texture[dval][floorTexY, floorTexX];

                        // Write pixel
                        m_Data[m_FIndexLookup[x,y]] = floorColor;
                        m_Data[m_CIndexLoopup[x,y]] = ceilingColor;        
                  }       
            }


            // Write the data to the texture
            m_RenderTarget.SetData(m_Data);

            return m_RenderTarget;
        }
    }
}
