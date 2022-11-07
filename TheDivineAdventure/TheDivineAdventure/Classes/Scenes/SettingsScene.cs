using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using System;

namespace TheDivineAdventure
{
    public class SettingsScene : Scene
    {
        //page 1 interactables
        private TextBox resWidth, resHeight;
        private SliderSelector masterVol,musicVol,sfxVol;
        private Texture2D settingsWindow1, settingsWindow2, settingsButton1, settingsButton2, settingsButton3;
        private Button settingsWindowed, settingsBorderless, settingsFullscreen, settingsNoAA, settingsAA2,
            settingsAA4, settingsAA8, settingsCancel, settingsApply, settingsPage1, settingsPage2;
        //page 2 interactables
        private SliderSelector sensitivity;
        private string[,] settings;
        private int page;

        public SettingsScene(SpriteBatch sb, GraphicsDeviceManager graph, Game1 parent, ContentManager cont) : base(sb, graph, parent, cont)
        {

        }

        //initialize settings Menu
        public override void Initialize()
        {
            base.Initialize();
            LoadContent();
            //get saved settings
            settings = GameSettings.ReadSettings();
            parent.settings = settings;

            //set page to audio video first
            page = 0;

            //show cursor
            parent.showCursor = true;
                        
            //Create Settings Page Button
            settingsPage1 = new Button(settingsButton3, settingsButton3, "Audio/Video", parent.smallFont,
                new Vector2(1580, 183), new Vector2(242, 110), parent.currentScreenScale);
            settingsPage1.IsActive = true;
            settingsPage2 = new Button(settingsButton3, settingsButton3, "Controls", parent.smallFont,
                new Vector2(1580, 293), new Vector2(242, 110), parent.currentScreenScale);

            #region Page 0 info
            //create resolution buttons
            resWidth = new TextBox(settings[0,1], 4,
                parent.smallFont, new Vector2(492, 325), 30, new Color(Color.Black, 60), parent);
            resHeight = new TextBox(settings[1,1], 4,
                parent.smallFont, new Vector2(682, 325), 30, new Color(Color.Black, 60), parent);

            //creat volume sliders
            masterVol = new SliderSelector(new Vector2(1126, 334), new Vector2(300, 9), parent, Content);
            masterVol.Value = float.Parse(settings[4, 1]);
            musicVol = new SliderSelector(new Vector2(1126, 415), new Vector2(300, 9), parent, Content);
            musicVol.Value = float.Parse(settings[5, 1]);
            sfxVol = new SliderSelector(new Vector2(1126, 496), new Vector2(300, 9), parent, Content);
            sfxVol.Value = float.Parse(settings[6, 1]);

            //create window buttons
            settingsWindowed = new Button(settingsButton1, settingsButton1, "Windowed", parent.smallFont,
                new Vector2(392, 404), new Vector2(180, 29), parent.currentScreenScale);

            settingsBorderless = new Button(settingsButton1, settingsButton1, "Borderless",
                parent.smallFont, new Vector2(392, 455), new Vector2(180, 29), parent.currentScreenScale);

            settingsFullscreen = new Button(settingsButton1, settingsButton1, "Fullscreen",
                parent.smallFont, new Vector2(392, 505), new Vector2(180, 29), parent.currentScreenScale);

            //set window button based on settings
            switch (Int32.Parse(settings[2, 1]))
            {
                case 0:
                    settingsWindowed.IsActive = true;
                    break;
                case 1:
                    settingsBorderless.IsActive = true;
                    break;
                case 2:
                    settingsFullscreen.IsActive = true;
                    break;
                default:
                    settingsWindowed.IsActive = true;
                    break;
            }


            //create antialiasing buttons
            settingsNoAA = new Button(settingsButton1, settingsButton1, "None",
                parent.smallFont, new Vector2(392, 585), new Vector2(180, 29), parent.currentScreenScale);

            settingsAA2 = new Button(settingsButton1, settingsButton1, "2x",
                parent.smallFont, new Vector2(392, 635), new Vector2(180, 29), parent.currentScreenScale);

            settingsAA4 = new Button(settingsButton1, settingsButton1, "4x",
                parent.smallFont, new Vector2(392, 685), new Vector2(180, 29), parent.currentScreenScale);

            settingsAA8 = new Button(settingsButton1, settingsButton1, "8x",
                parent.smallFont, new Vector2(392, 735), new Vector2(180, 29), parent.currentScreenScale);
            //set antialiasing button based on settings
            switch (Int32.Parse(settings[3, 1]))
            {
                case 0:
                    settingsNoAA.IsActive = true;
                    break;
                case 2:
                    settingsAA2.IsActive = true;
                    break;
                case 4:
                    settingsAA4.IsActive = true;
                    break;
                case 8:
                    settingsAA8.IsActive = true;
                    break;
                default:
                    settingsNoAA.IsActive = true;
                    break;
            }
            #endregion

            #region Page 1 Info

            sensitivity = new SliderSelector(new Vector2(386, 334), new Vector2(300, 9), parent, Content)
            {
                Value = (float.Parse(settings[7, 1]) - 15) / 100
            };

            #endregion

            //create back and apply buttons
            settingsCancel = new Button(settingsButton2, settingsButton2, "Cancel",
                parent.smallFont, new Vector2(70, 972), new Vector2(210, 76), parent.currentScreenScale);
            settingsApply = new Button(settingsButton2, settingsButton2, "Apply",
                parent.smallFont, new Vector2(375, 972), new Vector2(210, 76), parent.currentScreenScale);
            
        }

        public override void LoadContent()
        {
            base.LoadContent();
            settingsWindow1 = Content.Load<Texture2D>("TEX_Settings_Window");
            settingsWindow2 = Content.Load<Texture2D>("TEX_Settings_Window2");
            settingsButton1 = Content.Load<Texture2D>("TEX_Settings_Button1_Passive");
            settingsButton2 = Content.Load<Texture2D>("TEX_Settings_Button2");
            settingsButton3 = Content.Load<Texture2D>("TEX_Settings_Button3");
        }

        //update settings
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);


            //escape to exit settings without saving
            if ((Keyboard.GetState().IsKeyDown(Keys.Escape) && parent.lastKeyboard.IsKeyUp(Keys.Escape)))
            {
                settingsCancel.IsActive = true;
                parent.currentScene = parent.lastScene;
                return;
            }

            if (page == 0)
            {
                //set volume slider click states
                if (!musicVol.IsActive && !sfxVol.IsActive)
                    masterVol.IsPressed();
                if (!sfxVol.IsActive && !masterVol.IsActive)
                    musicVol.IsPressed();
                if (!musicVol.IsActive && !masterVol.IsActive)
                    sfxVol.IsPressed();
            }
            else
            {
                sensitivity.IsPressed();
            }

            //check what mouse is clicking for page selector
            if (parent.mouseState.LeftButton == ButtonState.Pressed)
            {
                if (settingsPage1.IsPressed())
                {
                    settingsPage1.IsActive = true;
                    settingsPage2.IsActive = false;
                    page = 0;
                    return;
                }
                if (settingsPage2.IsPressed())
                {
                    settingsPage1.IsActive = false;
                    settingsPage2.IsActive = true;
                    page = 1;
                    return;
                }
                //exit settings without saving
                if (settingsCancel.IsPressed())
                {
                    settingsCancel.IsActive = true;
                    parent.currentScene = parent.lastScene;
                    return;
                }

            }

            //check what mouse is clicking on page 1
            if (parent.mouseState.LeftButton == ButtonState.Pressed && page == 0)
            {
                //apply settings
                if (settingsApply.IsPressed())
                {
                    bool resChanged = false;
                    if (_graphics.PreferredBackBufferWidth != Int32.Parse(resWidth.Text))
                    {
                        _graphics.PreferredBackBufferWidth = Int32.Parse(resWidth.Text);
                        resChanged = true;
                        settings[0, 1] = resWidth.Text;
                        parent.settings[0, 1] = resWidth.Text;
                    }
                    if (_graphics.PreferredBackBufferHeight != Int32.Parse(resHeight.Text))
                    {
                        _graphics.PreferredBackBufferHeight = Int32.Parse(resHeight.Text);
                        resChanged = true;
                        settings[1, 1] = resHeight.Text;
                        parent.settings[1, 1] = resHeight.Text;
                    }
                    if (resChanged)
                    {
                        _graphics.ApplyChanges();
                    }
                    //switch to windowed
                    if (settingsWindowed.IsActive == true  && parent.Window.IsBorderless)
                    {
                        if (_graphics.IsFullScreen)
                        {
                            _graphics.ToggleFullScreen();
                            _graphics.ApplyChanges();
                        }

                        parent.Window.IsBorderless = false;
                        parent.Window.Position = new Point(parent.Window.Position.X+10, parent.Window.Position.Y+10);
                        settings[2, 1] = "0";
                        parent.settings[2, 1] = "0";
                    }//switch to borderless window
                    else if (settingsBorderless.IsActive == true)
                    {
                        if (_graphics.IsFullScreen)
                        {
                            _graphics.ToggleFullScreen();
                            _graphics.ApplyChanges();
                        }
                        if (!parent.Window.IsBorderless)
                        {
                            parent.Window.IsBorderless = true;
                            parent.Window.Position = new Point(0, 0);
                        }
                        settings[2, 1] = "1";
                        parent.settings[2, 1] = "1";
                    }//switch to fullscreen
                    else if (settingsFullscreen.IsActive == true && !_graphics.IsFullScreen) {
                        parent.Window.IsBorderless = true;
                        _graphics.ToggleFullScreen();
                        _graphics.ApplyChanges();

                        settings[2, 1] = "2";
                        parent.settings[2, 1] = "2";
                    }

                    //set antialiasing preferences
                    if (settingsNoAA.IsActive == true)
                    {
                        _graphics.PreferMultiSampling = false;
                        parent.GraphicsDevice.PresentationParameters.MultiSampleCount = 0;

                        settings[3, 1] = "0";
                        parent.settings[3, 1] = "0";
                    }
                    else if (settingsAA2.IsActive == true)
                    {
                        _graphics.PreferMultiSampling = true;
                        parent.GraphicsDevice.PresentationParameters.MultiSampleCount = 2;

                        settings[3, 1] = "1";
                        parent.settings[3, 1] = "1";
                    }
                    else if (settingsAA4.IsActive == true)
                    {
                        _graphics.PreferMultiSampling = true;
                        parent.GraphicsDevice.PresentationParameters.MultiSampleCount = 4;

                        settings[3, 1] = "2";
                        parent.settings[3, 1] = "2";
                    }
                    else if (settingsAA8.IsActive == true)
                    {
                        _graphics.PreferMultiSampling = true;
                        parent.GraphicsDevice.PresentationParameters.MultiSampleCount = 8;

                        settings[3, 1] = "3";
                        parent.settings[3, 1] = "3";
                    }
                    parent.currentScreenScale = new Vector2(_graphics.PreferredBackBufferWidth / 1920f, _graphics.PreferredBackBufferHeight / 1080f);

                    //set volume preferences
                    settings[4, 1] = (masterVol.Value).ToString();
                    parent.settings[4, 1] = (masterVol.Value).ToString();
                    settings[5, 1] = (musicVol.Value).ToString();
                    parent.settings[5, 1] = (musicVol.Value).ToString();
                    settings[6, 1] = (sfxVol.Value).ToString();
                    parent.settings[6, 1] = (sfxVol.Value).ToString();

                    MediaPlayer.Volume = float.Parse(settings[4, 1]) * float.Parse(settings[5, 1]);
                    Player.volume = float.Parse(settings[4, 1]) * float.Parse(settings[6, 1]);
                    Enemy.volume = float.Parse(settings[4, 1]) * float.Parse(settings[6, 1]);

                    GameSettings.WriteSettings(settings);
                    if (parent.lastScene == "TITLE")
                        parent.titleScene.Initialize();
                    if (parent.lastScene == "PAUSE")
                        parent.pauseScene.Initialize();
                    this.Initialize();

                    settingsApply.IsActive = true;
                    parent.currentScene = parent.lastScene;
                    return;
                }
                //update volumebars
                masterVol.Update();
                musicVol.Update();
                sfxVol.Update();

                //update texboxes;
                resWidth.IsPressed();
                resHeight.IsPressed();

                //update window buttons
                if (settingsWindowed.IsPressed())
                {
                    settingsWindowed.IsActive = true;
                    settingsFullscreen.IsActive = false;
                    settingsBorderless.IsActive = false;
                    return;
                }
                if (settingsBorderless.IsPressed())
                {
                    settingsBorderless.IsActive = true;
                    settingsFullscreen.IsActive = false;
                    settingsWindowed.IsActive = false;
                    return;
                }
                if (settingsFullscreen.IsPressed())
                {
                    settingsFullscreen.IsActive = true;
                    settingsBorderless.IsActive = false;
                    settingsWindowed.IsActive = false;
                    return;
                }

                //update antialiasing buttons
                if (settingsNoAA.IsPressed())
                {
                    settingsNoAA.IsActive = true;
                    settingsAA2.IsActive = false;
                    settingsAA4.IsActive = false;
                    settingsAA8.IsActive = false;
                    return;
                }
                if (settingsAA2.IsPressed())
                {
                    settingsNoAA.IsActive = false;
                    settingsAA2.IsActive = true;
                    settingsAA4.IsActive = false;
                    settingsAA8.IsActive = false;
                    return;
                }
                if (settingsAA4.IsPressed())
                {
                    settingsNoAA.IsActive = false;
                    settingsAA2.IsActive = false;
                    settingsAA4.IsActive = true;
                    settingsAA8.IsActive = false;
                    return;
                }
                if (settingsAA8.IsPressed())
                {
                    settingsNoAA.IsActive = false;
                    settingsAA2.IsActive = false;
                    settingsAA4.IsActive = false;
                    settingsAA8.IsActive = true;
                    return;
                }
            }

            //check what mouse is clicking on page 2
            if (parent.mouseState.LeftButton == ButtonState.Pressed && page == 1)
            {
                sensitivity.Update();
                if (settingsApply.IsPressed())
                {
                    settings[7, 1] = (Math.Round((sensitivity.Value+.15)*100,0)).ToString();
                    parent.settings[7, 1] = settings[7, 1];
                    if(parent.lastScene == "PAUSE")
                    {
                        parent.playScene.player.sensitivity = (int)Math.Round((sensitivity.Value + .15) * 100, 0);
                    }

                    settingsApply.IsActive = true;
                    parent.currentScene = parent.lastScene;
                    GameSettings.WriteSettings(settings);
                    return;
                }
            }
            resWidth.Update(gameTime);
            resHeight.Update(gameTime);

        }

        //Draw settings
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            if (parent.lastScene == "TITLE")
                parent.titleScene.Draw(gameTime);
            else
            {
                parent.playScene.Draw(gameTime);
                parent.pauseScene.Draw(gameTime);
            }
            parent.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            parent.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            _spriteBatch.Begin();

            //draw page buttons
            settingsPage1.DrawButton(_spriteBatch);
            settingsPage2.DrawButton(_spriteBatch);

            if (page == 0)
            {
                //draw window
                _spriteBatch.Draw(settingsWindow1, Vector2.Zero, null,
                    Color.White, 0, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 0);

                //resolution settings
                _spriteBatch.DrawString(parent.smallFont, "Width: ",
                    new Vector2(392 * parent.currentScreenScale.X, 325 * parent.currentScreenScale.Y),
                    Color.Black, 0f, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 1);
                resWidth.Draw(_spriteBatch);
                _spriteBatch.DrawString(parent.smallFont, "Height: ", new Vector2(582 * parent.currentScreenScale.X, 325 * parent.currentScreenScale.Y),
                    Color.Black, 0f, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 1);
                resHeight.Draw(_spriteBatch);

                //draw window Options
                settingsWindowed.DrawButton(_spriteBatch);
                settingsFullscreen.DrawButton(_spriteBatch);
                settingsBorderless.DrawButton(_spriteBatch);

                //draw antialiasing buttons
                settingsNoAA.DrawButton(_spriteBatch);
                settingsAA2.DrawButton(_spriteBatch);
                settingsAA4.DrawButton(_spriteBatch);
                settingsAA8.DrawButton(_spriteBatch);

                //draw volume options
                masterVol.Draw(_spriteBatch, gameTime);
                _spriteBatch.DrawString(parent.smallFont, Math.Round(masterVol.Value * 100, 0).ToString() + "%",
                    new Vector2(1440 * parent.currentScreenScale.X, 322 * parent.currentScreenScale.Y),
                    parent.textGold, 0f, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 1);

                musicVol.Draw(_spriteBatch, gameTime);
                _spriteBatch.DrawString(parent.smallFont, Math.Round(musicVol.Value * 100, 0).ToString() + "%",
                    new Vector2(1440 * parent.currentScreenScale.X, 403 * parent.currentScreenScale.Y),
                    parent.textGold, 0f, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 1);

                sfxVol.Draw(_spriteBatch, gameTime);
                _spriteBatch.DrawString(parent.smallFont, Math.Round(sfxVol.Value * 100, 0).ToString() + "%",
                    new Vector2(1440 * parent.currentScreenScale.X, 484 * parent.currentScreenScale.Y),
                    parent.textGold, 0f, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 1);
            }else if (page == 1)
            {
                //draw window
                _spriteBatch.Draw(settingsWindow2, Vector2.Zero, null,
                    Color.White, 0, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 0);

                //draw volume options
                sensitivity.Draw(_spriteBatch, gameTime);
                _spriteBatch.DrawString(parent.smallFont, Math.Round((sensitivity.Value+.15)*100, 0).ToString(),
                    new Vector2(700 * parent.currentScreenScale.X, 322 * parent.currentScreenScale.Y),
                    parent.textGold, 0f, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 1);
            }

            //draw close and apply buttons
            settingsCancel.DrawButton(_spriteBatch);
            settingsApply.DrawButton(_spriteBatch);
            _spriteBatch.End();
            parent.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            parent.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
        }

    }
}
