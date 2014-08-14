using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace XnaRaycaster
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        readonly GraphicsDeviceManager m_Graphics;
        SpriteBatch m_SpriteBatch;
        SpriteFont m_Font1;

        int m_FrameRate = 0;
        int m_FrameCounter = 0;
        TimeSpan m_ElapsedTime = TimeSpan.Zero;

        private const Int32 ScreenWidth = 640;
        private const Int32 ScreenHeight = 480;

        private const Int32 RayCastRenderWidth = 640;
        private const Int32 RayCastRenderHeight = 480;

        RayCasterCamera m_RayCasterCamera;
        RayCasterMap m_RayCasterMap;
        RayCaster m_RayCaster;
      
        public Game1()
        {
            m_Graphics = new GraphicsDeviceManager(this)
                {
                    PreferredBackBufferWidth = ScreenWidth,
                    PreferredBackBufferHeight = ScreenHeight,
                    IsFullScreen = false,
                    SynchronizeWithVerticalRetrace = false
                };

            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            m_SpriteBatch = new SpriteBatch(GraphicsDevice);
        
            m_RayCaster = new RayCaster(m_Graphics.GraphicsDevice, RayCastRenderWidth, RayCastRenderHeight);

            m_Font1 = Content.Load<SpriteFont>("Courier New");
            
            m_RayCaster.AddRayCasterTexture(new RayCasterTexture(Content.Load<Texture2D>("Textures/242")));
            m_RayCaster.AddRayCasterTexture(new RayCasterTexture(Content.Load<Texture2D>("Textures/244")));
            m_RayCaster.AddRayCasterTexture(new RayCasterTexture(Content.Load<Texture2D>("Textures/241")));
            m_RayCaster.AddRayCasterTexture(new RayCasterTexture(Content.Load<Texture2D>("Textures/203")));
            m_RayCaster.AddRayCasterTexture(new RayCasterTexture(Content.Load<Texture2D>("Textures/245")));
            m_RayCaster.AddRayCasterTexture(new RayCasterTexture(Content.Load<Texture2D>("Textures/233"))); 

            m_RayCasterCamera = new RayCasterCamera();
            m_RayCasterMap = new RayCasterMap();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Frame rate counter
            m_ElapsedTime += gameTime.ElapsedGameTime;
            if (m_ElapsedTime > TimeSpan.FromSeconds(1))
            {
                m_ElapsedTime -= TimeSpan.FromSeconds(1);
                m_FrameRate = m_FrameCounter;
                m_FrameCounter = 0;
            }

            KeyboardState keys = Keyboard.GetState();
            
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                this.Exit();
            }
            if (keys.IsKeyDown(Keys.Escape)) this.Exit();
            
            // Update the RayCasterCamera
            m_RayCasterCamera.Update(gameTime);

            // Move forward if no wall in front of you
            if(keys.IsKeyDown(Keys.W) || keys.IsKeyDown(Keys.Up))
            {
                // TODO make collision detection nicerr
                if (m_RayCasterMap.WorldMap[(Int32)(m_RayCasterCamera.Position.X + m_RayCasterCamera.Direction.X), (Int32)(m_RayCasterCamera.Position.Y)] == 0)
                {
                    if (m_RayCasterMap.WorldMap[(Int32) (m_RayCasterCamera.Position.X), (Int32) (m_RayCasterCamera.Position.Y + m_RayCasterCamera.Direction.Y)] == 0)
                    {
                        m_RayCasterCamera.MoveForward();
                    }
                }
            }

            //move backwards if no wall behind you
            if (keys.IsKeyDown(Keys.S) || keys.IsKeyDown(Keys.Down))
            {
                if (m_RayCasterMap.WorldMap[(Int32)(m_RayCasterCamera.Position.X - m_RayCasterCamera.Direction.X), (Int32)(m_RayCasterCamera.Position.Y)] == 0)
                {
                    if (m_RayCasterMap.WorldMap[(Int32)(m_RayCasterCamera.Position.X), (Int32)(m_RayCasterCamera.Position.Y - m_RayCasterCamera.Direction.Y)] == 0)
                    {
                        m_RayCasterCamera.MoveBackwards();
                    }
                }
            }

            // Rotate to the right
            if (keys.IsKeyDown(Keys.A) || keys.IsKeyDown(Keys.Left))
            {
                m_RayCasterCamera.RotateLeft();
            }

            // Rotate to the left
            if (keys.IsKeyDown(Keys.D) || keys.IsKeyDown(Keys.Right))
            {
                m_RayCasterCamera.RotateRight();
            }

            // Change render modes
            if (keys.IsKeyDown(Keys.D1))
            {
                m_RayCaster.EnableRenderDarkness = false;
            }
            if (keys.IsKeyDown(Keys.D2))
            {
                m_RayCaster.EnableRenderDarkness = true;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            m_FrameCounter++;
            GraphicsDevice.Clear(Color.Black);        
            
            // Draw Main Screen
            m_SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap,DepthStencilState.Default,RasterizerState.CullNone);

                var screen = m_RayCaster.Render(m_RayCasterCamera,m_RayCasterMap);
                
                m_SpriteBatch.Draw(screen, new Rectangle((ScreenWidth - RayCastRenderWidth) / 2, 0, RayCastRenderWidth, RayCastRenderHeight), new Rectangle(0, 0, RayCastRenderWidth, RayCastRenderHeight), Color.White);
               
            m_SpriteBatch.End();

            // Render FrameRate
            m_SpriteBatch.Begin();
                m_SpriteBatch.DrawString(m_Font1, "FrameRate :" + m_FrameRate.ToString(), Vector2.Zero, Color.White);
            m_SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
