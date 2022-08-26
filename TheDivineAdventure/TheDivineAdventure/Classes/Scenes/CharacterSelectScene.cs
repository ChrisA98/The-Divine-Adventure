using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

using System.Collections.Generic;

namespace TheDivineAdventure
{
    public class CharacterSelectScene : Scene
    {
        public static readonly string[] ROLES = { "WARRIOR", "ROGUE", "MAGE", "CLERIC" };
        private SpriteFont font, bigFont;
        private Texture2D backdrop, frontplate, arrowButton1, arrowButton2, actionButton, emberSheet01;
        private AnimatedSprite[] titleEmbers;
        private Button leftArrow, rightArrow, confirm, back;
        private Model warriorModel, rogueModel, mageModel, clericModel, stone;
        private Player characters;
        private List<SoundEffect> playerSounds = new List<SoundEffect>();
        private string currentChar;
        private Matrix worldStone, worldPlayer, proj;

        public CharacterSelectScene(SpriteBatch sb, GraphicsDeviceManager graph, Game1 game, ContentManager cont) : base(sb, graph, game, cont)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            // Declare first character view
            currentChar = "CLERIC";

            // Initialize game objects
            characters = new Player(playerSounds, currentChar, parent);

            // Set camera data
            proj = Matrix.CreatePerspectiveFieldOfView(
                                MathHelper.ToRadians(60),
                                parent.GraphicsDevice.Viewport.AspectRatio,
                                0.05f,
                                1000);

            //create buttons
            leftArrow = new Button(arrowButton1, arrowButton1, null, font, new Vector2(254, 910),
                new Vector2(179, 155), parent.currentScreenScale);
            rightArrow = new Button(arrowButton2, arrowButton2, null, font, new Vector2(719, 911),
                new Vector2(179, 155), parent.currentScreenScale);
            confirm = new Button(actionButton, actionButton, "Confirm", font, new Vector2(1633, 807),
                new Vector2(210, 76), parent.currentScreenScale);
            back = new Button(actionButton, actionButton, "Return", font, new Vector2(48, 48),
                new Vector2(210, 76), parent.currentScreenScale);


            //create embers
            titleEmbers = new AnimatedSprite[30];
            for (int i = 0; i < titleEmbers.Length; i++)
            {
                titleEmbers[i] = new AnimatedSprite(174, 346, emberSheet01, 6);
                titleEmbers[i].Pos = new Vector2(rand.Next(1920) * parent.currentScreenScale.X, rand.Next(450, 750) * parent.currentScreenScale.Y);
                titleEmbers[i].Scale = 1 - (rand.Next(-200, 50) / 100f);
                titleEmbers[i].Frame = rand.Next(6);
            }
        }

        public override void LoadContent()
        {
            base.LoadContent();
            //load font
            bigFont = Content.Load<SpriteFont>("BigFont");
            font = Content.Load<SpriteFont>("SmallFont");

            //load 2d textures
            backdrop = Content.Load<Texture2D>("TEX_CharacterSelect_Backdrop");
            emberSheet01 = Content.Load<Texture2D>("TEX_EmberSheet01");
            frontplate = Content.Load<Texture2D>("TEX_CharacterSelect_FrontPlate");
            arrowButton1 = Content.Load<Texture2D>("TEX_LArrowButton1");
            arrowButton2 = Content.Load<Texture2D>("TEX_RArrowButton1");
            actionButton = Content.Load<Texture2D>("TEX_Settings_Button2");

            // Load 3D models
            warriorModel = Content.Load<Model>("MODEL_Warrior");
            rogueModel = Content.Load<Model>("MODEL_Rogue");
            clericModel = Content.Load<Model>("MODEL_Cleric");
            mageModel = Content.Load<Model>("MODEL_Mage");
            stone = Content.Load<Model>("MODEL_Selection_Stone");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);


            //click detection

            if (parent.mouseState.LeftButton == ButtonState.Pressed && parent.lastMouseState.LeftButton != ButtonState.Pressed)
            {
                if (leftArrow.IsPressed())
                {
                    switch (currentChar)
                    {
                        case "CLERIC":
                            currentChar = "MAGE";
                            characters = new Player(playerSounds, currentChar, parent);
                            break;
                        case "MAGE":
                            currentChar = "ROGUE";
                            characters = new Player(playerSounds, currentChar, parent);
                            break;
                        case "ROGUE":
                            currentChar = "WARRIOR";
                            characters = new Player(playerSounds, currentChar, parent);
                            break;
                        default:
                            currentChar = "CLERIC";
                            characters = new Player(playerSounds, currentChar, parent);
                            break;
                    }
                }
                if (rightArrow.IsPressed())
                {
                    switch (currentChar)
                    {
                        case "WARRIOR":
                            currentChar = "ROGUE";
                            characters = new Player(playerSounds, currentChar, parent);
                            break;
                        case "ROGUE":
                            currentChar = "MAGE";
                            characters = new Player(playerSounds, currentChar, parent);
                            break;
                        case "MAGE":
                            currentChar = "CLERIC";
                            characters = new Player(playerSounds, currentChar, parent);
                            break;
                        default:
                            currentChar = "WARRIOR";
                            characters = new Player(playerSounds, currentChar, parent);
                            break;
                    }

                }
                if (confirm.IsPressed())
                {
                    parent.playerRole = currentChar;
                    parent.currentScene = "PLAY";
                    parent.ReloadContent();
                    parent.playScene.Initialize();
                    return;
                }
                if (back.IsPressed())
                {
                    parent.currentScene = "LEVEL_SELECT";
                    parent.ReloadContent();
                    parent.levelSelectScene.Initialize();
                    return;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            //Draw background
            parent.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            parent.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            _spriteBatch.Begin();
            //backdrop
            _spriteBatch.Draw(backdrop, Vector2.Zero, null, Color.White, 0, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 0);

            _spriteBatch.End();
            parent.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            parent.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;


            //Render Ground Stone
            worldStone = Matrix.CreateScale(0.07f) *
                        Matrix.CreateRotationY(MathHelper.ToRadians(characters.Rot.Y)) *
                        Matrix.CreateTranslation(new Vector3(0,-4,0));
            stone.Draw(worldStone, Matrix.CreateLookAt(new Vector3(8, 15, 40), new Vector3(15, 13, 0), Vector3.Up), proj);

            // Render character
            worldPlayer = Matrix.CreateScale(1f) *
                        Matrix.CreateRotationY(MathHelper.ToRadians(characters.Rot.Y)) *
                        Matrix.CreateTranslation(characters.Pos);

            switch (currentChar)
            {
                case "WARRIOR":
                    warriorModel.Draw(worldPlayer, Matrix.CreateLookAt(new Vector3(8, 15, 40), characters.Pos + new Vector3(15, 0, 0), Vector3.Up), proj);
                    break;
                case "ROGUE":
                    rogueModel.Draw(worldPlayer, Matrix.CreateLookAt(new Vector3(8, 15, 40), characters.Pos + new Vector3(15, 0, 0), Vector3.Up), proj);
                    break;
                case "MAGE":
                    mageModel.Draw(worldPlayer, Matrix.CreateLookAt(new Vector3(8, 15, 40), characters.Pos + new Vector3(15, 0, 0), Vector3.Up), proj);
                    break;
                case "CLERIC":
                    clericModel.Draw(worldPlayer, Matrix.CreateLookAt(new Vector3(8,15,40), characters.Pos+new Vector3(15,0,0), Vector3.Up), proj);
                    break;
            }

            _spriteBatch.Begin();

            //draw embers
            foreach (AnimatedSprite ember in titleEmbers)
            {
                if (ember.Frame == 0)
                {
                    ember.Pos = new Vector2((rand.Next(1920)) * parent.currentScreenScale.X, rand.Next(450, 750) * parent.currentScreenScale.Y);
                    ember.Scale = 1 - (rand.Next(-200, 50) / 100f);
                }
                ember.Draw(_spriteBatch, parent.currentScreenScale);
            }
            //Draw front plate
            _spriteBatch.Draw(frontplate, Vector2.Zero, null, Color.White, 0, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 0);

            //Draw character title on front plate
            _spriteBatch.DrawString(bigFont, currentChar, new Vector2(1052, 143) * parent.currentScreenScale, new Color(Color.Black,90), 0f, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 1);
            _spriteBatch.DrawString(bigFont, currentChar, new Vector2(1055,140) * parent.currentScreenScale,
                parent.textGold, 0f, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 1);
            //Draw character info on front plate
            _spriteBatch.DrawString(font, "Health: "+characters.healthMax, new Vector2(1064, 273) * parent.currentScreenScale,
                parent.textGold, 0f, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 1);
            if(characters.IsCaster)
                _spriteBatch.DrawString(font, "Mana: " + characters.secondary, new Vector2(1064, 303) * parent.currentScreenScale,
                    parent.textGold, 0f, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 1);
            else
                _spriteBatch.DrawString(font, "Stamina: " + characters.secondary, new Vector2(1064, 303) * parent.currentScreenScale,
                    parent.textGold, 0f, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 1);
            _spriteBatch.DrawString(font, "Speed: " + characters.initSpeed, new Vector2(1064, 333) * parent.currentScreenScale,
                parent.textGold, 0f, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 1);
            _spriteBatch.DrawString(font, "Stamina: " + characters.secondary, new Vector2(1064, 363) * parent.currentScreenScale,
                parent.textGold, 0f, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 1);
            _spriteBatch.DrawString(font, "Jump Speed: " + characters.jumpSpeed, new Vector2(1064, 393) * parent.currentScreenScale,
                parent.textGold, 0f, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 1);
            _spriteBatch.DrawString(font, "Stamina: " + characters.secondary, new Vector2(1064, 423) * parent.currentScreenScale,
                parent.textGold, 0f, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 1);
            if (characters.IsCaster)
                _spriteBatch.DrawString(font, "Mana Regen: " + characters.secondaryRegenRate*60+"/per second", new Vector2(1064, 453) * parent.currentScreenScale,
                    parent.textGold, 0f, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 1);
            else
                _spriteBatch.DrawString(font, "Stamina Regen: " + characters.secondaryRegenRate * 60 + "/per second", new Vector2(1064, 453) * parent.currentScreenScale,
                    parent.textGold, 0f, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 1);


            //draw buttons
            leftArrow.DrawButton(_spriteBatch);
            rightArrow.DrawButton(_spriteBatch);
            confirm.DrawButton(_spriteBatch);
            back.DrawButton(_spriteBatch);

            //draw fade in
            FadeIn();
            _spriteBatch.End();

        }


    }

}
