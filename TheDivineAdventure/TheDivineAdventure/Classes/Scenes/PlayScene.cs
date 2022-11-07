using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using TheDivineAdventure.SkinModels;

namespace TheDivineAdventure
{
    public class PlayScene : Scene
    {
        //Display
        public GraphicsDevice gpu;
        //2d Textures
        private Texture2D hudL1, hudL2, progIcon, healthBar, staminaBar, manaBar, clericIcon, clericProjectileTex, clericProjImpact,
            rogueIcon, warriorIcon, mageIcon;
        private Texture2D[] clericImpactAnim;
        private Skybox sky;
        private Color hudFade;

        //Projectile Sprites
        private WorldSprite clericProjectile;
        private List<WorldSprite> projectileImpacts;

        //Enemy Healthbars
        private List<WorldSprite> healthBars;

        // 3D Assets
        public Model warriorModel, rogueModel, mageModel;
        public Model demonModel;
        public Model levelModel;
        public Model playerProjModel, enemyProjModel;
        public Model playerMelModel, enemyMelModel;
        public Model portalModel;
        public Shapes debugHitbox;

        //Level Data
        LevelLoader levelLoader;
        public Level thisLevel;
        private int levelID;

        // --Animated Models--
        SkinModelLoader skinModel_loader;           //Loads characters
        SkinFx skinFx;                     //Player for SkinEffect
        SkinFx[] itemFx;                     //Items for SkinEffect
        SkinModel[] playerModel;                //main character model
        SkinModel[] socketedItems;              //items to be kept in sockets by player
        SkinModel terrainPiece;
        const int BASE = 0, WALK = 1, RUN = 2, JUMP = 3, WALKBACK = 4, ATTACK1 = 5, ATTACK2 = 6, WALKLEFT = 7, WALKRIGHT = 8, ATTACK1_2 = 9; // (could use enum but easier to index without casting)
        float[] playerAnimWeights;                  //Player animation weights

        //Enemy models
        SkinModel[] enemyModels;                   //enemy base model
        const int IMP = 0, HELLHOUND = 1, MINOTAUR = 2, REVENANT = 3;   //enemy model id's

        //reference for bone ints
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
        const int LHAND = 0, RHAND = 1, HEAD = 2;
        int[] socketIDs;        //ids for sockets
        Matrix[] socketTrans;

        //Capture travel distance
        float travel;

        bool init; // indicate loading into level


        //Camera
        private Camera camera;
        public Vector3 playerPos;
        public bool debugMode;

        // Player
        public Player player;
        private List<SoundEffect> playerSounds = new List<SoundEffect>();
        public static int score;
        private bool isDead;

        // Enemy
        public List<Enemy> worldEnemyList;
        public List<SoundEffect> enemySounds = new List<SoundEffect>();
        public EnemySpawner[] levelSpawnerList;

        // Matrices
        private Matrix worldProj;


        //<<CONSTRUCTOR>>
        public PlayScene(SpriteBatch sb, GraphicsDeviceManager graph, Game1 game, ContentManager cont, int LevelID) : base(sb, graph, game, cont)
        {
            //Display
            gpu = game.GraphicsDevice;
            levelID = LevelID;
        }

        //initialize game objects and load level
        public override void Initialize()
        {
            //hide cursor
            parent.showCursor = false;
            Mouse.SetPosition((int)(1920 * parent.currentScreenScale.X / 2), (int)(1080f * parent.currentScreenScale.Y / 2));

            //create fade color
            hudFade = Color.White;

            // Initialize game objects //
            player = new Player(playerSounds, parent.playerRole, parent)
            {
                Rot = new Vector3(0, 356, 0)
            };
            camera = new Camera(parent.GraphicsDevice, Vector3.Up, player);

            //Level Loading
            switch (levelID)
            {
                case 1:
                    levelLoader = new LevelLoader(@"Content\Level_Data\Level_1", Content, this);
                    break;
                default:
                    levelLoader = new LevelLoader(@"Content\Level_Data\Level_1", Content, this);
                    break;
            }

            //player models
            if (player.role == "ROGUE") playerModel = new SkinModel[10];    //adds extra anim slot for rogues second attack anim
            else playerModel = new SkinModel[9];

            //item models
            socketIDs = new int[3];
            socketTrans = new Matrix[3];
            itemFx = new SkinFx[3];
            socketedItems = new SkinModel[3];

            //enemy models
            worldEnemyList = new List<Enemy>();
            enemyModels = new SkinModel[4];
            healthBars = new List<WorldSprite>(); ;


            //Projectiles
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

            init = true; // indicate loading into level

            //Create Debug Hitbox Controller
            debugHitbox = new Shapes(gpu, Color.RoyalBlue, Vector3.Zero, Vector3.Zero, GetType());
            debugHitbox.DefineCuboid(Vector3.Zero);

            base.Initialize();
        }

        #region <<Load Content>>
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

            #region LOAD PLAYER 3D MODELS AND ANIMATIONS-------------

            // SKIN MODEL LOADER //
            skinFx = new SkinFx(Content, camera, "SkinEffect");
            itemFx[LHAND] = new SkinFx(Content, camera, "SkinEffect");
            itemFx[RHAND] = new SkinFx(Content, camera, "SkinEffect");
            itemFx[HEAD] = new SkinFx(Content, camera, "SkinEffect");
            skinModel_loader = new SkinModelLoader(Content, gpu);
            skinModel_loader.SetDefaultOptions(0.1f, "default_tex");

            // LOAD HERO ANIMATIONS //
            float scale = 0f;
            switch (player.role)
            {
                case "WARRIOR":
                    //load Warrior anims              
                    scale = 2.5f;
                    playerAnimWeights = new float[48];
                    playerModel[BASE] = skinModel_loader.Load("MOD_Warrior/ANIM_Warrior_Idle.fbx", "MOD_Warrior", true, 3, skinFx, rescale: scale);

                    playerModel[WALK] = skinModel_loader.Load("MOD_Warrior/ANIM_Warrior_Walk.fbx", "MOD_Warrior", true, 3, skinFx, rescale: scale);

                    playerModel[RUN] = skinModel_loader.Load("MOD_Warrior/ANIM_Warrior_Run.fbx", "MOD_Warrior", true, 3, skinFx, rescale: scale);

                    playerModel[JUMP] = skinModel_loader.Load("MOD_Warrior/ANIM_Warrior_Jump.fbx", "MOD_Warrior", true, 3, skinFx, rescale: scale);

                    playerModel[JUMP].loopAnimation = false;
                    playerModel[WALKBACK] = skinModel_loader.Load("MOD_Warrior/ANIM_Warrior_Walk_Rev.fbx", "MOD_Warrior", true, 3, skinFx, rescale: scale);

                    playerModel[ATTACK1] = skinModel_loader.Load("MOD_Warrior/ANIM_Warrior_AttackMain.fbx", "MOD_Warrior", true, 3, skinFx, rescale: scale);
                    playerModel[ATTACK1].loopAnimation = false;

                    playerModel[ATTACK2] = skinModel_loader.Load("MOD_Warrior/ANIM_Warrior_AttackSecondary.fbx", "MOD_Warrior", true, 3, skinFx, rescale: scale);
                    playerModel[ATTACK2].loopAnimation = false;

                    playerModel[WALKLEFT] = skinModel_loader.Load("MOD_Warrior/ANIM_Warrior_Walk_Left.fbx", "MOD_Warrior", true, 3, skinFx, rescale: scale);

                    playerModel[WALKRIGHT] = skinModel_loader.Load("MOD_Warrior/ANIM_Warrior_Walk_Right.fbx", "MOD_Warrior", true, 3, skinFx, rescale: scale);

                    //set mage sockets 
                    socketIDs[LHAND] = (int)WarriorBones.LeftHand;
                    socketIDs[RHAND] = (int)WarriorBones.RightHand;
                    socketIDs[HEAD] = (int)WarriorBones.Head;

                    //Load held items
                    socketedItems[RHAND] = skinModel_loader.Load("MOD_Warrior/MOD_Axe.fbx", "MOD_Warrior", true, 3, itemFx[RHAND], rescale: scale);
                    socketTrans[RHAND] = Matrix.CreateScale(0.5f) *
                        Matrix.CreateRotationX(MathHelper.ToRadians(180)) *
                        Matrix.CreateRotationY(MathHelper.ToRadians(84)) *
                        Matrix.CreateRotationZ(MathHelper.ToRadians(-32)) *
                        Matrix.CreateTranslation(new Vector3(-1, -1, -0.17f));
                    break;
                case "ROGUE":
                    //load Rogue anims              
                    scale = 2.5f;
                    playerAnimWeights = new float[48];
                    playerModel[BASE] = skinModel_loader.Load("MOD_Rogue/ANIM_Rogue_Idle.fbx", "MOD_Rogue", true, 3, skinFx, rescale: scale);

                    playerModel[WALK] = skinModel_loader.Load("MOD_Rogue/ANIM_Rogue_Walk.fbx", "MOD_Rogue", true, 3, skinFx, rescale: scale);

                    playerModel[RUN] = skinModel_loader.Load("MOD_Rogue/ANIM_Rogue_Run.fbx", "MOD_Rogue", true, 3, skinFx, rescale: scale);

                    playerModel[JUMP] = skinModel_loader.Load("MOD_Rogue/ANIM_Rogue_Jump.fbx", "MOD_Rogue", true, 3, skinFx, rescale: scale);

                    playerModel[JUMP].loopAnimation = false;
                    playerModel[WALKBACK] = skinModel_loader.Load("MOD_Rogue/ANIM_Rogue_Walk_Rev.fbx", "MOD_Rogue", true, 3, skinFx, rescale: scale);

                    playerModel[ATTACK1] = skinModel_loader.Load("MOD_Rogue/ANIM_Rogue_AttackMain_1.fbx", "MOD_Rogue", true, 3, skinFx, rescale: scale);
                    playerModel[ATTACK1].loopAnimation = false;

                    playerModel[ATTACK1_2] = skinModel_loader.Load("MOD_Rogue/ANIM_Rogue_AttackMain_2.fbx", "MOD_Rogue", true, 3, skinFx, rescale: scale);
                    playerModel[ATTACK1_2].loopAnimation = false;

                    playerModel[ATTACK2] = skinModel_loader.Load("MOD_Rogue/ANIM_Rogue_AttackSecondary.fbx", "MOD_Rogue", true, 3, skinFx, rescale: scale);
                    playerModel[ATTACK2].loopAnimation = false;

                    playerModel[WALKLEFT] = skinModel_loader.Load("MOD_Rogue/ANIM_Rogue_Walk_Left.fbx", "MOD_Rogue", true, 3, skinFx, rescale: scale);

                    playerModel[WALKRIGHT] = skinModel_loader.Load("MOD_Rogue/ANIM_Rogue_Walk_Right.fbx", "MOD_Rogue", true, 3, skinFx, rescale: scale);

                    //set mage sockets 
                    socketIDs[LHAND] = (int)RogueBones.LeftHand;
                    socketIDs[RHAND] = (int)RogueBones.RightHand;
                    socketIDs[HEAD] = (int)RogueBones.Head;

                    //Load held items
                    socketedItems[RHAND] = skinModel_loader.Load("MOD_Rogue/MOD_Dagger.fbx", "MOD_Rogue", true, 3, itemFx[RHAND], rescale: scale);
                    socketTrans[RHAND] = Matrix.CreateScale(0.25f) *
                        Matrix.CreateRotationX(MathHelper.ToRadians(70)) *
                        Matrix.CreateRotationY(MathHelper.ToRadians(-20)) *
                        Matrix.CreateTranslation(new Vector3(-.3f, 0.04f, 0));
                    socketedItems[LHAND] = skinModel_loader.Load("MOD_Rogue/MOD_Dagger.fbx", "MOD_Rogue", true, 3, itemFx[LHAND], rescale: scale);
                    socketTrans[LHAND] = Matrix.CreateScale(0.25f) *
                        Matrix.CreateRotationX(MathHelper.ToRadians(-20)) *
                        Matrix.CreateRotationY(MathHelper.ToRadians(1200)) *
                        Matrix.CreateRotationZ(MathHelper.ToRadians(60)) *
                        Matrix.CreateTranslation(new Vector3(.5f, 0f, 0));
                    break;
                case "MAGE":
                    //load Mage anims              
                    scale = 3f;
                    playerAnimWeights = new float[58];
                    playerModel[BASE] = skinModel_loader.Load("MOD_Mage/ANIM_Mage_Idle.fbx", "MOD_Mage", true, 3, skinFx, rescale: scale);

                    playerModel[WALK] = skinModel_loader.Load("MOD_Mage/ANIM_Mage_Walk.fbx", "MOD_Mage", true, 3, skinFx, rescale: scale);

                    playerModel[RUN] = skinModel_loader.Load("MOD_Mage/ANIM_Mage_Run.fbx", "MOD_Mage", true, 3, skinFx, rescale: scale);

                    playerModel[JUMP] = skinModel_loader.Load("MOD_Mage/ANIM_Mage_Jump.fbx", "MOD_Mage", true, 3, skinFx, rescale: scale);

                    playerModel[JUMP].loopAnimation = false;
                    playerModel[WALKBACK] = skinModel_loader.Load("MOD_Mage/ANIM_Mage_Walk_Rev.fbx", "MOD_Mage", true, 3, skinFx, rescale: scale);

                    playerModel[ATTACK1] = skinModel_loader.Load("MOD_Mage/ANIM_Mage_AttackMain.fbx", "MOD_Mage", true, 3, skinFx, rescale: scale);
                    playerModel[ATTACK1].loopAnimation = false;

                    playerModel[ATTACK2] = skinModel_loader.Load("MOD_Mage/ANIM_Mage_AttackSecondary.fbx", "MOD_Mage", true, 3, skinFx, rescale: scale);
                    playerModel[ATTACK2].loopAnimation = false;

                    playerModel[WALKLEFT] = skinModel_loader.Load("MOD_Mage/ANIM_Mage_Walk_Left.fbx", "MOD_Mage", true, 3, skinFx, rescale: scale);

                    playerModel[WALKRIGHT] = skinModel_loader.Load("MOD_Mage/ANIM_Mage_Walk_Right.fbx", "MOD_Mage", true, 3, skinFx, rescale: scale);

                    //set mage sockets 
                    socketIDs[LHAND] = (int)MageBones.LeftHand;
                    socketIDs[RHAND] = (int)MageBones.RightHand;
                    socketIDs[HEAD] = (int)MageBones.Head;

                    //Load Staff
                    socketedItems[RHAND] = skinModel_loader.Load("MOD_Mage/MOD_MageStaff.fbx", "MOD_Mage", true, 3, itemFx[RHAND], rescale: scale);
                    socketTrans[RHAND] = Matrix.CreateScale(0.35f) *
                        Matrix.CreateRotationX(MathHelper.ToRadians(60)) *
                        Matrix.CreateRotationY(MathHelper.ToRadians(-40)) *
                        Matrix.CreateRotationZ(MathHelper.ToRadians(220)) *
                        Matrix.CreateTranslation(new Vector3(-0.4f, -0.1f, 0));

                    break;
                case "CLERIC":
                    //load cleric anims
                    scale = 2.2f;
                    playerAnimWeights = new float[48];

                    playerModel[BASE] = skinModel_loader.Load("MOD_Cleric/ANIM_Cleric_Idle.fbx", "MOD_Cleric", true, 3, skinFx, rescale: scale);

                    playerModel[WALK] = skinModel_loader.Load("MOD_Cleric/ANIM_Cleric_Walk.fbx", "MOD_Cleric", true, 3, skinFx, rescale: scale);

                    playerModel[RUN] = skinModel_loader.Load("MOD_Cleric/ANIM_Cleric_Run.fbx", "MOD_Cleric", true, 3, skinFx, rescale: scale);

                    playerModel[JUMP] = skinModel_loader.Load("MOD_Cleric/ANIM_Cleric_Jump.fbx", "MOD_Cleric", true, 3, skinFx, rescale: scale);
                    playerModel[JUMP].loopAnimation = false;

                    playerModel[WALKBACK] = skinModel_loader.Load("MOD_Cleric/ANIM_Cleric_Walk_Rev.fbx", "MOD_Cleric", true, 3, skinFx, rescale: scale);

                    playerModel[ATTACK1] = skinModel_loader.Load("MOD_Cleric/ANIM_Cleric_AttackMain.fbx", "MOD_Cleric", true, 3, skinFx, rescale: scale);
                    playerModel[ATTACK1].loopAnimation = false;

                    playerModel[ATTACK2] = skinModel_loader.Load("MOD_Cleric/ANIM_Cleric_AttackSecondary.fbx", "MOD_Cleric", true, 3, skinFx, rescale: scale);
                    playerModel[ATTACK2].loopAnimation = false;

                    playerModel[WALKLEFT] = skinModel_loader.Load("MOD_Cleric/ANIM_Cleric_Walk_Left.fbx", "MOD_Cleric", true, 3, skinFx, rescale: scale);

                    playerModel[WALKRIGHT] = skinModel_loader.Load("MOD_Cleric/ANIM_Cleric_Walk_Right.fbx", "MOD_Cleric", true, 3, skinFx, rescale: scale);


                    //set cleric sockets 
                    socketIDs[LHAND] = 11;
                    socketIDs[RHAND] = 30;
                    socketIDs[HEAD] = 0;
                    break;
            }

            #endregion -- player models and anims

            #region <<LOAD ENEMY MODELS & ANIMATIONS>>
            // Enemies models
            enemyModels[HELLHOUND] = skinModel_loader.Load("MOD_HellHound/ANIM_Hellhound_Base.fbx", "MOD_HellHound", true, 3, skinFx, rescale: 3f, yRotation: -90);
            enemyModels[HELLHOUND].loopAnimation = false;
            enemyModels[IMP] = skinModel_loader.Load("MOD_Imp/ANIM_Imp_Base.fbx", "MOD_Imp", true, 3, skinFx, rescale: 3f);
            enemyModels[IMP].loopAnimation = false;
            enemyModels[MINOTAUR] = skinModel_loader.Load("MOD_Minotaur/ANIM_Minotaur_Base.fbx", "MOD_Minotaur", true, 3, skinFx, rescale: 3.2f);
            enemyModels[MINOTAUR].loopAnimation = false;
            enemyModels[REVENANT] = skinModel_loader.Load("MOD_Revenant/ANIM_Revenant_Base.fbx", "MOD_Revenant", true, 3, skinFx, rescale: 1.5f);
            enemyModels[REVENANT].loopAnimation = false;

            //enemy Animations

            #endregion

            #region LEVEL LOADING

            switch (levelID)
            {
                case 1:
                    thisLevel = levelLoader.Load("Level_1");
                    break;
                default:
                    //This shouldn't happen
                    thisLevel = levelLoader.Load("Level_1");
                    break;
            }
            Content.RootDirectory = "Content";

            // Levels
            thisLevel.Load();
            levelSpawnerList = thisLevel.Spawners;  //Create Spawners
            player.Pos = thisLevel.playerSpawn; //set player spawn location

            #endregion

            // Attacks
            playerProjModel = Content.Load<Model>("MODEL_PlayerProjectile");
            enemyProjModel = Content.Load<Model>("MODEL_EnemyProjectile");
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
        #endregion

        #region UPDATE-----------
        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (init) // INITIALIZE STARTING ANIMS
            {
                foreach (SkinModel sk in playerModel)
                {
                    if (sk != null) sk.BeginAnimation(0, gameTime);
                }
                foreach (SkinModel sk in socketedItems)
                {
                    if (sk != null) sk.BeginAnimation(0, gameTime);
                }
                foreach(EnemySpawner spawner in levelSpawnerList)
                {
                    if (spawner.enemyList != null && spawner.isActive) spawner.ActivateSpawner(gameTime);
                }
                foreach(Level.InteractiveDoor door in thisLevel.Doors)
                {
                    door.StartAnimation(gameTime);
                }
                init = false;
                
            }
            if(Keyboard.GetState().IsKeyDown(Keys.F2) && parent.lastKeyboard.IsKeyUp(Keys.F2))
            {
                int[] temp = { 1, 1, 1, 1 };
                levelSpawnerList[0].ActivateSpawner(temp, gameTime);
            }

            if (isDead) return;    //stop update if paused
            //pause game if window is tabbed out of
            if (parent.IsActive)
            {
                foreach (SkinModel an in playerModel)
                {
                    if (an != null) an.Update(gameTime);
                }
                foreach (SkinModel an in socketedItems)
                {
                    if (an != null) an.Update(gameTime);
                }
                player.Update(deltaTime, camera, gameTime);
                camera.Update(player.head, thisLevel);
                BlendPlayerAnims(gameTime);
            }
            //pause game on pressing esc
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) && parent.lastKeyboard.IsKeyUp(Keys.Escape))
            {
                parent.currentScene = "PAUSE";
                parent.pauseScene.Initialize();
                return;
            }

            //turn on debug
            if (Keyboard.GetState().IsKeyDown(Keys.F3) && parent.lastKeyboard.IsKeyUp(Keys.F3))
            {
                if (!debugMode) debugMode = true;
                else debugMode = false;
            }

            //reload level
            if (Keyboard.GetState().IsKeyDown(Keys.F4) && parent.lastKeyboard.IsKeyUp(Keys.F4))
            {
                levelModel = Content.Load<Model>("MODEL_Level1");
                thisLevel = levelLoader.Load("Level_1" + ".xml");
                thisLevel.Load();
                levelSpawnerList = thisLevel.Spawners;

                foreach (Level.InteractiveDoor door in thisLevel.Doors)
                {
                    door.StartAnimation(gameTime);
                }
            }

            foreach (EnemySpawner spawner in levelSpawnerList)
            {
                if (spawner.isActive == false) continue;
                spawner.Update(gameTime, player);
                foreach (Enemy e in spawner.enemyList)
                {
                    e.Update(deltaTime, gameTime, camera);
                    foreach (Attack p in e.projList)
                    {
                        p.Update(deltaTime, player,this);
                    }
                    if (e.TimeToDestroy)
                    {
                        spawner.enemyList.Remove(e);
                        break;
                    }
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

                p.Update(deltaTime, levelSpawnerList, this);
                //initialize projectile impacts
                if (p.TimeToDestroy == true)
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
                            Matrix secondaryProj = Matrix.CreateScale(0.2f) *
                                Matrix.CreateRotationY(MathHelper.ToRadians(90 + player.Rot.Y)) *
                                Matrix.CreateRotationZ(MathHelper.ToRadians(rand.Next(1, 180))) *
                                Matrix.CreateTranslation(p.Pos);
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
            if (player.Health <= 0)
            {
                isDead = true;
                parent.currentScene = "IS_DEAD";
                parent.lostScene.Initialize();
            }

            //Finish Level
            if (player.Pos.Z >= parent.levelLength)
            {
                parent.currentScene = "LEVEL_END";
                parent.levelEnd1.currentScore = score.ToString();
                parent.levelEnd1.Initialize();
                return;
            }

            base.Update(gameTime);
        }

        #endregion -- end update --

        #region SET 3D STATES -----------
        RasterizerState rs_ccw = new RasterizerState() { FillMode = FillMode.Solid, CullMode = CullMode.CullCounterClockwiseFace };
        void Set3dStates()
        {
            gpu.BlendState = BlendState.NonPremultiplied;
            gpu.DepthStencilState = DepthStencilState.Default;
            if (gpu.RasterizerState.CullMode == CullMode.None) { gpu.RasterizerState = rs_ccw;
            }
        }
        #endregion

        //function to do draw when player is playing in level.
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            bool debugDraw = debugMode;

            //draw Skybox
            sky.Draw(camera.view, camera.proj, player.Pos, gameTime);
            parent.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            parent.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            Set3dStates();

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            RasterizerState rasterizerStateWire = new RasterizerState();
            rasterizerStateWire.CullMode = CullMode.None;
            rasterizerStateWire.FillMode = FillMode.WireFrame;
            gpu.RasterizerState = rasterizerState;

            thisLevel.Draw(gameTime, camera, debugDraw);

            #region >>Debug Draw Level Components<<
            if (debugDraw)
            {
                gpu.RasterizerState = rasterizerStateWire;
                thisLevel.DebugDraw(gameTime, camera);
                gpu.RasterizerState = rasterizerState;
            }
            #endregion

            #region <<Debug Spawners>>
            if (debugDraw)
            {
                foreach (EnemySpawner spawner in levelSpawnerList)
                {
                    spawner.DebugDraw(gpu, camera);
                }
            }
            #endregion

            #region <<<Player Drawing>>>
            SkinModel hero = playerModel[BASE];

            int socketLoop = -1;
            skinFx.DisableFog();
            //draw socketed items
            foreach (SkinModel item in socketedItems)
            {
                socketLoop++;
                if (socketedItems[socketLoop] == null) continue;
                for (int i = 0; i < socketedItems[socketLoop].meshes.Length; i++)
                {
                    itemFx[socketLoop].SetDiffuseCol(Color.White.ToVector4());
                    itemFx[socketLoop].SetSpecularCol(new Vector3(1f, 0.1f, 0.05f));
                    itemFx[socketLoop].SetSpecularPow(256f);
                    itemFx[socketLoop].world = socketTrans[socketLoop] *
                        hero.GetBoneTransform(socketIDs[socketLoop])
                        * player.world;
                    socketedItems[socketLoop].DrawMesh(i, camera, itemFx[socketLoop].world);
                }
            }


            for (int i = 0; i < hero.meshes.Length; i++)
            {
                skinFx.SetDiffuseCol(Color.Red.ToVector4());
                skinFx.SetSpecularCol(new Vector3(1f, 0.1f, 0.05f));
                skinFx.SetSpecularPow(0.5f);
                hero.DrawMesh(i, camera, player.world);
            }

            #endregion

            #region <<Debug Player Projectile Hitboxes>>
            if (debugDraw == true) {
                //Uncomment to display attack hitboxes 
                gpu.RasterizerState = rasterizerStateWire;
                //draw hitboxes
                player.boundingCollider.DrawCubeDepict(camera);
                gpu.RasterizerState = rasterizerState;

                //Uncomment to display attack hitboxes         
                gpu.RasterizerState = rasterizerStateWire;
                if (player.projList.Count > 0)
                {   //draw hitboxes
                    player.projList[^1].HitBox.Draw(camera);
                }
                gpu.RasterizerState = rasterizerState;

            }
            #endregion

            skinFx.ToggleFog(); //RESET FOG

            // Render enemies
            foreach (EnemySpawner spawner in levelSpawnerList)
            {
                if (!spawner.isActive) continue;
                foreach (Enemy e in spawner.enemyList)
                {
                    #region <<Debug Enemy Hitboxes>>
                    if (debugDraw == true)
                    {
                        //Uncomment to display attack hitboxes 
                        gpu.RasterizerState = rasterizerStateWire;
                        //draw hitboxes
                        e.boundingCollider.DrawCubeDepict(camera);
                        if (e.projList.Count > 0)
                        {   //draw hitboxes
                            e.projList[0].HitBox.Draw(camera);
                        }
                        gpu.RasterizerState = rasterizerState;
                    }
                    #endregion
                    //draw enemy model

                    skinFx.DisableFog();
                    skinFx.SetFogColor(new Color(.2f, .0f, .0f, 1f));
                    skinFx.SetFogEnd(thisLevel.fogDistance);
                    skinFx.SetFogStart(thisLevel.fogDistance*-1);

                    e.Draw(gameTime, camera);

                    // Render enemy bullets
                    foreach (Attack p in e.projList)
                    {
                        worldProj = Matrix.CreateScale(1f) *
                            Matrix.CreateTranslation(p.Pos);
                        if (p.IsMelee)
                        {
                        }
                        else
                        {
                            enemyProjModel.Draw(worldProj, camera.view, camera.proj);
                        }
                    }
                }
            }

            if (parent.lostScene.fadeIn > 1)    return;

            #region <<Draw Projectiles>>
            // Render player bullets
            foreach (Attack p in player.projList)
            {
                worldProj = Matrix.CreateScale(1f) *
                        Matrix.CreateRotationZ(MathHelper.ToRadians(90)) *
                        Matrix.CreateTranslation(p.Pos + new Vector3(0, 5, 0));

                //Set 2D sprite world matrix
                Matrix secondaryProj = Matrix.CreateScale(0.029f) *
                    Matrix.CreateRotationY(MathHelper.ToRadians(90 + player.Rot.Y)) *
                    Matrix.CreateTranslation(p.Pos);

                if (!p.IsMelee)
                {

                    switch (player.role)
                    {
                        case "MAGE":
                            //draw 3d model for projectile
                            playerProjModel.Draw(worldProj, camera.view, camera.proj);
                            break;
                        case "CLERIC":
                            //draw a 2d sprite for the projectile
                            clericProjectile.Draw(secondaryProj, camera.view, camera.proj);
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
                        imp.Draw(camera.view, camera.proj);
                        break;
                }
            }
            #endregion


            #region <<Draw HUD>>
            parent.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            parent.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            _spriteBatch.Begin();
            _spriteBatch.Draw(hudL1, Vector2.Zero, null, hudFade, 0, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 0);
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

            //Interact notification
            if (player.nearInteractable)
            {
                _spriteBatch.DrawString(parent.smallFont, "Press F to interact",
                new Vector2(_graphics.PreferredBackBufferWidth * 0.408f - (parent.BigFont.MeasureString(score.ToString()) * .5f * parent.currentScreenScale).X,
                _graphics.PreferredBackBufferHeight * .75f), Color.White, 0f, Vector2.Zero, parent.currentScreenScale * 1.2f, SpriteEffects.None, 1);
            }

            // on screen debug info
            if (debugDraw)
            {

                string headingDeb = "(" + (Math.Round((double)player.head.Backward.X, 2)).ToString() + ", " +
                    (Math.Round((double)player.head.Backward.Y, 1)).ToString() + ", " +
                    (Math.Round((double)player.head.Backward.Z, 2)).ToString() + ")";
                _spriteBatch.DrawString(parent.smallFont, "Player Heading: " + headingDeb,
                new Vector2(_graphics.PreferredBackBufferWidth * 0.05f, _graphics.PreferredBackBufferHeight * .23f),
                Color.White, 0f, Vector2.Zero, parent.currentScreenScale * 0.8f, SpriteEffects.None, 1);

                string positionDeb = "(X = "+(Math.Round((double)player.Pos.X / 2.2f, 2)).ToString() + ", Y = " +
                    (Math.Round((double)player.Pos.Y / 2.2f, 1)).ToString() + ", Z = " +
                    (Math.Round((double)player.Pos.Z / 2.2f, 2)).ToString() + ")";
                _spriteBatch.DrawString(parent.smallFont, "Player pos: " +positionDeb,
                new Vector2(_graphics.PreferredBackBufferWidth * 0.05f, _graphics.PreferredBackBufferHeight * .25f),
                Color.White, 0f, Vector2.Zero, parent.currentScreenScale*0.8f, SpriteEffects.None, 1);

                _spriteBatch.DrawString(parent.smallFont, "FPS: " + Math.Round((1/gameTime.ElapsedGameTime.TotalSeconds)).ToString(),
                new Vector2(_graphics.PreferredBackBufferWidth * 0.05f, _graphics.PreferredBackBufferHeight * .27f),
                Color.White, 0f, Vector2.Zero, parent.currentScreenScale * 0.8f, SpriteEffects.None, 1);
            }
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
            #endregion

            FadeIn(0.01f);
            _spriteBatch.End();
            parent.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            parent.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
        }


        #region <<Blend Animations>>  
        public void BlendPlayerAnims(GameTime gameTime)
        {
            //reset weights for blending
            SetAnimBlends(1);
            float percent;
            switch (player.role)
            {
                case "WARRIOR":
                    #region Warrior Blending
                    //walk/run blending
                    if (player.isWalking())
                    {
                        percent = player.vel / player.walkMax;
                        if (player.animWalkDir[player.BACKWARD])
                        {
                            playerModel[BASE].UpdateBlendAnim(playerModel, BASE, WALKBACK, percent, playerAnimWeights);   //idle -> walkBack
                        }
                        if (player.animWalkDir[player.FORWARD])
                        {
                            playerModel[BASE].UpdateBlendAnim(playerModel, BASE, WALK, percent, playerAnimWeights);   //idle -> walk
                        }
                        if (player.animWalkDir[player.LEFT])
                        {
                            if (player.animWalkDir[player.FORWARD] || player.animWalkDir[player.BACKWARD]) percent = 0.5f;  //mix movement
                            playerModel[BASE].UpdateBlendAnim(playerModel, BASE, WALKLEFT, percent, playerAnimWeights);   //idle -> walkBack
                        }
                        if (player.animWalkDir[player.RIGHT])
                        {
                            if (player.animWalkDir[player.FORWARD] || player.animWalkDir[player.BACKWARD]) percent = 0.5f;  //mix movement
                            playerModel[BASE].UpdateBlendAnim(playerModel, BASE, WALKRIGHT, percent, playerAnimWeights);   //idle -> walkBack
                        }
                    }
                    if (player.isRunning())
                    {
                        percent = 1;
                        playerModel[BASE].UpdateBlendAnim(playerModel, WALK, RUN, percent, playerAnimWeights);   //walk -> run
                    }

                    //jump blending
                    if (player.jumping || playerModel[ATTACK1].animationRunning)
                    {
                        SetAnimBlends(1);
                        if (player.jumpDel <= 1) playerModel[JUMP].BeginAnimation(0, gameTime); //Start Jump Anim
                        playerModel[BASE].UpdateBlendAnim(playerModel, BASE, JUMP, .5f, playerAnimWeights);   //walk -> run
                    }

                    //Attack Blending//
                    SetAnimBlends(0);


                    //favour arms for blend animations
                    SetAnimBlends((int)WarriorBones.LeftShoulderTrans, (int)WarriorBones.LeftShoulderRot, 1.25f);
                    SetAnimBlends((int)WarriorBones.LeftArmTrans, (int)WarriorBones.LeftArmRot, 1.25f);
                    playerAnimWeights[(int)WarriorBones.LeftForeArm] = 1.25f;
                    playerAnimWeights[(int)WarriorBones.LeftHand] = 1.25f;

                    SetAnimBlends((int)WarriorBones.RightShoulderTrans, (int)WarriorBones.RightShoulderRot, 1.25f);
                    SetAnimBlends((int)WarriorBones.RightArmTrans, (int)WarriorBones.RightArmScale, 1.25f);
                    playerAnimWeights[(int)WarriorBones.RightForeArm] = 1.25f;
                    playerAnimWeights[(int)WarriorBones.RightHand] = 1.25f;

                    SetAnimBlends((int)WarriorBones.Spine2Trans, (int)WarriorBones.Spine2Rot, 1.25f);
                    playerAnimWeights[(int)WarriorBones.Spine1] = 1.25f;
                    SetAnimBlends((int)WarriorBones.SpineTrans, (int)WarriorBones.SpineRot, 1.25f);
                    SetAnimBlends((int)WarriorBones.HipsTrans, (int)WarriorBones.HipsRot, 1.25f);

                    //Main Attack
                    if (player.isAttacking[0] || playerModel[ATTACK1].animationRunning)
                    {
                        if (!playerModel[ATTACK1].animationRunning) playerModel[ATTACK1].BeginAnimation(0, gameTime); //Start attack Anim
                        if (playerModel[ATTACK1].currentAnimFrameTime > 0.6 && player.isAttacking[0])
                        {   //force restart animation
                            playerModel[ATTACK1].BeginAnimation(0, gameTime); //Start attack Anim
                        }
                        percent = 0.8f;
                        playerModel[BASE].UpdateBlendAnim(playerModel, BASE, ATTACK1, percent, playerAnimWeights);   //base -> Main Attack
                    }
                    //Secondary Attack
                    if (player.isAttacking[1] || playerModel[ATTACK2].animationRunning)
                    {
                        if (!playerModel[ATTACK2].animationRunning) playerModel[ATTACK2].BeginAnimation(0, gameTime); //Start attack Anim
                        if (playerModel[ATTACK2].currentAnimFrameTime > 0.6 && player.isAttacking[1])
                        {   //force restart animation
                            playerModel[ATTACK2].BeginAnimation(0, gameTime); //Start attack Anim
                        }
                        percent = 1 - (float)(playerModel[ATTACK2].currentAnimFrameTime * 0.5);
                        playerModel[BASE].UpdateBlendAnim(playerModel, BASE, ATTACK2, percent, playerAnimWeights);   //base -> Secondary Attack
                    }
                    #endregion
                    break;
                case "ROGUE":
                    #region Rogue Blending
                    //walk/run blending
                    if (player.isWalking())
                    {
                        percent = player.vel / player.walkMax;
                        if (player.animWalkDir[player.BACKWARD])
                        {
                            playerModel[BASE].UpdateBlendAnim(playerModel, BASE, WALKBACK, percent, playerAnimWeights);   //idle -> walkBack
                        }
                        if (player.animWalkDir[player.FORWARD])
                        {
                            playerModel[BASE].UpdateBlendAnim(playerModel, BASE, WALK, percent, playerAnimWeights);   //idle -> walk
                        }
                        if (player.animWalkDir[player.LEFT])
                        {
                            SetAnimBlends((int)RogueBones.LeftArmTrans, (int)RogueBones.LeftArmScale, 1);
                            SetAnimBlends((int)RogueBones.RightArmTrans, (int)RogueBones.RightArmScale, 1);
                            if (player.animWalkDir[player.FORWARD] || player.animWalkDir[player.BACKWARD]) percent = 0.5f;  //mix movement
                            playerModel[BASE].UpdateBlendAnim(playerModel, BASE, WALKLEFT, percent, playerAnimWeights);   //idle -> walkBack
                        }
                        if (player.animWalkDir[player.RIGHT])
                        {
                            SetAnimBlends((int)RogueBones.LeftArmTrans, (int)RogueBones.LeftArmScale, 1);
                            SetAnimBlends((int)RogueBones.RightArmTrans, (int)RogueBones.RightArmScale, 1);
                            if (player.animWalkDir[player.FORWARD] || player.animWalkDir[player.BACKWARD]) percent = 0.5f;  //mix movement
                            playerModel[BASE].UpdateBlendAnim(playerModel, BASE, WALKRIGHT, percent, playerAnimWeights);   //idle -> walkBack
                        }
                    }
                    if (player.isRunning())
                    {
                        percent = 1;
                        playerModel[BASE].UpdateBlendAnim(playerModel, WALK, RUN, percent, playerAnimWeights);   //walk -> run
                    }

                    //jump blending
                    if (player.jumping)
                    {
                        SetAnimBlends(1);
                        if (player.jumpDel <= 1) playerModel[JUMP].BeginAnimation(0, gameTime); //Start Jump Anim
                        playerModel[BASE].UpdateBlendAnim(playerModel, BASE, JUMP, .7f, playerAnimWeights);   //walk -> run
                    }
                    else
                    {
                        playerModel[JUMP].StopAnimation();
                    }

                    //Attack Blending//
                    SetAnimBlends(0);

                    //favour arms for blend animations
                    SetAnimBlends((int)RogueBones.LeftShoulderTrans, (int)RogueBones.LeftShoulderRot, 1);
                    SetAnimBlends((int)RogueBones.LeftArmTrans, (int)RogueBones.LeftArmRot, 1);
                    playerAnimWeights[(int)RogueBones.LeftForeArm] = 1;
                    playerAnimWeights[(int)RogueBones.LeftHand] = 1;

                    SetAnimBlends((int)RogueBones.RightShoulderTrans, (int)RogueBones.RightShoulderRot, 1);
                    SetAnimBlends((int)RogueBones.RightArmTrans, (int)RogueBones.RightArmScale, 1);
                    playerAnimWeights[(int)RogueBones.RightForeArm] = 1;
                    playerAnimWeights[(int)RogueBones.RightHand] = 1;

                    SetAnimBlends((int)RogueBones.Spine2Trans, (int)RogueBones.Spine2Rot, 1);
                    playerAnimWeights[(int)RogueBones.Spine1] = 1;

                    //Main Attack
                    if (player.isAttacking[0] || playerModel[ATTACK1_2].animationRunning)
                    {
                        if (!playerModel[ATTACK1_2].animationRunning) playerModel[ATTACK1_2].BeginAnimation(0, gameTime); //Start attack Anim
                        if (playerModel[ATTACK1_2].currentAnimFrameTime > 0.6 && player.isAttacking[0])
                        {   //force restart animation
                            playerModel[ATTACK1_2].BeginAnimation(0, gameTime); //Start attack Anim
                        }
                        percent = 1 - playerModel[ATTACK1_2].currentAnimFrameTime;
                        playerModel[BASE].UpdateBlendAnim(playerModel, BASE, ATTACK1_2, percent, playerAnimWeights);   //base -> Main Attack
                    }
                    //Secondary Attack
                    if (player.isAttacking[1] || playerModel[ATTACK2].animationRunning)
                    {

                        SetAnimBlends(1);
                        if (!playerModel[ATTACK2].animationRunning) playerModel[ATTACK2].BeginAnimation(0, gameTime); //Start attack Anim
                        if (playerModel[ATTACK2].currentAnimFrameTime > 0.6 && player.isAttacking[1])
                        {   //force restart animation
                            playerModel[ATTACK2].BeginAnimation(0, gameTime); //Start attack Anim
                        }
                        percent = 1;
                        playerModel[BASE].UpdateBlendAnim(playerModel, BASE, ATTACK2, percent, playerAnimWeights);   //base -> Secondary Attack
                    }
                    break;
                #endregion
                case "MAGE":
                    #region Mage Blending
                    //walk/run blending
                    if (player.isWalking())
                    {
                        percent = player.vel / player.walkMax;
                        if (player.animWalkDir[player.BACKWARD])
                        {
                            playerModel[BASE].UpdateBlendAnim(playerModel, BASE, WALKBACK, percent, playerAnimWeights);   //idle -> walkBack
                        }
                        if (player.animWalkDir[player.FORWARD])
                        {
                            playerModel[BASE].UpdateBlendAnim(playerModel, BASE, WALK, percent, playerAnimWeights);   //idle -> walk
                        }
                        if (player.animWalkDir[player.LEFT])
                        {
                            SetAnimBlends((int)MageBones.LeftArmTrans, (int)MageBones.LeftArmScale, 1);
                            SetAnimBlends((int)MageBones.RightArmTrans, (int)MageBones.RightArmScale, 1);
                            if (player.animWalkDir[player.FORWARD] || player.animWalkDir[player.BACKWARD]) percent = 0.5f;  //mix movement
                            playerModel[BASE].UpdateBlendAnim(playerModel, BASE, WALKLEFT, percent, playerAnimWeights);   //idle -> walkBack
                        }
                        if (player.animWalkDir[player.RIGHT])
                        {
                            SetAnimBlends((int)MageBones.LeftArmTrans, (int)MageBones.LeftArmScale, 1);
                            SetAnimBlends((int)MageBones.RightArmTrans, (int)MageBones.RightArmScale, 1);
                            if (player.animWalkDir[player.FORWARD] || player.animWalkDir[player.BACKWARD]) percent = 0.5f;  //mix movement
                            playerModel[BASE].UpdateBlendAnim(playerModel, BASE, WALKRIGHT, percent, playerAnimWeights);   //idle -> walkBack
                        }
                    }
                    if (player.isRunning())
                    {
                        percent = 1;
                        playerModel[BASE].UpdateBlendAnim(playerModel, WALK, RUN, percent, playerAnimWeights);   //walk -> run
                    }

                    //jump blending
                    if (player.jumping)
                    {
                        SetAnimBlends(1);
                        if (player.jumpDel <= 1) playerModel[JUMP].BeginAnimation(0, gameTime); //Start Jump Anim
                        playerModel[BASE].UpdateBlendAnim(playerModel, BASE, JUMP, .7f, playerAnimWeights);   //walk -> run
                    }
                    else
                    {
                        playerModel[JUMP].StopAnimation();
                    }

                    //Attack Blending//
                    SetAnimBlends(0);

                    //favour arms for blend animations
                    SetAnimBlends((int)MageBones.LeftShoulderTrans, (int)MageBones.LeftShoulderScale, 1);
                    SetAnimBlends((int)MageBones.LeftArmTrans, (int)MageBones.LeftArmScale, 1);
                    SetAnimBlends((int)MageBones.LeftForearmTrans, (int)MageBones.LeftForearmScale, 1);
                    playerAnimWeights[(int)MageBones.LeftHand] = 1;

                    SetAnimBlends((int)MageBones.RightShoulderTrans, (int)MageBones.RightShoulderScale, 1);
                    SetAnimBlends((int)MageBones.RightArmTrans, (int)MageBones.RightArmScale, 1);
                    SetAnimBlends((int)MageBones.RightForeArmTrans, (int)MageBones.RightForeArmScale, 1);
                    playerAnimWeights[(int)MageBones.RightHand] = 1;

                    SetAnimBlends((int)MageBones.Spine2Trans, (int)MageBones.Spine2Scale, 1);
                    SetAnimBlends((int)MageBones.Spine1Trans, (int)MageBones.Spine1Scale, 1);
                    SetAnimBlends((int)MageBones.SpineTrans, (int)MageBones.SpineScale, 1);
                    SetAnimBlends((int)MageBones.NeckTrans, (int)MageBones.NeckRot, 1);

                    //Main Attack
                    if (player.isAttacking[0] || playerModel[ATTACK1].animationRunning)
                    {
                        if (!playerModel[ATTACK1].animationRunning) playerModel[ATTACK1].BeginAnimation(0, gameTime); //Start attack Anim
                        if (playerModel[ATTACK1].currentAnimFrameTime > 0.6 && player.isAttacking[0])
                        {   //force restart animation
                            playerModel[ATTACK1].BeginAnimation(0, gameTime); //Start attack Anim
                        }
                        percent = 1 - playerModel[ATTACK1].currentAnimFrameTime;
                        playerModel[BASE].UpdateBlendAnim(playerModel, BASE, ATTACK1, percent, playerAnimWeights);   //base -> Main Attack
                    }
                    //Secondary Attack
                    if (player.isAttacking[1] || playerModel[ATTACK2].animationRunning)
                    {
                        if (!playerModel[ATTACK2].animationRunning) playerModel[ATTACK2].BeginAnimation(0, gameTime); //Start attack Anim
                        if (playerModel[ATTACK2].currentAnimFrameTime > 0.6 && player.isAttacking[1])
                        {   //force restart animation
                            playerModel[ATTACK2].BeginAnimation(0, gameTime); //Start attack Anim
                        }
                        percent = 1 - playerModel[ATTACK1].currentAnimFrameTime;
                        playerModel[BASE].UpdateBlendAnim(playerModel, BASE, ATTACK2, percent, playerAnimWeights);   //base -> Secondary Attack
                    }
                    break;
                #endregion
                case "CLERIC":
                    #region Cleric Blending
                    //walk/run blending
                    if (player.isWalking() || player.isDrifting)
                    {
                        percent = player.vel / player.walkMax;
                        if (player.animWalkDir[player.BACKWARD])
                        {
                            playerModel[BASE].UpdateBlendAnim(playerModel, BASE, WALKBACK, percent, playerAnimWeights);   //idle -> walkBack
                        }
                        if (player.animWalkDir[player.FORWARD])
                        {
                            playerModel[BASE].UpdateBlendAnim(playerModel, BASE, WALK, percent, playerAnimWeights);   //idle -> walk
                        }
                        if (player.animWalkDir[player.LEFT])
                        {
                            SetAnimBlends((int)ClericBones.LeftArmTrans, (int)ClericBones.LeftArmScale, 2);
                            SetAnimBlends((int)ClericBones.RightArmTrans, (int)ClericBones.RightArmScale, 2);
                            if (player.animWalkDir[player.FORWARD] || player.animWalkDir[player.BACKWARD]) percent = 0.5f;  //mix movement
                            playerModel[BASE].UpdateBlendAnim(playerModel, BASE, WALKLEFT, percent, playerAnimWeights);   //idle -> walkBack
                        }
                        if (player.animWalkDir[player.RIGHT])
                        {
                            SetAnimBlends((int)ClericBones.LeftArmTrans, (int)ClericBones.LeftArmScale, 2);
                            SetAnimBlends((int)ClericBones.RightArmTrans, (int)ClericBones.RightArmScale, 2);
                            if (player.animWalkDir[player.FORWARD] || player.animWalkDir[player.BACKWARD]) percent = 0.5f;  //mix movement
                            playerModel[BASE].UpdateBlendAnim(playerModel, BASE, WALKRIGHT, percent, playerAnimWeights);   //idle -> walkBack
                        }
                    }
                    if (player.isRunning())
                    {
                        percent = 1;
                        playerModel[BASE].UpdateBlendAnim(playerModel, WALK, RUN, percent, playerAnimWeights);   //walk -> run
                    }

                    //jump blending
                    if (player.jumping)
                    {
                        SetAnimBlends(1);
                        if (player.jumpDel <= 1) playerModel[JUMP].BeginAnimation(0, gameTime); //Start Jump Anim
                        playerModel[BASE].UpdateBlendAnim(playerModel, BASE, JUMP, .7f, playerAnimWeights);   //walk -> run
                    }
                    else
                    {
                        playerModel[JUMP].StopAnimation();
                    }

                    //Attack Blending//
                    SetAnimBlends(0);

                    //favour arms for blend animations
                    SetAnimBlends((int)ClericBones.LeftShoulderTrans, (int)ClericBones.LeftShoulderRot, 1);
                    SetAnimBlends((int)ClericBones.LeftArmTrans, (int)ClericBones.LeftArmScale, 1);
                    playerAnimWeights[(int)ClericBones.LeftForearm] = 1;
                    playerAnimWeights[(int)ClericBones.LeftHand] = 1;

                    SetAnimBlends((int)ClericBones.RightShoulderTrans, (int)ClericBones.RightShoulderRot, 1);
                    SetAnimBlends((int)ClericBones.RightArmTrans, (int)ClericBones.RightArmScale, 1);
                    playerAnimWeights[(int)ClericBones.RightForeArm] = 1;
                    playerAnimWeights[(int)ClericBones.RightHand] = 1;

                    SetAnimBlends((int)ClericBones.Spine2Trans, (int)ClericBones.Spine2Rot, 1);
                    playerAnimWeights[(int)ClericBones.Spine1] = 1;

                    //Main Attack
                    if (player.isAttacking[0] || playerModel[ATTACK1].animationRunning)
                    {
                        if (!playerModel[ATTACK1].animationRunning) playerModel[ATTACK1].BeginAnimation(0, gameTime); //Start attack Anim
                        if (playerModel[ATTACK1].currentAnimFrameTime > 0.6 && player.isAttacking[0])
                        {   //force restart animation
                            playerModel[ATTACK1].BeginAnimation(0, gameTime); //Start attack Anim
                        }
                        percent = 1 - playerModel[ATTACK1].currentAnimFrameTime;
                        playerModel[BASE].UpdateBlendAnim(playerModel, BASE, ATTACK1, percent, playerAnimWeights);   //base -> Main Attack
                    }
                    //Secondary Attack
                    if (player.isAttacking[1] || playerModel[ATTACK2].animationRunning)
                    {
                        if (!playerModel[ATTACK2].animationRunning) playerModel[ATTACK2].BeginAnimation(0, gameTime); //Start attack Anim
                        if (playerModel[ATTACK2].currentAnimFrameTime > 0.6 && player.isAttacking[1])
                        {   //force restart animation
                            playerModel[ATTACK2].BeginAnimation(0, gameTime); //Start attack Anim
                        }
                        percent = 1 - playerModel[ATTACK2].currentAnimFrameTime;
                        playerModel[BASE].UpdateBlendAnim(playerModel, BASE, ATTACK2, percent, playerAnimWeights);   //base -> Secondary Attack
                    }
                    break;
                    #endregion
            }
        }


        //set specific playerAnimWeights
        private void SetAnimBlends(int start, int end, float val)
        {
            for (int i = start; i <= end; i++)
            {
                playerAnimWeights[i] = val;
            }
        }

        //Set all playerAnimWeights
        private void SetAnimBlends(int val)
        {
            for (int i = 0; i <= playerAnimWeights.Length - 1; i++)
            {
                playerAnimWeights[i] = val;
            }
        }
        #endregion --blend animations--


        ////////////////////
        ///GETTER/SETTERS///
        ////////////////////
        public Camera Camera
        {
            get { return camera; }
        }

        #region NESTED CLASSES -------------------------------------------------------------------------------------------------------------------------------------
        //<Spawner for enemies>
        public class EnemySpawner
        {
            public Vector3 location;        //location of spawner in world
            public List<Enemy> enemyList;   //Local enemies activated by spwaner
            private Random rand;            //random class
            private PlayScene parent;       //scene holding the spawner
            private float enemyTimer, enemyTimerMax;
            private float spawnTimer, spawnTime;       //timer for spawnining enemies
            private int spawnRange;        //radius of general spawner
            public Shapes origin, perimiter;//shapes for drawing spawner
            public bool isSingleSpawner;    //spawn a specific enemy under certain conditions
            public bool isActive;           //sets whether to activate or update enemies
            private int[] spawnList;
            private bool enemysActivated;
            private int activateRange;

            #region <<Contructors>>
            //Constructor -- general spawner
            public EnemySpawner(PlayScene parent_, Vector3 loc, int spawnRange_)
            {
                location = loc;
                rand = new Random();
                parent = parent_;
                spawnRange = spawnRange_;
                isSingleSpawner = false;
                enemyList = new List<Enemy>();

                //creat debug spawner shapes
                origin = new Shapes(parent.gpu,Color.Coral, Vector3.Zero, loc, typeof(EnemySpawner));
                origin.DefineCuboid(3, 3, 0, 15, 3, 3);
                origin.Position = loc;
                perimiter = new Shapes(parent.gpu, Color.Coral, Vector3.Zero, loc, typeof(EnemySpawner));
                perimiter.DefineCuboid(spawnRange, spawnRange, spawnRange, spawnRange, spawnRange, spawnRange);
                perimiter.Position = loc;
            }

            //Constructor -- single spawner
            public EnemySpawner(PlayScene parent_, Vector3 loc, int enemyRole, float activationRadius)
            {
                location = loc;
                rand = new Random();
                parent = parent_;
                isSingleSpawner = true;
                enemysActivated = false;

                //creat debug spawner shapes
                origin = new Shapes(parent.gpu, Color.Coral, Vector3.Zero, loc, typeof(EnemySpawner));
                origin.DefineCuboid((int)location.X + 1, (int)location.X - 1, 0, 15, (int)location.Z + 1, (int)location.Z - 1);
                perimiter = new Shapes(parent.gpu, Color.Coral, Vector3.Zero, loc, typeof(EnemySpawner));
                perimiter.DefineCuboid((int)location.X + spawnRange, (int)location.X - spawnRange, 0,15, (int)location.Z + spawnRange, (int)location.Z - spawnRange);
            }

            //Constructor -- with defined spawning list
            public EnemySpawner(PlayScene Parent, Vector3 loc, int SpawnRange, int activationRadius, int[] SpawnList, bool startActive)
            {
                location = loc*2.2f;
                rand = new Random();
                parent = Parent;
                isSingleSpawner = false;
                spawnRange = SpawnRange;
                spawnList = SpawnList;
                enemyList = new List<Enemy>();
                isActive = startActive;
                spawnTimer = 10;
                spawnTime = 0;
                enemysActivated = false;
                if (activationRadius == -1) activateRange = spawnRange * 4;
                else activateRange = activationRadius;

                //creat debug spawner shapes
                origin = new Shapes(parent.gpu, Color.Coral, Vector3.Zero, location, typeof(EnemySpawner));
                origin.DefineCuboid(3, 3, 0, 15, 3, 3);
                origin.Position = location;
                perimiter = new Shapes(parent.gpu, Color.Coral, Vector3.Zero, location, typeof(EnemySpawner));
                perimiter.DefineCuboid(spawnRange, spawnRange, spawnRange, spawnRange, spawnRange, spawnRange);
                perimiter.Position = location;

            }
            #endregion

            // spawn enemies around spawner
            public void SpawnEnemy(int enemyType, int amt, GameTime gameTime)
            {
                for (int i = 0; i < amt; i++)
                {
                    Vector3 spawnLoc = new Vector3(location.X + rand.Next(spawnRange * -1, spawnRange), location.Y, location.Z + rand.Next(spawnRange * -1, spawnRange));

                    switch (enemyType) {
                        case 0:
                            enemyList.Add(new Imp(parent.enemySounds, Enemy.ROLES[enemyType], spawnLoc, parent, parent.enemyModels[enemyType], parent.Content));
                            break;
                        case 1:
                            enemyList.Add(new HellHound(parent.enemySounds, Enemy.ROLES[enemyType], spawnLoc, parent, parent.enemyModels[enemyType], parent.Content));
                            break;
                        case 2:
                            enemyList.Add(new Minotaur(parent.enemySounds, Enemy.ROLES[enemyType], spawnLoc, parent, parent.enemyModels[enemyType], parent.Content));
                            break;
                        case 3:
                            enemyList.Add(new Revenant(parent.enemySounds, Enemy.ROLES[enemyType], spawnLoc, parent, parent.enemyModels[enemyType], parent.Content));
                            break;
                        default:
                            break;
                    }
                }
            }

            public void Update(GameTime gameTime, Player player)
            {
                if (!enemysActivated && spawnList[0] + spawnList[1] + spawnList[2] + spawnList[3] <= 0)
                {
                    if (Vector3.Distance(player.Pos, location) < activateRange)
                    {
                        foreach(Enemy enemy in enemyList)
                        {
                            enemy.isActive = true;
                        }
                        enemysActivated = true;
                    }
                }
                if (isActive)
                {
                    if (spawnList[0] + spawnList[1] + spawnList[2] + spawnList[3] > 0)
                    {
                        if (spawnTime <= spawnTimer) { spawnTime++; return; }
                        if (spawnList[0] > 0)
                        {
                            SpawnEnemy(0, 1, gameTime);
                            spawnList[0]--;
                        }
                        if (spawnList[1] > 0)
                        {
                            SpawnEnemy(1, 1, gameTime);
                            spawnList[1]--;
                        }
                        if (spawnList[2] > 0)
                        {
                            SpawnEnemy(2, 1, gameTime);
                            spawnList[2]--;
                        }
                        if (spawnList[3] > 0)
                        {
                            SpawnEnemy(3, 1, gameTime);
                            spawnList[3]--;
                        }
                    }
                }
            }
            //activate to spawner with list of enemies to spawn (Rarey used)
            public void ActivateSpawner(int[] spawnAmounts, GameTime gameTime)
            {
                for(int i = 0; i < 4; i++)
                {
                    SpawnEnemy(i, spawnAmounts[i], gameTime);
                }
                isActive = true;
            }
            //activate to spawner with local list
            public void ActivateSpawner(GameTime gameTime)
            {
                isActive = true;
            }
            public void DebugDraw(GraphicsDevice gpu, Camera cam)
            {
                RasterizerState rasterizerState = new RasterizerState();
                rasterizerState.CullMode = CullMode.None;
                rasterizerState.FillMode = FillMode.Solid;
                RasterizerState rasterizerStateWire = new RasterizerState();
                rasterizerStateWire.CullMode = CullMode.None;
                rasterizerStateWire.FillMode = FillMode.WireFrame;
                gpu.RasterizerState = rasterizerStateWire;
                perimiter.Rotation = new Vector3(0, 0, 0);
                perimiter.Draw(cam);
                gpu.RasterizerState = rasterizerState;
                origin.Draw(cam);
            }
    }

        public class CollisionSet
        {
            List<BoundingBox> collisionBoxes;
            List<BoundingSphere> collisionSpheres;

            public CollisionSet(float bounciness_)
            {
                collisionBoxes = new List<BoundingBox>();
                collisionSpheres = new List<BoundingSphere>();
            }

            public List<Vector3> GetCollisions (BoundingBox checker, bool favourSelf, bool checkSpheres = false)
            {
                List<Vector3> outList = new List<Vector3>();
                foreach(BoundingBox box in collisionBoxes)
                {
                    if (checker.Intersects(box))
                    {
                        outList.Add(Vector3.Lerp(box.Min, box.Max, 0.5f));
                    }
                }
                return outList;
            }

            public void Add(BoundingBox newB)
            {
                collisionBoxes.Add(newB);
            }
            public void Add(BoundingSphere newB)
            {
                collisionSpheres.Add(newB);
            }

        }

        #endregion
    }
}
