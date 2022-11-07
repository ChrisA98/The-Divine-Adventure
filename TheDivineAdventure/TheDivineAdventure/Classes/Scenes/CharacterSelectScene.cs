using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

using System.Collections.Generic;
using TheDivineAdventure.SkinModels;


namespace TheDivineAdventure
{
    public class CharacterSelectScene : Scene
    {
        public static readonly string[] ROLES = { "WARRIOR", "ROGUE", "MAGE", "CLERIC" };
        const int WARRIOR = 0, ROGUE = 1, MAGE = 2, CLERIC = 3;
        
        //Display
        GraphicsDevice gpu;

        private SpriteFont font, bigFont;
        private Texture2D backdrop, frontplate, arrowButton1, arrowButton2, actionButton, emberSheet01;
        private AnimatedSprite[] titleEmbers;
        private Button leftArrow, rightArrow, confirm, back;
        private Model stone;    //Standing stone
        private SkinModel[] playerModels;
        private Matrix characterWorld;
        private Player characters;
        private SkinModelLoader skinModel_loader;           //Loads characters
        private SkinFx skinFx;                              //Player for SkinEffect
        private List<SoundEffect> playerSounds = new List<SoundEffect>();
        private string currentChar;
        private Matrix worldStone, worldPlayer, proj;

        // Mouse
        private MouseState currMouseState;
        private MouseState previousMouseState;
        private float rot;      //rotate model

        private Camera camera;  //scene camera


        enum ClericBones
        {
            Head = 0,
            HipsTrans,
            HipsRot,
            HipsScale,
            LeftArmTrans,
            LeftArmRot,
            LeftArmScale,
            LeftFootTrans,
            LeftFootRot,
            LeftFootScale,
            LeftForearm,
            LeftHand,
            LeftLeg,
            LeftShoulderTrans,
            LeftShoulderRot,
            LeftShoulderScale,
            LeftToeBase,
            LeftUpLegTrans,
            LeftUpLegRot,
            LeftUpLegScale,
            NeckTrans,
            NeckRot,
            NeckScale,
            RightArmTrans,
            RightArmRot,
            RightArmScale,
            RightFootTrans,
            RightFootRot,
            RightFootScale,
            RightForeArm,
            RightHand,
            RightLeg,
            RightShoulderTrans,
            RightShoulderRot,
            RightShoulderScale,
            RightToeBaseTrans,
            RightToeBaseRot,
            RightToeBaseScale,
            RightUpLegTrans,
            RightUpLegRot,
            RightUpLegScale,
            SpineTrans,
            SpineRot,
            SpineScale,
            Spine1,
            Spine2Trans,
            Spine2Rot,
            Spine2Scale
        }
        enum MageBones
        {
            Head = 0,
            HipsTrans,
            HipsRot,
            HipsScale,
            LeftArmTrans,
            LeftArmRot,
            LeftArmScale,
            LeftFootTrans,
            LeftFootRot,
            LeftFootScale,
            LeftForearmTrans,
            LeftForearmRot,
            LeftForearmScale,
            LeftHand,
            LeftLegTrans,
            LeftLegRot,
            LeftLegScale,
            LeftShoulderTrans,
            LeftShoulderRot,
            LeftShoulderScale,
            LeftToeBase,
            LeftUpLegTrans,
            LeftUpLegRot,
            LeftUpLegScale,
            NeckTrans,
            NeckRot,
            NeckScale,
            RightArmTrans,
            RightArmRot,
            RightArmScale,
            RightFootTrans,
            RightFootRot,
            RightFootScale,
            RightForeArmTrans,
            RightForeArmRot,
            RightForeArmScale,
            RightHand,
            RightLegTrans,
            RightLegRot,
            RightLegScale,
            RightShoulderTrans,
            RightShoulderRot,
            RightShoulderScale,
            RightToeBaseTrans,
            RightToeBaseRot,
            RightToeBaseScale,
            RightUpLegTrans,
            RightUpLegRot,
            RightUpLegScale,
            SpineTrans,
            SpineRot,
            SpineScale,
            Spine1Trans,
            Spine1Rot,
            Spine1Scale,
            Spine2Trans,
            Spine2Rot,
            Spine2Scale
        }
        enum WarriorBones
        {
            Head = 0,
            HipsTrans,
            HipsRot,
            HipsScale,
            LeftArmTrans,
            LeftArmRot,
            LeftArmScale,
            LeftFootTrans,
            LeftFootRot,
            LeftFootScale,
            LeftForeArm,
            LeftHand,
            LeftLeg,
            LeftShoulderTrans,
            LeftShoulderRot,
            LeftShoulderScale,
            LeftToeBase,
            LeftUpLegTrans,
            LeftUpLegRot,
            LeftUpLegScale,
            NeckTrans,
            NeckRot,
            NeckScale,
            RightArmTrans,
            RightArmRot,
            RightArmScale,
            RightFootTrans,
            RightFootRot,
            RightFootScale,
            RightForeArm,
            RightHand,
            RightLeg,
            RightShoulderTrans,
            RightShoulderRot,
            RightShoulderScale,
            RightToeBaseTrans,
            RightToeBaseRot,
            RightToeBaseScale,
            RightUpLegTrans,
            RightUpLegRot,
            RightUpLegScale,
            SpineTrans,
            SpineRot,
            SpineScale,
            Spine1,
            Spine2Trans,
            Spine2Rot,
            Spine2Scale
        }
        enum RogueBones
        {
            Head,
            HipsTrans,
            HipsRot,
            HipsScale,
            LeftArmTrans,
            LeftArmRot,
            LeftArmScale,
            LeftFootTrans,
            LeftFootRot,
            LeftFootScale,
            LeftForeArm,
            LeftHand,
            LeftLeg,
            LeftShoulderTrans,
            LeftShoulderRot,
            LeftShoulderScale,
            LeftToeBase,
            LeftUpLegTrans,
            LeftUpLegRot,
            LeftUpLegScale,
            NeckTrans,
            NeckRot,
            NeckScale,
            RightArmTrans,
            RightArmRot,
            RightArmScale,
            RightFootTrans,
            RightFootRot,
            RightFootScale,
            RightForeArm,
            RightHand,
            RightLeg,
            RightShoulderTrans,
            RightShoulderRot,
            RightShoulderScale,
            RightToeBaseTrans,
            RightToeBaseRot,
            RightToeBaseScale,
            RightUpLegTrans,
            RightUpLegRot,
            RightUpLegScale,
            SpineTrans,
            SpineRot,
            SpineScale,
            Spine1,
            Spine2Trans,
            Spine2Rot,
            Spine2Scale
        }

        int[] socketIDs;        //ids for sockets
        Matrix[] socketTrans;
        SkinModel[] socketedItems;              //items to be kept in sockets by player
        SkinFx[] itemFx;                        //Items for SkinEffect

        bool init;  //starting scene to start anims

        public CharacterSelectScene(SpriteBatch sb, GraphicsDeviceManager graph, Game1 game, ContentManager cont) : base(sb, graph, game, cont)
        {
            //Display
            gpu = game.GraphicsDevice;
        }

        public override void Initialize()
        {        
            // Declare first character view
            currentChar = "CLERIC";


            rot = -180;    //rotating model starts at -180;

            //Camera
            camera = new Camera(parent.GraphicsDevice, Vector3.Up);

            // Initialize game objects
            characters = new Player(playerSounds, currentChar, parent);

            playerModels = new SkinModel[4];
            characterWorld = Matrix.CreateScale(1f) *
                        Matrix.CreateRotationX(MathHelper.ToRadians(90)) *
                        Matrix.CreateRotationY(MathHelper.ToRadians(0)) *
                        Matrix.CreateRotationX(MathHelper.ToRadians(0)) *
                        Matrix.CreateTranslation(new Vector3(0,0,0));
            socketIDs = new int[4];
            socketTrans = new Matrix[4];
            itemFx = new SkinFx[4];
            socketedItems = new SkinModel[4];

            // Set camera data
            proj = Matrix.CreatePerspectiveFieldOfView(
                                MathHelper.ToRadians(60),
                                parent.GraphicsDevice.Viewport.AspectRatio,
                                0.05f,
                                1000);

            init = true;

            base.Initialize();
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

            //create embers
            titleEmbers = new AnimatedSprite[30];
            for (int i = 0; i < titleEmbers.Length; i++)
            {
                titleEmbers[i] = new AnimatedSprite(174, 346, emberSheet01, 6);
                titleEmbers[i].Pos = new Vector2(rand.Next(1920) * parent.currentScreenScale.X, rand.Next(450, 750) * parent.currentScreenScale.Y);
                titleEmbers[i].Scale = 1 - (rand.Next(-200, 50) / 100f);
                titleEmbers[i].Frame = rand.Next(6);
            }

            //create buttons
            leftArrow = new Button(arrowButton1, arrowButton1, null, font, new Vector2(254, 910),
                new Vector2(179, 155), parent.currentScreenScale);
            rightArrow = new Button(arrowButton2, arrowButton2, null, font, new Vector2(719, 911),
                new Vector2(179, 155), parent.currentScreenScale);
            confirm = new Button(actionButton, actionButton, "Confirm", font, new Vector2(1633, 807),
                new Vector2(210, 76), parent.currentScreenScale);
            back = new Button(actionButton, actionButton, "Return", font, new Vector2(48, 48),
                new Vector2(210, 76), parent.currentScreenScale);


            // Load 3D models
            stone = Content.Load<Model>("MODEL_Selection_Stone");



            // SKIN MODEL LOADER //
            skinFx = new SkinFx(Content, "SkinEffect");
            skinFx.DisableFog();
            itemFx[0] = new SkinFx(Content, "SkinEffect");
            itemFx[1] = new SkinFx(Content, "SkinEffect");
            itemFx[2] = new SkinFx(Content, "SkinEffect");
            itemFx[3] = new SkinFx(Content, "SkinEffect");
            skinModel_loader = new SkinModelLoader(Content, gpu);
            skinModel_loader.SetDefaultOptions(0.1f, "default_tex");

            playerModels[CLERIC] = skinModel_loader.Load("MOD_Cleric/ANIM_Cleric_Idle.fbx", "MOD_Cleric", true, 3, skinFx, rescale: 2.2f);
            playerModels[MAGE] = skinModel_loader.Load("MOD_Mage/ANIM_Mage_Idle.fbx", "MOD_Mage", true, 3, skinFx, rescale: 2.5f);
            playerModels[WARRIOR] = skinModel_loader.Load("MOD_Warrior/ANIM_Warrior_Idle.fbx", "MOD_Warrior", true, 3, skinFx, rescale: 2.5f);
            playerModels[ROGUE] = skinModel_loader.Load("MOD_Rogue/ANIM_Rogue_Idle.fbx", "MOD_Rogue", true, 3, skinFx, rescale: 2.5f);

            //Load Staff
            socketedItems[0] = skinModel_loader.Load("MOD_Mage/MOD_MageStaff.fbx", "MOD_Mage", true, 3, itemFx[0], rescale: 3);
            socketTrans[0] = Matrix.CreateScale(1) *
                Matrix.CreateRotationX(MathHelper.ToRadians(60)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(-40)) *
                Matrix.CreateRotationZ(MathHelper.ToRadians(220)) *
                Matrix.CreateTranslation(new Vector3(-0.4f, -0.1f, 0));

            //Load Daggers
            socketedItems[1] = skinModel_loader.Load("MOD_Rogue/MOD_Dagger.fbx", "MOD_Rogue", true, 3, itemFx[1], rescale: 2.2f);
            socketTrans[1] = Matrix.CreateScale(0.75f) *
                Matrix.CreateRotationX(MathHelper.ToRadians(70)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(-20)) *
                Matrix.CreateTranslation(new Vector3(-.3f, 0.04f, 0));
            socketedItems[2] = skinModel_loader.Load("MOD_Rogue/MOD_Dagger.fbx", "MOD_Rogue", true, 3, itemFx[2], rescale: 2.2f);
            socketTrans[2] = Matrix.CreateScale(0.75f) *
                Matrix.CreateRotationX(MathHelper.ToRadians(-20)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(1200)) *
                Matrix.CreateRotationZ(MathHelper.ToRadians(60)) *
                Matrix.CreateTranslation(new Vector3(.5f, 0f, 0));

            //Load Axe
            socketedItems[3] = skinModel_loader.Load("MOD_Warrior/MOD_Axe.fbx", "MOD_Warrior", true, 3, itemFx[3], rescale: 2.2f);
            socketTrans[3] = Matrix.CreateScale(1f) *
                Matrix.CreateRotationX(MathHelper.ToRadians(180)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(84)) *
                Matrix.CreateRotationZ(MathHelper.ToRadians(-32)) *
                Matrix.CreateTranslation(new Vector3(-1, -1, -0.17f));

        }

        public override void Update(GameTime gameTime)
        {
            if (init) // INITIALIZE STARTING ANIMS
            {
                foreach (SkinModel sk in playerModels)
                {
                    if (sk != null) sk.BeginAnimation(0, gameTime); //begin player anims
                }
                foreach (SkinModel sk in socketedItems)
                {
                    if (sk != null) sk.BeginAnimation(0, gameTime); //start item anims (currently are none, but may add them later)
                }
                init = false;
            }
            foreach (SkinModel an in playerModels)
            {
                if (an != null) an.Update(gameTime);    //update model animations
            }

            //rotate model
            // Handle mouse movement
            float deltaX;
            float deltaY;
            currMouseState = Mouse.GetState();
            if (parent.mouseState.LeftButton == ButtonState.Pressed)
            {
                // Only handle mouse movement code if the mouse has moved (the state has changed)
                if (currMouseState != previousMouseState)
                {
                    // Cache mouse location
                    deltaX = (currMouseState.X - previousMouseState.X) * (parent.currentScreenScale.X);

                    // Applies the rotation (we are only rotating on the X and Y axes)
                    rot += deltaX;

                    //Clamp roatations
                    if (rot > 360) rot -= 360;
                    if (rot < -360) rot += 360;
                }
            }

            //Camera Updates
            camera.Update(Matrix.Identity);

            //click detection for model rotating
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

            previousMouseState = Mouse.GetState();
            base.Update(gameTime);
        }

        #region SET 3D STATES -----------
        RasterizerState rs_ccw = new RasterizerState() { FillMode = FillMode.Solid, CullMode = CullMode.CullCounterClockwiseFace };
        void Set3dStates()
        {
            gpu.BlendState = BlendState.NonPremultiplied;
            gpu.DepthStencilState = DepthStencilState.Default;
            if (gpu.RasterizerState.CullMode == CullMode.None)
            {
                gpu.RasterizerState = rs_ccw;
            }
        }
        #endregion
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Set3dStates();

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
            stone.Draw(worldStone, Matrix.CreateLookAt(new Vector3(4, 20, 50), new Vector3(15, 13, 0), Vector3.Up), camera.proj);

            // Render character
            worldPlayer = Matrix.CreateScale(1f) *
                        Matrix.CreateRotationY(MathHelper.ToRadians(characters.Rot.Y)) *
                        Matrix.CreateTranslation(characters.Pos);

            SkinModel hero;

            
            switch (currentChar)
            {
                case "WARRIOR":
                    hero = playerModels[WARRIOR];
                    characterWorld = Matrix.CreateScale(1f) *
                                Matrix.CreateRotationY(MathHelper.ToRadians(rot)) *
                                Matrix.CreateTranslation(new Vector3(10, -6, 13));
                    for (int i = 0; i < socketedItems[3].meshes.Length; i++)
                    {
                        itemFx[3].SetDiffuseCol(Color.White.ToVector4());
                        itemFx[3].SetSpecularCol(new Vector3(0.2f, 01f, 0.05f));
                        itemFx[3].SetSpecularPow(256f);
                        itemFx[3].world = socketTrans[3] *
                            hero.GetBoneTransform((int)WarriorBones.RightHand)
                            * characterWorld;
                        socketedItems[3].DrawMesh(i, camera, itemFx[3].world, false);
                    }
                    break;
                case "ROGUE":
                    hero = playerModels[ROGUE];
                    characterWorld = Matrix.CreateScale(1f) *
                                Matrix.CreateRotationY(MathHelper.ToRadians(rot)) *
                                Matrix.CreateTranslation(new Vector3(10, 6, 13));
                    for (int i = 0; i < socketedItems[3].meshes.Length; i++)
                    {
                        itemFx[1].SetDiffuseCol(Color.White.ToVector4());
                        itemFx[1].SetSpecularCol(new Vector3(0.2f, 01f, 0.05f));
                        itemFx[1].SetSpecularPow(256f);
                        itemFx[1].world = socketTrans[1] *
                            hero.GetBoneTransform((int)RogueBones.RightHand)
                            * characterWorld;
                        socketedItems[1].DrawMesh(i, camera, itemFx[1].world, false);
                    }
                    for (int i = 0; i < socketedItems[3].meshes.Length; i++)
                    {
                        itemFx[2].SetDiffuseCol(Color.White.ToVector4());
                        itemFx[2].SetSpecularCol(new Vector3(0.2f, 01f, 0.05f));
                        itemFx[2].SetSpecularPow(256f);
                        itemFx[2].world = socketTrans[2] *
                            hero.GetBoneTransform((int)RogueBones.LeftHand)
                            * characterWorld;
                        socketedItems[1].DrawMesh(i, camera, itemFx[2].world, false);
                    }
                    break;
                case "MAGE":
                    hero = playerModels[MAGE];
                    characterWorld = Matrix.CreateScale(1f) *
                                Matrix.CreateRotationY(MathHelper.ToRadians(rot)) *
                                Matrix.CreateTranslation(new Vector3(10, 6, 13));
                    for (int i = 0; i < socketedItems[0].meshes.Length; i++)
                    {
                        itemFx[0].SetDiffuseCol(Color.White.ToVector4());
                        itemFx[0].SetSpecularCol(new Vector3(0.2f, 01f, 0.05f));
                        itemFx[0].SetSpecularPow(256f);
                        itemFx[0].world = socketTrans[0] *
                            hero.GetBoneTransform((int)MageBones.RightHand)
                            * characterWorld;
                        socketedItems[0].DrawMesh(i, camera, itemFx[0].world, false);
                    }
                    break;
                default:
                    hero = playerModels[CLERIC];
                    characterWorld = Matrix.CreateScale(1f) *
                                Matrix.CreateRotationY(MathHelper.ToRadians(rot)) *
                                Matrix.CreateTranslation(new Vector3(10, 6, 13));
                    break;
            }


            for (int i = 0; i < hero.meshes.Length; i++)
            {
                skinFx.SetDiffuseCol(Color.White.ToVector4());
                skinFx.SetSpecularCol(new Vector3(1f, 0.1f, 0.05f));
                skinFx.SetSpecularPow(255f);
                hero.DrawMesh(i, camera, characterWorld, false);
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
            _spriteBatch.DrawString(font, "Speed: " + characters.walkMax, new Vector2(1064, 333) * parent.currentScreenScale,
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
