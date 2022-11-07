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
    public class Level
    {
        private SkinFx levelFx;
        private SkinModelLoader model_loader;   //loader for meshes
        private List<SkinModel> meshes;         //meshes for level
        private List<string> existingFiles;     //files already loaded
        private List<int> missionProgress;      //check progress towards goal

        //<<Lists of Level components>>
        private List<FloorTile> floorPieces;
        private List<StaticCollisionMesh> staticCollisionMeshes;
        private List<StaticMesh> staticMeshes;
        private List<DeathBox> deathBoxes;
        private List<InteractiveDoor> doors;
        private List<PlayScene.EnemySpawner> enemySpawners;
        private List<Activator> activators;
        private List<SpawnerTriggerBox> spawnTriggers;
        private List<MissionTriggerBox> missionTriggers;


        //Level data
        public int levelId;
        public Color fogColor;
        public int fogDistance;
        public Vector3 playerSpawn;

        public Level(ContentManager cont, GraphicsDevice gpu_, Camera cam)
        {
            levelFx = new SkinFx(cont, cam, "SkinEffect");
            fogColor = Color.White;
            fogDistance = 0;
            levelFx.ToggleFog();
            model_loader = new SkinModelLoader(cont, gpu_);
            model_loader.SetDefaultOptions(0.1f, "default_tex");
            meshes = new List<SkinModel>();
            existingFiles = new List<string>();

            //make component lists
            floorPieces           = new List<FloorTile>();
            staticCollisionMeshes = new List<StaticCollisionMesh>();
            enemySpawners         = new List<PlayScene.EnemySpawner>();
            staticMeshes          = new List<StaticMesh>();
            deathBoxes            = new List<DeathBox>();
            doors                 = new List<InteractiveDoor>();
            activators            = new List<Activator>();
            spawnTriggers         = new List<SpawnerTriggerBox>();
            missionTriggers       = new List<MissionTriggerBox>();

            missionProgress = new List<int>(new int[15]);
        }

        #region Set Level Data
        public void SetData(int id, Vector4 FogColor, int FogDistance, Vector3 pSpawn)
        {
            levelId = id;
            fogColor = new Color(FogColor.X, FogColor.Y, FogColor.Z, FogColor.W);
            fogDistance = FogDistance;
            playerSpawn = pSpawn;
        }

        //set fog color by color
        public void SetFogColor(Color col)
        {
            fogColor = col;
        }
        //set fog color from Vector 4
        public void SetFogColor(Vector4 FogColor)
        {
            fogColor = new Color(FogColor.X, FogColor.Y, FogColor.Z, FogColor.W);
        }
        //change fog distance
        public void SetFogdistance(int dis)
        {
            fogDistance = dis;
        }

        #endregion


        //load meshes into items
        public void Load()
        {
            UpdateSkinFX();
            if (staticCollisionMeshes.Count != 0)
            {
                foreach (StaticCollisionMesh mesh in staticCollisionMeshes)
                {
                    mesh.Load(model_loader, meshes, existingFiles, levelFx);
                    floorPieces.Add(mesh.top);
                }
            }
            if (floorPieces.Count != 0)
            {
                foreach (FloorTile floor in floorPieces)
                {
                    floor.Load(model_loader, meshes, existingFiles, levelFx);
                }
            }
            if (staticMeshes.Count != 0)
            {
                foreach (StaticMesh mesh in staticMeshes)
                {
                    mesh.Load(model_loader, meshes, existingFiles, levelFx);
                }
            }
            if (doors.Count != 0)
            {
                foreach (InteractiveDoor door in doors)
                {
                    door.Load(model_loader, meshes, existingFiles, levelFx);
                }
            }
            if (activators.Count != 0)
            {
                foreach (Activator activator in activators)
                {
                    activator.Load(model_loader, meshes, existingFiles, levelFx, this);
                }
            }
            if (spawnTriggers.Count != 0)
            {
                foreach (SpawnerTriggerBox trigger in spawnTriggers)
                {
                    trigger.Load(this);
                }
            }

        }


        #region Add Content Functions
        public void Add(List<FloorTile> toAdd)
        {
            floorPieces.AddRange(toAdd);
        }
        public void Add(List<StaticCollisionMesh> toAdd)
        {
            staticCollisionMeshes.AddRange(toAdd);
        }
        public void Add(List<PlayScene.EnemySpawner> toAdd)
        {
            enemySpawners.AddRange(toAdd);
        }
        public void Add(List<StaticMesh> toAdd)
        {
            staticMeshes.AddRange(toAdd);
        }
        public void Add(List<DeathBox> toAdd)
        {
            deathBoxes.AddRange(toAdd);
        }
        public void Add(List<InteractiveDoor> toAdd)
        {
            doors.AddRange(toAdd);
        }
        public void Add(List<Activator> toAdd)
        {
            activators.AddRange(toAdd);
        }
        public void Add(List<SpawnerTriggerBox> toAdd)
        {
            spawnTriggers.AddRange(toAdd);
        }
        public void Add(List<MissionTriggerBox> toAdd)
        {
            missionTriggers.AddRange(toAdd);
        }
        #endregion

        #region Mission Progress Functions

        public void MissionUpdate(int missionID, GameTime gameTime)
        {
            switch (missionID){
                case 0:
                    Mission00(gameTime);
                    break;
                case 1:
                    Mission01(gameTime);
                    break;
                case 2:
                    Mission02(gameTime);
                    break;
            }
        }

        #region Level 01 Missions
        //-------------
        // Mission to activate each pedastal to open the boss door
        //-------------
        private void Mission00(GameTime gameTime)
        {
            missionProgress[0]++;
            if (missionProgress[0] >= 3)
            {
                doors[3].isLocked = false;
                doors[3].openQueued = true;
                Spawners[0].isActive = true;
                Spawners[0].ActivateSpawner(gameTime);
            }
        }

        //-------------
        //close Boss door and begin boss fight
        //-------------
        private void Mission01(GameTime gameTime)
        {
            missionProgress[1]++;
            if (missionProgress[1] >= 1)
            {
                doors[doors.Count-1].Open(gameTime);
                doors[doors.Count-1].isLocked = true;
            }
        }

        //-------------
        //End level when boss is beaten
        //-------------
        private void Mission02(GameTime gameTime)
        {
            missionProgress[1]++;
            if (missionProgress[1] >= 1)
            {
                doors[doors.Count - 1].Open(gameTime);
                doors[doors.Count - 1].isLocked = true;
            }
        }
        #endregion


        #endregion

        #region Draw Functions
        //debugdraw for hitboxes that should be wireframed
        public void DebugDraw(GameTime gameTime, Camera cam)
        {
            if (spawnTriggers.Count != 0)
            {
                foreach (SpawnerTriggerBox box in spawnTriggers)
                {
                    UpdateSkinFX();
                    box.Draw(cam, true);
                }
            }
            if (doors.Count != 0)
            {
                foreach (InteractiveDoor door in doors)
                {
                    UpdateSkinFX();
                    door.Draw(gameTime, cam, true);
                }
            }
            if (activators.Count != 0)
            {
                foreach (Activator activator in activators)
                {
                    UpdateSkinFX();
                    activator.Draw(gameTime, cam, true);
                }
            }
            if (missionTriggers.Count != 0)
            {
                foreach (MissionTriggerBox activator in missionTriggers)
                {
                    UpdateSkinFX();
                    activator.Draw(cam, true);
                }
            }
        }

        public void Draw(GameTime gameTime, Camera cam, bool debug = false)
        {
            foreach (FloorTile floor in floorPieces)
            {
                UpdateSkinFX();
                floor.Draw(gameTime, cam, debug);
            }
            foreach (StaticCollisionMesh sMesh in staticCollisionMeshes)
            {
                UpdateSkinFX();
                sMesh.Draw(gameTime, cam, debug);
            }
            foreach (StaticMesh sMesh in staticMeshes)
            {
                UpdateSkinFX();
                sMesh.Draw(gameTime, cam);
            }
            foreach (DeathBox deathBoxes in deathBoxes)
            {
                UpdateSkinFX();
                deathBoxes.Draw(gameTime, cam, debug);
            }
            foreach (InteractiveDoor door in doors)
            {
                UpdateSkinFX();
                door.Draw(gameTime, cam, false);
            }
            foreach (Activator activator in activators)
            {
                UpdateSkinFX();
                activator.Draw(gameTime, cam, false);
            }
        }

        public void UpdateSkinFX()
        {
            levelFx.SetEmissiveCol(Color.White.ToVector3());
            levelFx.SetSpecularCol(Color.Red.ToVector3());
            levelFx.SetSpecularPow(1f);
            levelFx.SetFogColor(fogColor);
            levelFx.SetFogStart(-4 * fogDistance);
            levelFx.SetFogEnd(fogDistance*-1f);
        }

        #endregion

        #region Getters / Setters
        public List<FloorTile> FloorTiles
        {
            get { return floorPieces; }
        }
        public List<StaticCollisionMesh> StaticCollisionMeshes
        {
            get { return staticCollisionMeshes; }
        }
        public PlayScene.EnemySpawner[] Spawners
        {
            get { return enemySpawners.ToArray(); }
        }
        public List<DeathBox> DeathBoxes
        {
            get { return deathBoxes; }
        }
        public List<InteractiveDoor> Doors
        {
            get { return doors; }
        }
        public List<Activator> Activators
        {
            get { return activators; }
        }
        public List<SpawnerTriggerBox> Triggers
        {
            get { return spawnTriggers; }
        }
        public List<MissionTriggerBox> MissionTriggers
        {
            get { return missionTriggers; }
        }


        #endregion


        #region -------------Component Classes-----------------
        public class FloorTile
        {
            private string path;
            private string filename;
            public SkinModel mesh;
            private Vector3 position;
            private Vector3 rotation;
            private Matrix world;
            public Shapes collider;

            //floor tile w model
            public FloorTile(string Path, string Filename, Vector3 Position, Vector3 Rotation, GraphicsDevice gpu_, Vector3 scale)
            {
                path = Path;
                filename = Filename;
                position = Position*2.2f;
                rotation = Rotation;

                collider = new Shapes(gpu_, Color.Cyan, Rotation, position, isStatic_: true);
                collider.DefineCuboid((int)(scale.X), (int)(scale.X), (int)(scale.Y / 2), (int)(scale.Y / 2), (int)(scale.Z), (int)(scale.Z));

                world = Matrix.CreateScale(2.2f) *
                Matrix.CreateRotationX(MathHelper.ToRadians(rotation.X)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(rotation.Y)) *
                Matrix.CreateRotationZ(MathHelper.ToRadians(rotation.Z)) *
                Matrix.CreateTranslation(position);
            }
            //floor tile w no model
            public FloorTile(Vector3 Position, Vector3 Rotation, GraphicsDevice gpu_, Vector3 scale)
            {
                path = null;
                filename = null;
                position = Position*2.2f;
                rotation = Rotation;

                collider = new Shapes(gpu_, Color.Cyan, Rotation, position, isStatic_:true);
                collider.DefineCuboid((int)(scale.X), (int)(scale.X), (int)(scale.Y/2), (int)(scale.Y/2), (int)(scale.Z), (int)(scale.Z));

                world = Matrix.CreateScale(2.2f) *
                Matrix.CreateRotationX(MathHelper.ToRadians(rotation.X)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(rotation.Y)) *
                Matrix.CreateRotationZ(MathHelper.ToRadians(rotation.Z)) *
                Matrix.CreateTranslation(position);
            }

            public void Load(SkinModelLoader loader, List<SkinModel> meshes, List<string> alreadyLoaded, SkinFx levelFx)
            {
                if (filename == null) return;   //break for floors with no mesh
                if (!alreadyLoaded.Contains(filename))
                {
                    meshes.Add(loader.Load(path + @"\" + filename, path, true, 3, levelFx, rescale: 2.2f));
                    alreadyLoaded.Add(filename);
                }

                mesh = meshes[alreadyLoaded.IndexOf(filename)];
            }

            public void Draw(GameTime gameTime, Camera cam,  bool debug = false)
            {
                bool doRender = collider.Intersects(cam.renderSpace);   //check if in view frustum
                if (!doRender) return;                                  //dont draw when out of view
                if (debug) { collider.Draw(cam);}                       //draw only collision box in debug mode
                if (mesh == null) return;                               //dont draw if just a collision box
                for (int i = 0; i < mesh.meshes.Length; i++)
                {
                    mesh.DrawMesh(i, cam, world);
                }
                
            }

            #region Getters / Setters
            public Vector3 Position
            {
                get { return position; }
            }
            public Vector3 Rotation
            {
                get { return rotation; }
            }
            #endregion

        }

        public class StaticCollisionMesh
        {
            private string path;
            private string filename;
            public SkinModel mesh;
            public readonly FloorTile top;
            private Vector3 position;
            private Vector3 rotation;
            private Matrix world;
            public Shapes collider;


            public StaticCollisionMesh(string Path, string Filename, Vector3 Position, Vector3 Rotation, GraphicsDevice gpu_, Vector3 scale)
            {
                path = Path;
                filename = Filename;
                position = Position*2.2f;
                rotation = Rotation;

                collider = new Shapes(gpu_, Color.BlueViolet, Rotation, position, isStatic_: true);
                collider.DefineCuboid((int)(scale.X), (int)(scale.X), (int)(scale.Y), (int)(scale.Y), (int)(scale.Z), (int)(scale.Z));

                world = Matrix.CreateScale(2.2f) *
                Matrix.CreateRotationX(MathHelper.ToRadians(rotation.X)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(rotation.Y)) *
                Matrix.CreateRotationZ(MathHelper.ToRadians(rotation.Z)) *
                Matrix.CreateTranslation(position);

                Matrix floorWorld = Matrix.CreateScale(1) *
                Matrix.CreateRotationX(MathHelper.ToRadians(rotation.X)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(rotation.Y)) *
                Matrix.CreateRotationZ(MathHelper.ToRadians(rotation.Z)) *
                Matrix.CreateTranslation(Position);

                top = new FloorTile(Position + floorWorld.Up* (scale.Y*0.6f), Rotation, gpu_, new Vector3(scale.X, 0.5f*scale.Y, scale.Z));
            }

            public void Load(SkinModelLoader loader, List<SkinModel> meshes, List<string> alreadyLoaded, SkinFx levelFx)
            {
                if (filename == "") return;
                if (!alreadyLoaded.Contains(filename))
                {
                    meshes.Add(loader.Load(path + @"\" + filename, path, true, 3, levelFx, rescale: 4.2f));
                    alreadyLoaded.Add(filename);
                }

                mesh = meshes[alreadyLoaded.IndexOf(filename)];
            }

            public void Draw(GameTime gameTime, Camera cam, bool debug = false)
            {
                bool doRender = (Vector3.Distance(position, cam.pos) < 400 && collider.Intersects(cam.renderSpace));   //check if in view frustum or close to player
                if (!doRender) return;                                  //dont draw when out of view
                if (debug) { collider.Draw(cam); return; }              //draw only collision box in debug mode
                if (filename == "") return;                                 //break if no model to draw
                if (mesh == null) return;                               //dont draw if just a collision box
                for (int i = 0; i < mesh.meshes.Length; i++)
                {
                    mesh.DrawMesh(i, cam, world);
                    mesh.BeginAnimation(0, gameTime);
                }

            }

            #region Getters / Setters
            public Vector3 Position
            {
                get { return position; }
            }
            #endregion

        }

        public class DeathBox
        {
            public SkinModel mesh;
            public readonly FloorTile top;
            private Vector3 position;
            private Vector3 rotation;
            public Shapes collider;


            public DeathBox(Vector3 Position, Vector3 Rotation, GraphicsDevice gpu_, Vector3 scale)
            {
                position = Position * 2.2f;
                rotation = Rotation;

                collider = new Shapes(gpu_, Color.Crimson, rotation, position, isStatic_: true);
                collider.DefineCuboid((int)(scale.X), (int)(scale.X), (int)(scale.Y), (int)(scale.Y), (int)(scale.Z), (int)(scale.Z));

            }

            public void Draw(GameTime gameTime, Camera cam, bool debug = false)
            {
                bool doRender = (Vector3.Distance(position, cam.pos) < 200 || collider.Intersects(cam.renderSpace));   //check if in view frustum or close to player
                if (!doRender) return;                                  //dont draw when out of view
                if (debug) { collider.Draw(cam); return; }              //draw only collision box in debug mode
            }

            #region Getters / Setters
            public Vector3 Position
            {
                get { return position; }
            }
            #endregion

        }

        public class StaticMesh
        {
            private string path;
            private string filename;
            public SkinModel mesh;
            private Vector3 position;
            private Vector3 rotation;
            private Matrix world;
            public Shapes collider;


            public StaticMesh(string Path, string Filename, Vector3 Position, Vector3 Rotation, GraphicsDevice gpu_, Vector3 scale)
            {
                path = Path;
                filename = Filename;
                position = Position*2.2f;
                rotation = Rotation;

                collider = new Shapes(gpu_, Color.BlueViolet, Rotation, position, isStatic_: true);
                collider.DefineCuboid((int)scale.X, (int)scale.X, (int)scale.Y, (int)scale.Y, (int)scale.Z, (int)scale.Z);


                world = Matrix.CreateScale(2.2f) *
                Matrix.CreateRotationX(MathHelper.ToRadians(rotation.X)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(rotation.Y)) *
                Matrix.CreateRotationZ(MathHelper.ToRadians(rotation.Z)) *
                Matrix.CreateTranslation(position);
            }

            public void Load(SkinModelLoader loader, List<SkinModel> meshes, List<string> alreadyLoaded, SkinFx levelFx)
            {
                if (!alreadyLoaded.Contains(filename))
                {
                    meshes.Add(loader.Load(path + @"\" + filename, path, false, 3, levelFx, rescale: 1f));
                    alreadyLoaded.Add(filename);
                }

                mesh = meshes[alreadyLoaded.IndexOf(filename)];
            }

            public void Draw(GameTime gameTime, Camera cam)
            {
                bool doRender = (Vector3.Distance(position, cam.pos) < 300 || collider.Intersects(cam.renderSpace));   //check if in view frustum or close to player
                if (!doRender) return;                                  //dont draw when out of view
                for (int i = 0; i < mesh.meshes.Length; i++)
                {
                    mesh.DrawMesh(i, cam, world);
                }

            }

            #region Getters / Setters
            public Vector3 Position
            {
                get { return position; }
            }
            #endregion

        }

        public class InteractiveDoor
        {
            //mesh info
            private string name;
            private string path;
            private string filename;
            public SkinModel mesh;

            //location info
            private Vector3 position;
            private Vector3 rotation;
            private Matrix world;

            //colliders
            public Shapes leftCollider;
            public Shapes rightCollider;
            public Shapes interactBox;

            //open info
            public bool isLocked;
            public bool isOpen;
            public bool opening;
            public bool closeable;
            private float rightOpenRot, leftOpenRot;
            public bool openQueued;
            public bool active;


            public InteractiveDoor(string Path, string Filename, string doorName, Vector3 Position, Vector3 Rotation,
                GraphicsDevice gpu_, Vector3 lScale, Vector3 rScale, bool lockState, float doorRotL, float doorRotR, bool canClose)
            {
                path = Path;
                filename = Filename;
                position = Position * 2.2f;
                rotation = Rotation;
                isLocked = lockState;
                name = doorName;
                isOpen = false;
                opening = false;
                rightOpenRot = doorRotR;
                leftOpenRot = doorRotL;
                openQueued = false;
                active = true;
                closeable = canClose;

                world = Matrix.CreateScale(2.2f) *
                Matrix.CreateRotationX(MathHelper.ToRadians(rotation.X)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(rotation.Y)) *
                Matrix.CreateRotationZ(MathHelper.ToRadians(rotation.Z)) *
                Matrix.CreateTranslation(position);

                Vector3 doorPos = position + (world.Backward * lScale.Z/2.2f) + (world.Up * lScale.Y / 4.4f); ;
                leftCollider = new Shapes(gpu_, Color.YellowGreen, rotation, doorPos, isStatic_: false);
                leftCollider.DefineCuboid((int)(lScale.X), (int)(lScale.X), (int)(lScale.Y), (int)(lScale.Y), 0, (int)(lScale.Z));

                doorPos = position + (world.Forward * rScale.Z / 2.2f) + (world.Up * rScale.Y / 4.4f); ;
                rightCollider = new Shapes(gpu_, Color.YellowGreen, rotation, doorPos, isStatic_: false);
                rightCollider.DefineCuboid((int)(rScale.X), (int)(rScale.X), (int)(rScale.Y), (int)(rScale.Y), (int)(rScale.Z), 0);

                Vector3 interactBounds = new Vector3(lScale.X * 10, (int)(lScale.Y + rScale.Y), (int)(lScale.Z) + (int)(rScale.Z));
                interactBox = new Shapes(gpu_, Color.DarkMagenta, rotation, position + (world.Up * lScale.Y / 4.4f), isStatic_: true);
                interactBox.DefineCuboid(interactBounds);


                world = Matrix.CreateScale(1f) *
                Matrix.CreateRotationX(MathHelper.ToRadians(rotation.X)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(rotation.Y+180f)) *
                Matrix.CreateRotationZ(MathHelper.ToRadians(rotation.Z)) *
                Matrix.CreateTranslation(position);
            }

            public void Load(SkinModelLoader loader, List<SkinModel> meshes, List<string> alreadyLoaded, SkinFx levelFx)
            {
                if (filename == "") return;
                mesh = loader.Load(path + @"\" + filename, path, true, 3, levelFx, rescale: 2.2f);
                mesh.loopAnimation = false;
            }

            public void Draw(GameTime gameTime, Camera cam, bool debug = false)
            {
                bool doRender = Vector3.Distance(position, cam.pos) < 200 || interactBox.Intersects(cam.renderSpace);   //check if in view frustum or close to player
                if (!doRender) return;                                  //dont draw when out of view

                if (openQueued && interactBox.Intersects(cam.renderSpace))
                {
                    Open(gameTime); //open if player triggred from another room
                    openQueued = false;
                }

                //draw only collision box in debug mode
                if (debug)
                {
                    leftCollider.Draw(cam);
                    rightCollider.Draw(cam);
                    interactBox.Draw(cam);
                }
                if (filename == "") return;                                 //break if no model to draw
                if (mesh == null) return;                               //dont draw if just a collision box

                if (!isOpen)    mesh.BeginAnimation(0, gameTime);   //reset local animation

                if (opening)
                {
                    mesh.Update(gameTime);
                    if(mesh.currentAnimFrameTime > 0.7f)
                    {
                        opening = false;
                    }
                }

                for (int i = 0; i < mesh.meshes.Length; i++)
                {
                    mesh.DrawMesh(i, cam, world);
                }
            }

            public void StartAnimation(GameTime gt)
            {
                mesh.BeginAnimation(0, gt);
                mesh.Update(gt);
            }

            public void Open(GameTime gameTime)
            {
                if (isOpen)
                {
                    if (!closeable) return;
                    leftCollider.Rotation = rotation;
                    rightCollider.Rotation = rotation;
                    isOpen = false;
                    opening = false;
                    mesh.BeginAnimation(0, gameTime);
                    mesh.Update(gameTime);
                    return;
                }

                leftCollider.Rotation += new Vector3(0, leftOpenRot, 0);
                rightCollider.Rotation += new Vector3(0, rightOpenRot, 0);
                opening = true;
                isOpen = true;

                if (!closeable) active = false;
            }

            #region Getters / Setters
            public Vector3 Position
            {
                get { return position; }
            }
            public string Name
            {
                get { return name; }
            }
            #endregion

        }

        public class Activator
        {
            //mesh info
            private string name;
            private string path;
            private string filename;
            public SkinModel mesh;

            //location info
            private Vector3 position;
            private Vector3 rotation;
            private Matrix world;

            //collision
            public Shapes collider;
            public Shapes interactiveBox;

            //game Logic
            private int progressID;
            private int uses;
            public bool active;

            public Activator(string Path, string Filename, Vector3 Position, Vector3 Rotation, GraphicsDevice gpu_, Vector3 scale,
                int targetid, int uses_ = 0)
            {
                path = Path;
                filename = Filename;
                position = Position * 2.2f;
                rotation = Rotation;
                active = true;

                progressID = targetid;
                uses = uses_;

                interactiveBox = new Shapes(gpu_, Color.CornflowerBlue, Rotation, position, isStatic_: true);
                interactiveBox.DefineCuboid((int)scale.X+8, (int)scale.X + 8, (int)scale.Y + 8, (int)scale.Y + 8, (int)scale.Z + 8, (int)scale.Z + 8);

                world = Matrix.CreateScale(2.2f) *
                Matrix.CreateRotationX(MathHelper.ToRadians(rotation.X)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(rotation.Y)) *
                Matrix.CreateRotationZ(MathHelper.ToRadians(rotation.Z)) *
                Matrix.CreateTranslation(position);

            }

            public void Load(SkinModelLoader loader, List<SkinModel> meshes, List<string> alreadyLoaded, SkinFx levelFx, Level parent)
            {
                if (!alreadyLoaded.Contains(filename))
                {
                    meshes.Add(loader.Load(path + @"\" + filename, path, false, 3, levelFx, rescale: 1f));
                    alreadyLoaded.Add(filename);
                }

                mesh = meshes[alreadyLoaded.IndexOf(filename)];

            }

            public void Draw(GameTime gameTime, Camera cam, bool debug = false)
            {
                bool doRender = (Vector3.Distance(position, cam.pos) < 200 || interactiveBox.Intersects(cam.renderSpace));   //check if in view frustum or close to player
                if (!doRender) return;                                  //dont draw when out of view
                if (debug)
                {
                    interactiveBox.Draw(cam);
                }              
                if (filename == "") return;                             //break if no model to draw
                if (mesh == null) return;                               //dont draw if just a collision box
                for (int i = 0; i < mesh.meshes.Length; i++)
                {
                    mesh.DrawMesh(i, cam, world);
                }
            }

            public void Activate(Level parent, GameTime gameTime)
            {
                if (uses <= 0) return;
                parent.MissionUpdate(progressID, gameTime);
                uses--;
                if (uses <= 0) active = false;
            }

            #region Getters / Setters
            public Vector3 Position
            {
                get { return position; }
            }
            #endregion

        }

        public class SpawnerTriggerBox
        {

            //location info
            private Vector3 position;

            //collision
            public Shapes collider;
            public Shapes interactiveBox;

            //game Logic
            private int targetID;
            private int uses;
            private PlayScene.EnemySpawner targetSpawner;

            public SpawnerTriggerBox(Vector3 Position, Vector3 Rotation, GraphicsDevice gpu_, Vector3 scale, int targetid, int uses_ = 0)
            {
                position = Position * 2.2f;

                targetID = targetid;
                uses = uses_;

                interactiveBox = new Shapes(gpu_, Color.Fuchsia, Rotation, position, isStatic_: true);
                interactiveBox.DefineCuboid((int)scale.X, (int)scale.X, (int)scale.Y, (int)scale.Y, (int)scale.Z, (int)scale.Z);
            }

            public void Load(Level parent)
            {
                targetSpawner = parent.Spawners[targetID];
            }

            public void Draw(Camera cam, bool debug = false)
            {
                bool doRender = (Vector3.Distance(position, cam.pos) < 200 || interactiveBox.Intersects(cam.renderSpace));   //check if in view frustum or close to player
                if (!doRender) return;                                  //dont draw when out of view
                if (debug)
                {
                    interactiveBox.Draw(cam);
                }

            }

            public void Activate(GameTime gameTime)
            {
                if (uses <= 0) return;
                targetSpawner.isActive = true;
                targetSpawner.ActivateSpawner(gameTime);
                uses--;
            }

            #region Getters / Setters
            public Vector3 Position
            {
                get { return position; }
            }
            #endregion

        }

        public class MissionTriggerBox
        {

            //location info
            private Vector3 position;

            //collision
            public Shapes collider;
            public Shapes interactiveBox;

            //game Logic
            private int targetID;
            private int uses;

            public MissionTriggerBox(Vector3 Position, Vector3 Rotation, GraphicsDevice gpu_, Vector3 scale, int targetid, int uses_ = 0)
            {
                position = Position * 2.2f;

                targetID = targetid;
                uses = uses_;

                interactiveBox = new Shapes(gpu_, Color.LemonChiffon, Rotation, position, isStatic_: true);
                interactiveBox.DefineCuboid((int)scale.X, (int)scale.X, (int)scale.Y, (int)scale.Y, (int)scale.Z, (int)scale.Z);
            }

            public void Draw(Camera cam, bool debug = false)
            {
                bool doRender = (Vector3.Distance(position, cam.pos) < 200 || interactiveBox.Intersects(cam.renderSpace));   //check if in view frustum or close to player
                if (!doRender) return; //dont draw when out of view
                if (debug) interactiveBox.Draw(cam);
            }

            public void Activate(Level parent, GameTime gameTime)
            {
                if (uses <= 0) return;
                parent.MissionUpdate(targetID,gameTime);
                uses--;
            }

            #region Getters / Setters
            public Vector3 Position
            {
                get { return position; }
            }
            #endregion

        }

        #endregion
    }


}
