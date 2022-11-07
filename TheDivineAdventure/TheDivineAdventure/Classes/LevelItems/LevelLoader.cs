using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Diagnostics;
using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using TheDivineAdventure.SkinModels;


namespace TheDivineAdventure
{
    public class LevelLoader
    {
        PlayScene parent;
        GraphicsDevice gpu;
        ContentManager content;                    //content manager
        Camera cam;
        string path;                               //path to load data from
        XmlDocument level;                         //level datafile
        public SkinFx          levelFX;            //fx for the level (need to look closer at editing)


        public LevelLoader(string levelsFolder, ContentManager cont, PlayScene Parent)
        {
            parent = Parent;
            gpu = parent.gpu;
            content = cont;
            path = levelsFolder;
            level = new XmlDocument();
            cam = parent.Camera;
        }

        public Level Load(string levelFile)
        {
            levelFile += ".xml";
            Level outLevel = new Level(content, gpu, cam);  //create output loaded level

            string file = Path.Combine(Path.Combine(Environment.CurrentDirectory, path), levelFile);   //file location for level file
            //string file = Path.Combine(@"D:\Holder\School\TDA_Repository\The-Divine-Adventure\TheDivineAdventure\TheDivineAdventure\Content\Level_Data\Level_1", levelFile);   //file location for level file

            if (File.Exists(file))
            {
                level.Load(file);
                Debug.WriteLine("Loaded: " + file);
            } else { Debug.WriteLine("Failed to load file: " + file); return null; }

            List<Level.FloorTile> floorsToAdd = new List<Level.FloorTile>();
            List<Level.StaticCollisionMesh> staticColMeshToAdd = new List<Level.StaticCollisionMesh>();
            List<Level.DeathBox> deathboxToAdd = new List<Level.DeathBox>();
            List<Level.StaticMesh> staticMeshToAdd = new List<Level.StaticMesh>();
            List<Level.InteractiveDoor> doorsToAdd = new List<Level.InteractiveDoor>();
            List<Level.Activator> activatorToAdd = new List<Level.Activator>();
            List<Level.SpawnerTriggerBox> spawntriggersToAdd = new List<Level.SpawnerTriggerBox>();
            List<Level.MissionTriggerBox> missionTriggersToAdd = new List<Level.MissionTriggerBox>();
            List<PlayScene.EnemySpawner> spawnersToAdd = new List<PlayScene.EnemySpawner>();

            //Get each rooom contents
            foreach (XmlNode room in level.DocumentElement?.ChildNodes)
            {
                switch (room.Name)
                {
                    case "room":
                        foreach (XmlNode prefab in room.ChildNodes)
                        {
                            Vector3 prefPos = new Vector3();
                            Vector3 prefRot = new Vector3();
                            string name = "";

                            //Get prefab transform
                            foreach (XmlNode data in prefab.ChildNodes)
                            {
                                if (data.Name == "prefab_name")
                                {
                                    name = data.FirstChild.InnerText;
                                }
                                if (data.Name == "pos")
                                {
                                    prefPos.X = float.Parse(data.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                    prefPos.Y = float.Parse(data.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                    prefPos.Z = float.Parse(data.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                }
                                if (data.Name == "rot")
                                {
                                    prefRot.X = float.Parse(data.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                    prefRot.Y = float.Parse(data.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                    prefRot.Z = float.Parse(data.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                }

                            }

                            Matrix prefabWorld = Matrix.CreateScale(1) *
                                Matrix.CreateRotationX(MathHelper.ToRadians(prefRot.X)) *
                                Matrix.CreateRotationY(MathHelper.ToRadians(prefRot.Y)) *
                                Matrix.CreateRotationZ(MathHelper.ToRadians(prefRot.Z)) *
                                Matrix.CreateTranslation(prefPos);

                            //Grab compnents of Prefab
                            foreach (XmlNode child in prefab.ChildNodes)
                            {
                                string filename = "";
                                Vector3 position = Vector3.Zero;
                                Vector3 rotation = Vector3.Zero;
                                Vector3 dims = Vector3.Zero;
                                int range = 0;
                                int[] spawnList = new int[4];
                                int targetID = 0;
                                int uses = 1;
                                switch (child.Name)
                                {
                                    case "floor":
                                        #region load floor tile
                                        foreach (XmlNode floor in child.ChildNodes)
                                        {
                                            if (floor.Name == "model")
                                            {
                                                filename = floor.FirstChild.InnerText;
                                            }
                                            if (floor.Name == "pos")
                                            {
                                                position.X = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                position.Y = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                position.Z = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                                position = Vector3.Transform(position, prefabWorld);
                                            }
                                            if (floor.Name == "rot")
                                            {
                                                rotation.X = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                rotation.Y = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                rotation.Z = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                                rotation += prefRot;
                                            }
                                            if (floor.Name == "coll_bounds")
                                            {
                                                dims.X = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                dims.Y = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                dims.Z = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                            }

                                        }
                                        if (filename == "") floorsToAdd.Add(new Level.FloorTile(position, rotation, gpu, dims));
                                        else floorsToAdd.Add(new Level.FloorTile(path, filename, position, rotation, gpu, dims));
                                        continue;
                                    #endregion
                                    case "static_collision_mesh":
                                        #region load Static Collision Mesh
                                        foreach (XmlNode floor in child.ChildNodes)
                                        {
                                            if (floor.Name == "model")
                                            {
                                                filename = floor.FirstChild.InnerText;
                                            }
                                            if (floor.Name == "pos")
                                            {
                                                position.X = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                position.Y = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                position.Z = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                                position = Vector3.Transform(position, prefabWorld);
                                            }
                                            if (floor.Name == "rot")
                                            {
                                                rotation.X = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                rotation.Y = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                rotation.Z = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                                rotation += prefRot;
                                            }
                                            if (floor.Name == "coll_bounds")
                                            {
                                                dims.X = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                dims.Y = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                dims.Z = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                            }

                                        }
                                        staticColMeshToAdd.Add(new Level.StaticCollisionMesh(path, filename, position, rotation, gpu, dims));
                                        continue;
                                    #endregion
                                    case "death_box":
                                        #region load Death Boxes
                                        foreach (XmlNode deathBox in child.ChildNodes)
                                        {
                                            if (deathBox.Name == "pos")
                                            {
                                                position.X = float.Parse(deathBox.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                position.Y = float.Parse(deathBox.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                position.Z = float.Parse(deathBox.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                                position = Vector3.Transform(position, prefabWorld);
                                            }
                                            if (deathBox.Name == "rot")
                                            {
                                                rotation.X = float.Parse(deathBox.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                rotation.Y = float.Parse(deathBox.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                rotation.Z = float.Parse(deathBox.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                                rotation += prefRot;
                                            }
                                            if (deathBox.Name == "coll_bounds")
                                            {
                                                dims.X = float.Parse(deathBox.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                dims.Y = float.Parse(deathBox.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                dims.Z = float.Parse(deathBox.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                            }

                                        }
                                        deathboxToAdd.Add(new Level.DeathBox(position, rotation, gpu, dims));
                                        continue;
                                    #endregion
                                    case "spawner":
                                        #region load Spawner
                                        bool startActive = true;
                                        int activateRange = -1;
                                        foreach (XmlNode floor in child.ChildNodes)
                                        {
                                            if (floor.Name == "pos")
                                            {
                                                position.X = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                position.Y = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                position.Z = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                                position = Vector3.Transform(position, prefabWorld);
                                            }
                                            if (floor.Name == "range")
                                            {
                                                range = int.Parse(floor.FirstChild.InnerText);
                                            }
                                            if (floor.Name == "activate_range")
                                            {
                                                activateRange = int.Parse(floor.FirstChild.InnerText);
                                            }
                                            if (floor.Name == "spawn_list")
                                            {
                                                spawnList[0] = int.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                spawnList[1] = int.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                spawnList[2] = int.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                                spawnList[3] = int.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[3]);
                                            }
                                            if (floor.Name == "starts_active")
                                            {
                                                if (floor.FirstChild.InnerText == "true") startActive = true;
                                                else startActive = false;
                                            }

                                        }
                                        spawnersToAdd.Add(new PlayScene.EnemySpawner(parent, position, range, activateRange, spawnList, startActive));
                                        continue;
                                    #endregion
                                    case "static_mesh":
                                        #region load Static Collision Mesh
                                        foreach (XmlNode floor in child.ChildNodes)
                                        {
                                            if (floor.Name == "model")
                                            {
                                                filename = floor.FirstChild.InnerText;
                                            }
                                            if (floor.Name == "pos")
                                            {
                                                position.X = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                position.Y = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                position.Z = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                                position = Vector3.Transform(position, prefabWorld);
                                            }
                                            if (floor.Name == "rot")
                                            {
                                                rotation.X = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                rotation.Y = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                rotation.Z = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                                rotation += prefRot;
                                            }
                                            if (floor.Name == "coll_bounds")
                                            {
                                                dims.X = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                dims.Y = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                dims.Z = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                            }
                                        }
                                        staticMeshToAdd.Add(new Level.StaticMesh(path, filename, position, rotation, gpu, dims));
                                        continue;
                                    #endregion
                                    case "door":
                                        #region Load Door
                                        bool closeable = false;
                                        bool isLocked = false;
                                        Vector3 rBounds = Vector3.Zero;
                                        Vector3 lBounds = Vector3.Zero;
                                        float rRot = 0;
                                        float lRot = 0;
                                        foreach (XmlNode floor in child.ChildNodes)
                                        {
                                            if (floor.Name == "model")
                                            {
                                                filename = floor.FirstChild.InnerText;
                                            }
                                            if (floor.Name == "pos")
                                            {
                                                position.X = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                position.Y = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                position.Z = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                                position = Vector3.Transform(position, prefabWorld);
                                            }
                                            if (floor.Name == "rot")
                                            {
                                                rotation.X = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                rotation.Y = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                rotation.Z = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                                rotation += prefRot;
                                            }
                                            if (floor.Name == "left_door_bounds")
                                            {
                                                lBounds.X = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                lBounds.Y = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                lBounds.Z = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                            }
                                            if (floor.Name == "right_door_bounds")
                                            {
                                                rBounds.X = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                rBounds.Y = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                rBounds.Z = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                            }
                                            if (floor.Name == "left_door_rot")
                                            {
                                                lRot = float.Parse(floor.FirstChild.InnerText);
                                            }
                                            if (floor.Name == "right_door_rot")
                                            {
                                                rRot = float.Parse(floor.FirstChild.InnerText);
                                            }
                                            if (floor.Name == "is_locked")
                                            {
                                                if (floor.FirstChild.InnerText == "true") isLocked = true;
                                                else isLocked = false;
                                            }
                                            if (floor.Name == "closeable")
                                            {
                                                if (floor.FirstChild.InnerText == "true") closeable = true;
                                                else closeable = false;
                                            }
                                        }
                                        doorsToAdd.Add(new Level.InteractiveDoor(path, filename, name, position, rotation, gpu, lBounds, rBounds, isLocked, lRot, rRot, closeable));
                                        continue;
                                    #endregion
                                    case "activator":
                                        #region Load Activator
                                        foreach (XmlNode floor in child.ChildNodes)
                                        {
                                            if (floor.Name == "model")
                                            {
                                                filename = floor.FirstChild.InnerText;
                                            }
                                            if (floor.Name == "pos")
                                            {
                                                position.X = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                position.Y = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                position.Z = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                                position = Vector3.Transform(position, prefabWorld);
                                            }
                                            if (floor.Name == "rot")
                                            {
                                                rotation.X = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                rotation.Y = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                rotation.Z = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                                rotation += prefRot;
                                            }
                                            if (floor.Name == "coll_bounds")
                                            {
                                                dims.X = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                dims.Y = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                dims.Z = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                            }
                                            if (floor.Name == "uses")
                                            {
                                                uses = int.Parse(floor.FirstChild.InnerText);
                                            }
                                            if (floor.Name == "target_id")
                                            {
                                                targetID = int.Parse(floor.FirstChild.InnerText);
                                            }
                                        }
                                        activatorToAdd.Add(new Level.Activator(path, filename, position, rotation, gpu, dims, targetID, uses));
                                        continue;
                                    #endregion
                                    case "spawner_trigger_box":
                                        #region Load Spawn Triggers
                                        foreach (XmlNode floor in child.ChildNodes)
                                        {
                                            if (floor.Name == "pos")
                                            {
                                                position.X = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                position.Y = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                position.Z = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                                position = Vector3.Transform(position, prefabWorld);
                                            }
                                            if (floor.Name == "rot")
                                            {
                                                rotation.X = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                rotation.Y = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                rotation.Z = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                                rotation += prefRot;
                                            }
                                            if (floor.Name == "coll_bounds")
                                            {
                                                dims.X = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                dims.Y = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                dims.Z = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                            }
                                            if (floor.Name == "uses")
                                            {
                                                uses = int.Parse(floor.FirstChild.InnerText);
                                            }
                                            if (floor.Name == "target_id")
                                            {
                                                targetID = int.Parse(floor.FirstChild.InnerText);
                                            }
                                        }
                                        spawntriggersToAdd.Add(new Level.SpawnerTriggerBox(position, rotation, gpu, dims, targetID, uses));
                                        continue;
                                    #endregion
                                    case "mission_trigger_box":
                                        #region Load Mission Trigger Boxes
                                        foreach (XmlNode floor in child.ChildNodes)
                                        {
                                            if (floor.Name == "pos")
                                            {
                                                position.X = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                position.Y = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                position.Z = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                                position = Vector3.Transform(position, prefabWorld);
                                            }
                                            if (floor.Name == "rot")
                                            {
                                                rotation.X = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                rotation.Y = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                rotation.Z = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                                rotation += prefRot;
                                            }
                                            if (floor.Name == "coll_bounds")
                                            {
                                                dims.X = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                                dims.Y = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                                dims.Z = float.Parse(floor.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                            }
                                            if (floor.Name == "uses")
                                            {
                                                uses = int.Parse(floor.FirstChild.InnerText);
                                            }
                                            if (floor.Name == "target_id")
                                            {
                                                targetID = int.Parse(floor.FirstChild.InnerText);
                                            }
                                        }
                                        missionTriggersToAdd.Add(new Level.MissionTriggerBox(position, rotation, gpu, dims, targetID, uses));
                                        continue;
                                        #endregion
                                }
                            }
                        }
                        break;
                    case "level_data":
                        int levId = 0;
                        Vector4 fogCol = Vector4.Zero;
                        int fogDis = 0;
                        Vector3 pSpawn = Vector3.Zero;
                        foreach (XmlNode data in room.ChildNodes)
                        {
                            switch (data.Name)
                            {
                                case "level_id":
                                    levId = int.Parse(data.FirstChild.InnerText);
                                    break;
                                case "fog_color":
                                    fogCol.X = float.Parse(data.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                    fogCol.Y = float.Parse(data.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                    fogCol.Z = float.Parse(data.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                    fogCol.W = float.Parse(data.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[3]);
                                    break;
                                case "fog_range":
                                    fogDis = int.Parse(data.FirstChild.InnerText);
                                    break;
                                case "player_spawn":
                                    pSpawn.X = float.Parse(data.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[0]);
                                    pSpawn.Y = float.Parse(data.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[1]);
                                    pSpawn.Z = float.Parse(data.FirstChild.InnerText.Trim('(').Trim(')').Split(',')[2]);
                                    break;
                            }
                        }
                        outLevel.SetData(levId, fogCol, fogDis, pSpawn);
                        break;
                }
            }
            //Add components to level
            if (floorsToAdd.Count != 0)          outLevel.Add(floorsToAdd);
            if (staticColMeshToAdd.Count !=0)    outLevel.Add(staticColMeshToAdd);
            if (spawnersToAdd.Count != 0)        outLevel.Add(spawnersToAdd);
            if (staticMeshToAdd.Count != 0)      outLevel.Add(staticMeshToAdd);
            if (deathboxToAdd.Count != 0)        outLevel.Add(deathboxToAdd);
            if (doorsToAdd.Count != 0)           outLevel.Add(doorsToAdd);
            if (activatorToAdd.Count != 0)       outLevel.Add(activatorToAdd);
            if (spawntriggersToAdd.Count != 0)   outLevel.Add(spawntriggersToAdd);
            if (missionTriggersToAdd.Count != 0) outLevel.Add(missionTriggersToAdd);

            return outLevel;
        }
    }
}