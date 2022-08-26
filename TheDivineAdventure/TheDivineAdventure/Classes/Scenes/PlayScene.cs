using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

using System;
using System.Collections.Generic;

namespace TheDivineAdventure
{
    public class PlayScene : Scene
    {
        //2d Textures
        private Texture2D hudL1, hudL2, progIcon, healthBar, staminaBar, manaBar, clericIcon, clericProjectileTex, clericProjImpact,
            rogueIcon, warriorIcon, mageIcon;
        private Texture2D[] clericImpactAnim;
        private Skybox sky;
        private Color hudFade;

        //Projectile Sprites
        private WorldSprite clericProjectile;
        private List<WorldSprite> projectileImpacts;

        // 3D Assets
        public Model warriorModel, rogueModel, mageModel, clericModel;
        public Model demonModel;
        public Model level1Model;
        public Model playerProjModel, enemyProjModel;
        public Model playerMelModel, enemyMelModel;
        public Model portalModel;

        //Capture travel distance
        float travel;
        //Camera
        private Camera camera;
        public Vector3 playerPos;

        // Player
        private Player player;
        private List<SoundEffect> playerSounds = new List<SoundEffect>();
        public static int score;
        private bool isDead;

        // Enemy
        private List<Enemy> enemyList;
        private List<SoundEffect> enemySounds = new List<SoundEffect>();
        private string enemyRole;
        private float enemyTimer, enemyTimerMax;

        // Matrices
        private Matrix worldPlayer, worldEnemy, worldProj, worldLevel;

        public PlayScene(SpriteBatch sb, GraphicsDeviceManager graph, Game1 game, ContentManager cont) : base(sb, graph, game, cont)
        {

        }

        //initialize game objects and load level
        public override void Initialize()
        {
            base.Initialize();
            LoadContent();
            //hide cursor
            parent.showCursor = false;
            Mouse.SetPosition((int)(1920*parent.currentScreenScale.X/2), (int)(1080f * parent.currentScreenScale.Y / 2));

            //create fade color
            hudFade = Color.White;

            // Timer Info
            enemyTimerMax = 3f;
            enemyTimer = 1f;

            // Initialize game objects
            player = new Player(playerSounds, parent.playerRole, parent)
            {
                Rot = new Vector3(0, 356, 0)
            };
            camera = new Camera(parent.GraphicsDevice, Vector3.Up, player, this);
            enemyList = new List<Enemy>();
            projectileImpacts = new List<WorldSprite>();

            // Generate resource Bars rectangles
            parent.healthBarRec = new Rectangle(
                (int)Math.Round(_graphics.PreferredBackBufferWidth * 0.099f / parent.currentScreenScale.X),
                (int)Math.Round(_graphics.PreferredBackBufferHeight * 0.044f / parent.currentScreenScale.Y),
                (int)Math.Round(.201f * _graphics.PreferredBackBufferWidth / parent.currentScreenScale.X),
                (int)Math.Round(.05f * _graphics.PreferredBackBufferHeight / parent.currentScreenScale.Y));
            parent.secondBarRec = new Rectangle(
                (int)Math.Round(_graphics.PreferredBackBufferWidth * 0.088f / parent.currentScreenScale.X),
                (int)Math.Round(_graphics.PreferredBackBufferHeight * 0.099f / parent.currentScreenScale.Y),
                (int)Math.Round(.201f * _graphics.PreferredBackBufferWidth / parent.currentScreenScale.X),
                (int)Math.Round(.05f * _graphics.PreferredBackBufferHeight / parent.currentScreenScale.Y));

            // Initialize Distance to Boss(kept as a variable in case we have multiple level length)
            parent.levelLength = 3500;
            travel = 0;

            //set score to 0
            score = 0;

            //make player alive
            isDead = false;
        }

        public override void LoadContent()
        {
            base.LoadContent();

            //load 2d textures
            hudL1 = Content.Load<Texture2D>("TEX_HolyHUD_L1");
            hudL2 = Content.Load<Texture2D>("TEX_HolyHUD_L2");
            progIcon = Content.Load<Texture2D>("TEX_ProgressionIcon");
            healthBar = Content.Load<Texture2D>("TEX_HealthBar");
            manaBar = Content.Load<Texture2D>("TEX_ManaBar");
            staminaBar = Content.Load<Texture2D>("TEX_StaminaBar");
            //cleric items
            clericIcon = Content.Load<Texture2D>("TEX_Cleric_Icon");
            clericProjectileTex = Content.Load<Texture2D>("TEX_DivineProjectile01_Base");
            clericProjectile = new WorldSprite(clericProjectileTex, parent, Content);
            clericProjImpact = Content.Load<Texture2D>("TEX_ClericProjectileImpact_Sheet");
            clericImpactAnim = WorldSprite.GenerateAnim(clericProjImpact, 512, parent);
            //rogueItems
            rogueIcon = Content.Load<Texture2D>("TEX_Rogue_Icon");
            //WarriorItems
            warriorIcon = Content.Load<Texture2D>("TEX_Warrior_Icon");
            //rogueItems
            mageIcon = Content.Load<Texture2D>("TEX_Mage_Icon");


            //load skybox
            sky = new Skybox("TEX_SkyboxLevel1", Content);

            // Load 3D models
            // Heroes
            warriorModel = Content.Load<Model>("MODEL_Warrior");
            rogueModel = Content.Load<Model>("MODEL_Rogue");
            mageModel = Content.Load<Model>("MODEL_Mage");
            clericModel = Content.Load<Model>("MODEL_Cleric");

            // Enemies
            demonModel = Content.Load<Model>("MODEL_Demon");

            // Levels
            level1Model = Content.Load<Model>("MODEL_Level1");

            // Attacks
            playerProjModel = Content.Load<Model>("MODEL_PlayerProjectile");
            enemyProjModel = Content.Load<Model>("MODEL_EnemyProjectile");
            playerMelModel = Content.Load<Model>("MODEL_PlayerMelee");
            enemyMelModel = Content.Load<Model>("MODEL_EnemyMelee");
            portalModel = Content.Load<Model>("MODEL_Portal");

            // Load sounds
            playerSounds.Add(Content.Load<SoundEffect>("SOUND_swordSlash"));
            playerSounds.Add(Content.Load<SoundEffect>("SOUND_DivineSpell"));
            playerSounds.Add(Content.Load<SoundEffect>("SOUND_SwordSpecial"));
            playerSounds.Add(Content.Load<SoundEffect>("SOUND_Heal"));
            playerSounds.Add(Content.Load<SoundEffect>("SOUND_Teleport"));
            playerSounds.Add(Content.Load<SoundEffect>("SOUND_TradeOff"));
            playerSounds.Add(Content.Load<SoundEffect>("SOUND_HealingPotion"));


            enemySounds.Add(Content.Load<SoundEffect>("SOUND_FireSpell"));

        }

        //function to do updates when player is playing in level.
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            //check if player is alive
            player.health = 500;
            if (!isDead)
            {
                //pause game if window is tabbed out of
                if (parent.IsActive)
                {
                    player.Update(deltaTime, camera);
                    camera.Update(deltaTime, player);
                }
                //pause game on pressing esc
                if (Keyboard.GetState().IsKeyDown(Keys.Escape) && parent.lastKeyboard.IsKeyUp(Keys.Escape))
                {
                    parent.currentScene = "PAUSE";
                    parent.pauseScene.Initialize();
                    return;
                }
            }
            // Spawn enemies
            // Stops spawning enemies when player is about to reach the end
            enemyTimer -= deltaTime;
            if (enemyTimer < 0f && enemyList.Count < 5 && player.Pos.Z < 3000)
            {
                enemyTimer = enemyTimerMax;
                enemyRole = Enemy.ROLES[0];
                enemyList.Add(new Enemy(enemySounds, enemyRole, player.Pos));
                if (enemyTimerMax > 2f)
                    enemyTimerMax -= 0.05f;
            }

            // If the player is at the end, despawn the remaining enemies
            if(player.Pos.Z > 3500 && enemyList.Count >0)
            {
                enemyList.Clear();
            }

            foreach (Enemy e in enemyList)
            {
                // If the player runs past an enemy, despawn it and spawn another in its place.
                if (player.Pos.Z > e.Pos.Z + 80)
                {
                    enemyList.Remove(e);
                    enemyList.Add(new Enemy(enemySounds, enemyRole, player.Pos));
                    break;
                }
                    
                // Only update enemies before player reaches the end
                if (player.Pos.Z < parent.levelLength)
                    e.Update(deltaTime, player);
                foreach (Attack p in e.projList)
                {
                    p.Update(deltaTime, player);
                }
                if (e.TimeToDestroy)
                {
                    enemyList.Remove(e);
                    break;
                }
            }

            //update projectile impacts
            foreach (WorldSprite imp in projectileImpacts)
            {
                if (imp.finished)
                {
                    projectileImpacts.Remove(imp);
                    break;
                }
            }

            foreach (Attack p in player.projList)
            {
                p.Update(deltaTime, enemyList);
                //initialize projectile impacts
                if (p.TimeToDestroy==true)
                {
                    switch (player.role)
                    {
                        case "WARRIOR":
                        case "ROGUE":
                            break;
                        case "MAGE":
                            break;
                        case "CLERIC":
                            projectileImpacts.Add(new WorldSprite(clericImpactAnim, false, 0.2f, parent, Content));
                            //Set 2D sprite world matrix
                            Matrix secondaryProj = Matrix.CreateScale(0.2f) * Matrix.CreateRotationY(MathHelper.ToRadians(90)) *
                                Matrix.CreateRotationZ(MathHelper.ToRadians(rand.Next(1, 180))) * Matrix.CreateTranslation(p.Pos);
                            projectileImpacts[projectileImpacts.Count - 1].SetPos(secondaryProj);
                            break;
                    }
                }
            }

            //update distance to boss
            if (player.Pos.Z > 0 && player.Pos.Z < parent.levelLength)
                travel = (player.Pos.Z * 434 * parent.currentScreenScale.X) / Math.Abs(parent.levelLength);

            //fade hud out color generator
            if (isDead)
            {
                hudFade = new Color(Color.DarkSalmon, 1 - parent.lostScene.fadeIn);
                return;
            }
            // Basic kill player
            if (player.Health <= 0 )
            {
                isDead = true;
                parent.currentScene = "IS_DEAD";
                parent.lostScene.Initialize();
            }

            //Finish Level
            if(player.Pos.Z >= parent.levelLength)
            {
                parent.currentScene = "LEVEL_END";
                parent.levelEnd1.currentScore = score.ToString();
                parent.levelEnd1.Initialize();
                return;
            }
        }

        //function to do draw when player is playing in level.
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            //draw Skybox
            sky.Draw(camera.View, camera.Proj, player.Pos, gameTime);
            parent.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            parent.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            // Render world
            worldLevel = Matrix.CreateScale(1f) *
                        Matrix.CreateRotationY(MathHelper.ToRadians(180f)) *
                        Matrix.CreateTranslation(new Vector3(0, 0, -5));

            level1Model.Draw(worldLevel, camera.View, camera.Proj);

            // Render player
            worldPlayer = player.world;
            switch (player.role)
            {
                case "WARRIOR":
                    warriorModel.Draw(worldPlayer, camera.View, camera.Proj);
                    break;
                case "ROGUE":
                    rogueModel.Draw(worldPlayer, camera.View, camera.Proj);
                    break;
                case "MAGE":
                    mageModel.Draw(worldPlayer, camera.View, camera.Proj);
                    break;
                case "CLERIC":
                    clericModel.Draw(worldPlayer, camera.View, camera.Proj);
                    break;
            }


            // Render player bullets
            foreach (Attack p in player.projList)
            {
                worldProj = Matrix.CreateScale(1f) *
                        Matrix.CreateRotationZ(MathHelper.ToRadians(90)) *
                        Matrix.CreateTranslation(p.Pos + new Vector3(0, 5, 0));

                //Set 2D sprite world matrix
                Matrix secondaryProj = Matrix.CreateScale(0.029f) * Matrix.CreateRotationY(MathHelper.ToRadians(90)) *
                    Matrix.CreateRotationZ(MathHelper.ToRadians(1)) * Matrix.CreateTranslation(p.Pos);

                if (p.IsMelee)
                {
                    playerMelModel.Draw(worldProj, camera.View, camera.Proj);
                }
                else
                {

                    switch (player.role)
                    {
                        case "MAGE":
                            //draw 3d model for projectile
                            playerProjModel.Draw(worldProj, camera.View, camera.Proj);
                            break;
                        case "CLERIC":
                            //draw a 2d sprite for the projectile
                            clericProjectile.Draw(secondaryProj, camera.View, camera.Proj);
                            break;
                        default:
                            break;
                    }

                }
            }
            //render projectile impacts
            foreach (WorldSprite imp in projectileImpacts)
            {

                switch (player.role)
                {
                    case "WARRIOR":
                        break;
                    case "ROGUE":
                        break;
                    case "MAGE":
                        break;
                    case "CLERIC":
                        imp.Draw(camera.View, camera.Proj);
                        break;
                }
            }

            // Render enemies
            foreach (Enemy e in enemyList)
            {
                worldEnemy = Matrix.CreateScale(1f) *
                        Matrix.CreateRotationY(e.Rot) *
                        Matrix.CreateTranslation(e.Pos);
                switch (e.Role)
                {
                    case "DEMON":
                        demonModel.Draw(worldEnemy, camera.View, camera.Proj);
                        break;
                    case "HELLHOUND":
                        //houndModel.Draw(worldEnemy, camera.View, camera.Proj);
                        break;
                    case "GOBLIN":
                        //goblinModel.Draw(worldEnemy, camera.View, camera.Proj);
                        break;
                    case "SKELETON":
                        //skeleModel.Draw(worldEnemy, camera.View, camera.Proj);
                        clericModel.Draw(worldEnemy, camera.View, camera.Proj);
                        break;
                }

                // Render enemy bullets
                foreach (Attack p in e.projList)
                {
                    worldProj = Matrix.CreateScale(1f) *
                        Matrix.CreateTranslation(p.Pos);
                    if (p.IsMelee)
                    {
                        enemyMelModel.Draw(worldProj, camera.View, camera.Proj);
                    }
                    else
                    {
                        enemyProjModel.Draw(worldProj, camera.View, camera.Proj);
                    }
                }
            }

            if (parent.lostScene.fadeIn > 1)
            {
                return;
            }
            // ** Render HUD **
            parent.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            parent.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            _spriteBatch.Begin();
            _spriteBatch.Draw(hudL1, Vector2.Zero, null, hudFade, 0, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 0);
            //progessIcon
            if (player.Pos.Z < parent.levelLength)
            {
                _spriteBatch.Draw(progIcon,
                    new Vector2(714 * parent.currentScreenScale.X + travel, 958 * parent.currentScreenScale.Y),
                    null, hudFade, 0, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 0);
            }
            //Score
            _spriteBatch.DrawString(parent.BigFont, score.ToString(),
                new Vector2(_graphics.PreferredBackBufferWidth * 0.498f - (parent.BigFont.MeasureString(score.ToString()) * .5f * parent.currentScreenScale).X, _graphics.PreferredBackBufferHeight * -0.01f),
                parent.textGold, 0f, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 1);
            //Resource bars
            _spriteBatch.Draw(healthBar,
                player.resourceBarUpdate(true, parent.healthBarRec,
                new Vector2(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight),
                parent.currentScreenScale), hudFade);
            if (player.IsCaster)
                _spriteBatch.Draw(manaBar,
                    player.resourceBarUpdate(false, parent.secondBarRec,
                    new Vector2(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight),
                    parent.currentScreenScale), hudFade);
            else
                _spriteBatch.Draw(staminaBar,
                    player.resourceBarUpdate(false, parent.secondBarRec,
                    new Vector2(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight)
                    , parent.currentScreenScale), hudFade);
            //topHUD layer
            _spriteBatch.Draw(hudL2, Vector2.Zero, null, hudFade, 0, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 1);
            //draw player Icon
            switch (player.role)
            {
                case "WARRIOR":
                    _spriteBatch.Draw(warriorIcon, new Vector2(50, 21) * parent.currentScreenScale, null,
                       hudFade, 0, Vector2.Zero, 0.07f * parent.currentScreenScale, SpriteEffects.None, 1);
                    break;
                case "ROGUE":
                    _spriteBatch.Draw(rogueIcon, new Vector2(50, 19) * parent.currentScreenScale, null,
                       hudFade, 0, Vector2.Zero, 0.108f * parent.currentScreenScale, SpriteEffects.None, 1);
                    break;
                case "MAGE":
                    _spriteBatch.Draw(mageIcon, new Vector2(50, 21) * parent.currentScreenScale, null,
                       hudFade, 0, Vector2.Zero, 0.07f * parent.currentScreenScale, SpriteEffects.None, 1);
                    break;
                default:
                    _spriteBatch.Draw(clericIcon, new Vector2(49, 19) * parent.currentScreenScale, null,
                        hudFade, 0, Vector2.Zero, 0.071f * parent.currentScreenScale, SpriteEffects.None, 1);
                    break;
            }
            FadeIn(0.01f);
            _spriteBatch.End();
            parent.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            parent.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
        }

        ////////////////////
        ///GETTER/SETTERS///
        ////////////////////
        public Matrix WorldPlayer
        {
            get { return worldPlayer; }
        }
    }
}
