using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nestacular.NESCore;
using System;
using Nestacular.NESCore.CPUCore;
using NestacularFrontend.Input;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using System.Text;
using System.IO;
using System.Collections.Generic;
using SkiaSharp;

namespace Nestacular
{
    public class GUI : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        Texture2D outputTexture = null;
        private NES _nes = new NES();
        private SpriteFont font;
        private Frame _frame;
        //string framecalc = "";
        long prevFrameCalc;
        //RenderTarget2D rt;
        private int frameCount;
        private int lagFrames;
        //private GameWindow outputWindow;

        private string _cpuStatus;
        public GUI()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _nes.Cart.Insert("Resources/nestest.nes");
            _nes.RunEngine(true);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            //rt = new RenderTarget2D(GraphicsDevice, 256, 240);
            font = Content.Load<SpriteFont>("CascadiaMono");
        }

        protected override void Update(GameTime gameTime)
        {

            //so really what needs to happen, is GenerateFrame, stores a frame object in the 'buffer'
            //and then on update, we retrieve the frame object and draw it.
            //on the getter of the the stored frame, we can set a flag to flush the frame buffer and allow another frame to be buffered
            //that means the cpu will fill the frame buffer, pause and wait for the next frame.
            //we will also, at that point, set the inputs for the next calculated frame.
            //so it goes,
            //set inputs for next,
            //get current
            //clear buffer
            //fill next (with the current inputs)
            //set buffer ready.
            _frame = null;
            _frame = _nes.FrameBuffer;
            if (_frame != null)
            {
                //AGH
                frameCount++;
                var bmp = _frame.GetFrame();

                outputTexture = new Texture2D(GraphicsDevice, 256, 240);
                var pixelData = _frame.GetPixelData();
                outputTexture.SetData(0, 0, null, pixelData, 0, pixelData.Length);
                //


            }


            else //frame is not ready, either it is stuck, or the framerate is too high and it hasn't loaded another frame yet.
                lagFrames++;




            _cpuStatus = _nes.CPUStatus.ToString();

            Keyboard.GetState();
            if (Keyboard.HasBeenPressed(Keys.Left))
            {
                //pause and start going backwards step by step

            }
            if (Keyboard.HasBeenPressed(Keys.Down))
            {
                _nes.ToggleExecutionMode();
            }
            if (Keyboard.HasBeenPressed(Keys.Right))
            {
                //pause and start going forward step by step.

                _nes.ExecutionBlocker.Set();
            }
            if (Keyboard.HasBeenPressed(Keys.Space))
            {
                //toggle pause of execution;
                _nes.Pause = !_nes.Pause;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();

            _spriteBatch.DrawString(font, _cpuStatus, new Vector2(0, 0), Color.Black);

            
            StringBuilder history = new StringBuilder();
            for (int i = 0; i < 8; i++)
            {
                if (_nes.InstructionHistory.Count < 8) continue;
                history.Append(_nes.InstructionHistory[7 - i]);
                history.Append("\n");
            }
            _spriteBatch.DrawString(font, history, new Vector2(0, 350), Color.Black); //8 rows of historical instructions

            _spriteBatch.DrawString(font, $"{(gameTime.IsRunningSlowly ? "Running Slow" : "Running At Speed")}", new Vector2(400, 0), Color.Black);
            _spriteBatch.DrawString(font, $"Frame Time: {gameTime.ElapsedGameTime}", new Vector2(400, 20), Color.Black);
            _spriteBatch.DrawString(font, $"Engine Frames: {gameTime.TotalGameTime / gameTime.ElapsedGameTime}", new Vector2(400, 40), Color.Black);
            _spriteBatch.DrawString(font, $"NES Frames: {frameCount}", new Vector2(400, 60), Color.Black);
            _spriteBatch.DrawString(font, $"Execution Mode: {Enum.GetName(typeof(NES.ExecutionMode), _nes.CurrentExecutionMode)}", new Vector2(400, 80), Color.Black);
            _spriteBatch.DrawString(font, $"Lag Frames: {lagFrames}", new Vector2(400, 100), Color.Black);

            
            if (_frame != null)
            {
                prevFrameCalc = _frame.CalcTime;
            }
            _spriteBatch.DrawString(font, $"Emulation Frame Calc Time (MS): {prevFrameCalc}", new Vector2(400, 120), Color.Black);

            _spriteBatch.DrawString(font, $"Output: ", new Vector2(400, 140), Color.Black);
            if (outputTexture != null)
                _spriteBatch.Draw(outputTexture, new Vector2(400, 160), Color.Black);




            _spriteBatch.End();

            base.Draw(gameTime);
        }



    }
}