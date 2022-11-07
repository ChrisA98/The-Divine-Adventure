using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;


using System;
using System.Collections.Generic;
using System.IO;
namespace TheDivineAdventure
{
    public class LevelEndScene : Scene
    {
        private SpriteFont mainFont, buttonFont;
        private Texture2D backdrop, frontPlate, actionButton, emberSheet01;
        private AnimatedSprite[] titleEmbers;
        private Button playAgain, levelSelect, mainMenu;
        private string currentRole;
        public string currentScore;
        private List<(string Role, int Score)> highscores;
        private bool setHigh;

        public LevelEndScene(SpriteBatch sb, GraphicsDeviceManager graph, Game1 game, ContentManager cont) : base(sb, graph, game, cont)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            //create buttons
            playAgain = new Button(actionButton, actionButton, "Play Again", buttonFont, new Vector2(579, 464),
                new Vector2(210, 76), parent.currentScreenScale);
            levelSelect = new Button(actionButton, actionButton, "Level Select", buttonFont, new Vector2(579, 564),
                new Vector2(210, 76), parent.currentScreenScale);
            mainMenu = new Button(actionButton, actionButton, "Main Menu", buttonFont, new Vector2(579, 664),
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

            //show cursor
            parent.showCursor = true;

            //set current character
            currentRole = parent.playerRole;
            currentRole = currentRole.Substring(0, 1).ToUpper() + currentRole.Substring(1, currentRole.Length-1).ToLower();


            //populate high scores
            highscores = GetHighScore();

            setHigh = (Int32.Parse(currentScore) == highscores[0].Score);

            WriteScores();

        }

        public override void LoadContent()
        {
            base.LoadContent();

            //Load 2D Assets
            backdrop = Content.Load<Texture2D>("TEX_LevelEndScreen_Back");
            frontPlate = Content.Load<Texture2D>("TEX_LevelEndScreen_Front");
            actionButton = Content.Load<Texture2D>("TEX_Settings_Button2");
            emberSheet01 = Content.Load<Texture2D>("TEX_EmberSHeet01");

            //Load Font
            mainFont = Content.Load<SpriteFont>("BigFont");
            buttonFont = Content.Load<SpriteFont>("SmallFont");

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (parent.mouseState.LeftButton == ButtonState.Pressed && parent.lastMouseState.LeftButton != ButtonState.Pressed)
            {
                if (playAgain.IsPressed())
                {
                    parent.currentScene = "PLAY";
                    parent.ReloadContent();
                    parent.playScene.Initialize();
                    return;
                }
                if (levelSelect.IsPressed())
                {
                    parent.currentScene = "LEVEL_SELECT";
                    parent.ReloadContent();
                    parent.levelSelectScene.Initialize();
                    return;
                }
                if (mainMenu.IsPressed())
                {
                    parent.currentScene = "TITLE_SCREEN";
                    parent.ReloadContent();
                    parent.titleScene.Initialize();
                    return;
                }
            }

        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            _spriteBatch.Begin();


            //backdrop
            _spriteBatch.Draw(backdrop, Vector2.Zero, null, Color.White, 0, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 0);


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
            _spriteBatch.Draw(frontPlate, Vector2.Zero, null, Color.White, 0, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 0);

            //draw left text and dropshadows
            if (setHigh)
            {
                _spriteBatch.DrawString(mainFont, currentScore + " New High Score!", new Vector2(652, 265) * parent.currentScreenScale,
                     Color.White, 0f, Vector2.Zero, parent.currentScreenScale * 0.35f, SpriteEffects.None, 1);
                _spriteBatch.DrawString(mainFont, currentScore + " New High Score!", new Vector2(653, 264) * parent.currentScreenScale,
                    parent.textGold, 0f, Vector2.Zero, parent.currentScreenScale * 0.35f, SpriteEffects.None, 1);
            }
            else
            {
                _spriteBatch.DrawString(mainFont, currentScore, new Vector2(652, 255) * parent.currentScreenScale,
                     Color.White, 0f, Vector2.Zero, parent.currentScreenScale * 0.5f, SpriteEffects.None, 1);
                _spriteBatch.DrawString(mainFont, currentScore, new Vector2(653, 254) * parent.currentScreenScale,
                    parent.textGold, 0f, Vector2.Zero, parent.currentScreenScale * 0.5f, SpriteEffects.None, 1);

            }
            _spriteBatch.DrawString(mainFont, currentRole, new Vector2(814, 314) * parent.currentScreenScale,
                Color.White, 0f, Vector2.Zero, parent.currentScreenScale * 0.5f, SpriteEffects.None, 1);
            _spriteBatch.DrawString(mainFont, currentRole, new Vector2(815, 313) * parent.currentScreenScale,
                parent.textGold, 0f, Vector2.Zero, parent.currentScreenScale * 0.5f, SpriteEffects.None, 1);

            //draw high scores
            for(int i = 0;i<7;i++)
            {
                if (i < highscores.Count)
                {
                    _spriteBatch.DrawString(mainFont, i+1 + ". " + highscores[i].Role + ": " + highscores[i].Score, new Vector2(1122, 345+ (i * 60)) * parent.currentScreenScale,
                        Color.White, 0f, Vector2.Zero, parent.currentScreenScale * 0.35f, SpriteEffects.None, 1);
                    _spriteBatch.DrawString(mainFont, i+1 + ". " + highscores[i].Role + ": " + highscores[i].Score, new Vector2(1124, 344 + (i * 60)) * parent.currentScreenScale,
                        parent.textGold, 0f, Vector2.Zero, parent.currentScreenScale * 0.35f, SpriteEffects.None, 1);
                }
                else
                {
                    _spriteBatch.DrawString(mainFont, i + 1 + ". None : 0000", new Vector2(1122, 345 + (i * 60)) * parent.currentScreenScale,
                        Color.White, 0f, Vector2.Zero, parent.currentScreenScale * 0.35f, SpriteEffects.None, 1);
                    _spriteBatch.DrawString(mainFont, i + 1 + ". None : 0000", new Vector2(1124, 344 + (i * 60)) * parent.currentScreenScale,
                        parent.textGold, 0f, Vector2.Zero, parent.currentScreenScale * 0.35f, SpriteEffects.None, 1);

                }
            }


            //draw Buttons
            playAgain.DrawButton(_spriteBatch);
            levelSelect.DrawButton(_spriteBatch);
            mainMenu.DrawButton(_spriteBatch);

            //fade in
            FadeIn();

            _spriteBatch.End();
        }

        private List<(string Role, int Score)> GetHighScore()
        {
            //define file path
            string filePath = Directory.GetCurrentDirectory() + @"\Level1_HighScores.txt";
            //write file to array
            string[] text = File.ReadAllLines(filePath);
            //creat empty tuple list
            List<(string Role, int Score)> scoreList = new List<(string Role, int Score)>();

            //populate Tuple list
            foreach (string line in text)
            {
                (string Role, int Score) output;
                output.Role = line.Substring(0, line.LastIndexOf(':'));
                output.Score = Int32.Parse(line.Substring(line.LastIndexOf('-') + 2));
                scoreList.Add(output);
            }
            scoreList.Add((currentRole, Int32.Parse(currentScore)));
            //sort Tuple List
            sortScoreList(scoreList);

            return scoreList;
        }

        private void sortScoreList(List<(string Role, int Score)> list)
        {
            int length = list.Count;
            for (int i = 1; i < length; i++)
            {
                (string Role, int Score) key = list[i];
                int j = i - 1;

                while (j >= 0 && list[j].Score < key.Score)
                {
                    list[j + 1] = list[j];
                    j = j - 1;
                }
                list[j + 1] = key;
            }

        }

        //Write Scores to file
        public void WriteScores()
        {
            string[] output = new string[highscores.Count];
            for (int i = 0; i < highscores.Count; i++)
            {
                output[i] = highscores[i].Role+": - " + highscores[i].Score;
            }

            string filePath = (Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.ToString() + @"\Level1_HighScores.txt");

            File.WriteAllLines(filePath, output);
        }
    }
}
