//#define USING_COLORED_VERTICES  // uncomment this in both SkinModel and SkinModelLoader if using
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

// ASSIMP INSTRUCTIONS:
// AssimpNET is (cross platform) .NET wrapper for Open Asset Import Library 
// Add the AssimpNET nuget to your solution:
// - in the solution explorer, right click on the project
// - select manage nuget packages
// - click browse
// - type in assimpNET and install it to the solution and project via the checkbox on the right.

/// THIS IS BASED ON WORK BY:  WIL MOTIL (a modified slightly older version)
/// https://github.com/willmotil/MonoGameUtilityClasses

namespace TheDivineAdventure.SkinModels
{
    // CLASS SKINMODEL 
    public class SkinModel
    {
        // SETTINGS
        public const bool WARN_MISSING_DIFFUSE_TEX = false;

        #region MEMBERS     
        GraphicsDevice gpu;
        SkinFx skinFx;                // using to control SkinEffect             
        public Texture2D debug_tex;
        public Texture2D[] animated_tex;
        public int animated_tex_frame = -1;
        public bool use_debug_tex;
        public int max_bones = 180;
        public Matrix[] skinShaderMatrices;    // these are the real final bone matrices they end up on the shader
        public SkinMesh[] meshes;
        public ModelNode rootNodeOfTree;        // actual model root node - base node of the model - from here we can locate any node in the chain

        // animations
        public List<RigAnimation> animations = new List<RigAnimation>();
        int currentAnim;
        public int currentFrame;
        public bool animationRunning;
        public bool loopAnimation = true;
        float timeStart;
        public float currentAnimFrameTime;
        public float overrideAnimFrameTime = -1;  // mainly for testing to step thru each frame
        public float animationOffset;
        public float playSpeed = 1;
        #endregion


        #region CONSTRUCTOR AND METHODS      
        //----------------------
        // CONSTRUCTOR
        //----------------------
        public SkinModel(GraphicsDevice GPU, SkinFx skin_effect)
        {
            gpu = GPU;
            skinFx = skin_effect;
            skinShaderMatrices = new Matrix[max_bones];
            animated_tex = new Texture2D[1];
            ResetShaderMatrices();
        }
        //------------------------------------------
        // RESET SHADER MATRICES 
        //------------------------------------------
        public void ResetShaderMatrices()
        {
            for (int i = 0; i < max_bones; i++)
            {
                skinShaderMatrices[i] = Matrix.Identity;
            }
        }

        //--------------------
        // SET EFFECT
        //--------------------
        public void SetEffect(SkinFx skinFX_)
        {
            skinFx = skinFX_;
        }
        #endregion // constructor & methods


        #region UPDATES (animating)
        //------------
        // UPDATE
        //------------
        public void Update(GameTime gameTime)
        {
            if (animationRunning) UpdateModelAnimations(gameTime); // determine local transforms for animation
            UpdateTexAnims();                                      // update testure animations if they exist
            UpdateNodes(rootNodeOfTree);                           // update the skeleton 
            UpdateMeshAnims();                                     // update any regular mesh animations
        }

        public void UpdateBlendAnim(SkinModel[] adds,int id1, int id2, float baseWeight, float[] boneWeights)
        {
            int[] ids = {id1, id2};
            BlendAnimations(adds, ids, baseWeight, boneWeights);  // determine local transforms for animation
            UpdateNodes(rootNodeOfTree);          // update the skeleton 
            UpdateMeshAnims();                    // update any regular mesh animations
        }


        //----------------------------------------------
        // UPDATE MODEL ANIMATIONS
        //----------------------------------------------
        ///<summary> Gets the animation frame (based on elapsed time) for all nodes and loads them into the model node transforms. </summary>
        private void UpdateModelAnimations(GameTime gameTime)
        {
            if (animations.Count <= 0 || currentAnim >= animations.Count) return;

            AnimationTimeLogic(gameTime);                                      // process what to do based on animation time (frames, duration, complete | looping)

            int cnt = animations[currentAnim].animatedNodes.Count;             // loop thru animated nodes
            for (int n = 0; n < cnt;  n++)
            {
                AnimNodes animNode = animations[currentAnim].animatedNodes[n]; // get animation keyframe lists (each animNode)
                ModelNode node = animNode.nodeRef;                         // get bone associated with this animNode (could be mesh-node) 
                node.local_mtx = animations[currentAnim].Interpolate(currentAnimFrameTime, animNode); // interpolate keyframes (animate local matrix) for this bone
                //-------Get node ids for bone mapping
                //ModelNode test = animNode.nodeRef;
                //Debug.WriteLine(n + "," + test.name);
                //Debug.WriteLine(" ");
            }
        }

        //get animation info
        public void getInfo()
        {
            if (animations.Count <= 0 || currentAnim >= animations.Count) return;

            int cnt = animations[currentAnim].animatedNodes.Count;             // loop thru animated nodes
            for (int n = 0; n < cnt; n++)
            {
                AnimNodes animNode = animations[currentAnim].animatedNodes[n]; // get animation keyframe lists (each animNode)
                ModelNode node = animNode.nodeRef;                         // get bone associated with this animNode (could be mesh-node) 
                node.local_mtx = animations[currentAnim].Interpolate(currentAnimFrameTime, animNode); // interpolate keyframes (animate local matrix) for this bone
                //-------Get node ids for bone mapping
                ModelNode test = animNode.nodeRef;
                Debug.WriteLine(n + "," + test.name + ", " + test.local_mtx);
                Debug.WriteLine("--------------");
            }
        }


        //get target bone to animate
        private ModelNode FindBone(ModelNode thisBone, string boneName)
        {
            if (thisBone.name == boneName) return thisBone;
            if (thisBone.children.Count == 0) return null;
            foreach (ModelNode child in thisBone.children)
            {
                if (FindBone(child, boneName) != null) return FindBone(child, boneName);
            }
            return null;
        }
        //Blend animations
        private void BlendAnimations(SkinModel[] adds, int[] animID, float baseWeight, float[] boneWeights)
        {
            if (animations.Count <= 0) return;
            int cnt = animations[currentAnim].animatedNodes.Count;             // loop thru animated nodes

            //blended matrices
            for (int n = 0; n < cnt; n++)
            {
                AnimNodes base_ = adds[0].animations[currentAnim].animatedNodes[n]; // get animation keyframe lists from base
                AnimNodes anim1 = adds[animID[0]].animations[currentAnim].animatedNodes[n]; // get animation keyframe lists from base
                AnimNodes anim2 = adds[animID[1]].animations[currentAnim].animatedNodes[n]; // get animation keyframe lists from anim1
                ModelNode node0 = base_.nodeRef;                         // get bone associated with this animNode
                ModelNode node1 = anim1.nodeRef;                         // get bone associated with this animNode
                ModelNode node2 = anim2.nodeRef;                         // get bone associated with this animNode

                float blendWeight = baseWeight * boneWeights[n];          //Get Total Blend Percent
                if (blendWeight > 1) blendWeight = 1;                     //cap blendweights at 1 to not over animate
                //blend animations
                node0.local_mtx = Matrix.Lerp(node1.local_mtx, node2.local_mtx, blendWeight);
            }
        }

        //----------------------------------------
        // ANIMATION TIME LOGIC 
        //----------------------------------------
        public void AnimationTimeLogic(GameTime gameTime)
        {

            currentAnimFrameTime = ((float)(gameTime.TotalGameTime.TotalSeconds) - timeStart+(animationOffset/10))*playSpeed; // *.1f; // if we want to purposely slow it for testing
            float animTotalDuration = (float)animations[currentAnim].DurationInSeconds + (float)animations[currentAnim].DurationInSecondsAdded; // add extra for looping

            // if we need to see a single frame; let us override the current frame
            if (overrideAnimFrameTime >= 0f)
            {
                currentAnimFrameTime = overrideAnimFrameTime;
                if (overrideAnimFrameTime > animTotalDuration) overrideAnimFrameTime = 0f;
            }
            // Animation time exceeds total duration.
            if (currentAnimFrameTime > animTotalDuration)
            {
                if (loopAnimation)
                { // LOOP ANIMATION                
                    currentAnimFrameTime = currentAnimFrameTime - animTotalDuration; // loop back to start
                    timeStart = (float)(gameTime.TotalGameTime.TotalSeconds);        // reset startTime
                }
                else
                {// ANIMATION COMPLETE                
                    currentFrame = 0;  // assuming we might want to restart the animation later (from 0) 
                    timeStart = 0;
                    animationRunning = false;
                }
            }
        }


        //------------------------
        // UPDATE NODES
        //------------------------
        /// <summary> Updates the skeleton (combined) after updating the local animated transforms </summary>
        private void UpdateNodes(ModelNode node)
        {
            // if there's a parent, we can add the local bone onto it to get the resulting bone location in skeleton:
            if (node.parent != null) node.combined_mtx = node.local_mtx * node.parent.combined_mtx;
            else node.combined_mtx = node.local_mtx;  // no parent so just provide the local matrix transform

            // loop thru the flat-list of bones for this node (bone could effect more than 1 mesh):
            for (int i = 0; i < node.uniqueMeshBones.Count; i++)
            {
                ModelBone bn = node.uniqueMeshBones[i];                // refers to the bone in uniqueMeshBones list (holds mesh#, bone#, etc)
                meshes[bn.meshIndex].shader_matrices[bn.boneIndex] = bn.offset_mtx * node.combined_mtx; // converts resulting vert transforms back to bind-pose-relative space
            }
            foreach (ModelNode n in node.children) UpdateNodes(n);     // do same for children
        }


        //----------------------------------
        // UPDATE MESH ANIMS
        //----------------------------------
        /// In draw, this should enable us to call on this in relation to the world transform.
        private void UpdateMeshAnims()
        {
            if (animations.Count <= 0) return;
            for (int i = 0; i < meshes.Length; i++)
            {                                // try to handle when we just have mesh transforms                                                      
                if (animations[currentAnim].animatedNodes.Count > 0)
                { // clear out the combined transforms
                    meshes[i].node_with_anim_trans.combined_mtx = Matrix.Identity;
                }
            }
        }

        //----------------------------------
        // UPDATE TEX ANIMS
        //----------------------------------
        /// In draw, this should enable us to call on this in relation to the world transform.
        private void UpdateTexAnims()
        {
            if (animated_tex.Length <= 0 || animated_tex_frame ==-1) return;
            animated_tex_frame++;
            if (animated_tex_frame >= animated_tex.Length) animated_tex_frame = 0;
        }
        #endregion // updates


        #region ANIMATION CONTROLS
        // CURRENT ANIMATION INDEX
        public int CurrentAnimationIndex
        {
            get { return currentAnim; }
            set
            {
                var n = value;
                if (n >= animations.Count) n = 0;
                if (n < 0) n += animations.Count;
                currentAnim = n;
            }
        }

        // BEGIN ANIMATION
        public void BeginAnimation(int animationIndex, GameTime gametime, float speedMod = 1)
        {
            timeStart = (float)gametime.TotalGameTime.TotalSeconds;  // capture the start time
            currentAnim = animationIndex;                              // set current animation
            animationRunning = true;
            playSpeed = speedMod;
        }

        // STOP ANIMATION
        public void StopAnimation()
        {
            animationRunning = false;
        }

        // BEGIN TEXTURE ANIMATION
        public void BeginTexAnimation(int animationIndex)
        {
            animated_tex_frame = animationIndex;
        }

        // STOP TEXTURE ANIMATION
        public void StopTexAnimation()
        {
            animated_tex_frame = -1;
        }
        #endregion // Animation Stuff


        #region DRAWS
        //--------------------------------
        // ASSIGN MATERIALS
        //--------------------------------
        private void AssignMaterials(SkinMesh m, bool use_material_spec)
        {
            skinFx.ambientCol.X = m.ambient.X; skinFx.ambientCol.Y = m.ambient.Y; skinFx.ambientCol.Z = m.ambient.Z;  //Vec4 to Vec3
            skinFx.emissiveCol.X = m.emissive.X; skinFx.emissiveCol.Y = m.emissive.Y; skinFx.emissiveCol.Z = m.emissive.Z; //Vec4 to Vec3
            skinFx.diffuseCol = m.diffuse;
            if (use_material_spec)
            {
                skinFx.specularCol.X = m.specular.X; skinFx.specularCol.Y = m.specular.Y; skinFx.specularCol.Z = m.specular.Z;
                skinFx.specularPow = m.shineStrength; // I think...                   
            }
        }


        //---------------------------------------------------
        // DRAW MESH (by index)
        //---------------------------------------------------
        /// <summary> Draws the mesh by the index. </summary>
        public void DrawMesh(int meshIndex, Camera cam, Matrix world, bool use_mesh_materials = true, bool use_material_spec = true)
        {
            var m = meshes[meshIndex];
            skinFx.fx.Parameters["Bones"].SetValue(m.shader_matrices);     // provide the bone matrices of this mesh
            if (use_mesh_materials)
                AssignMaterials(m, use_material_spec);
            skinFx.world = m.node_with_anim_trans.combined_mtx * world;    // set model's world transform

            // DO LAST (this will apply the technique with any parameters set before it (or in it)): 
            if(animated_tex[0] != null) skinFx.SetDrawParams(cam, animated_tex[animated_tex_frame], m.tex_normalMap, m.tex_specular);
            else skinFx.SetDrawParams(cam, m.tex_diffuse, m.tex_normalMap, m.tex_specular);
            gpu.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, m.vertices, 0, m.vertices.Length, m.indices, 0, m.indices.Length / 3, VertexNormMapSkin.VertexDeclaration);
        }


        // MESH DEBUG DRAW 
        /// <summary> Sets the global final bone matrices to the shader and draws it. </summary>
        public void MeshDebugDraw(GraphicsDevice gd, Matrix world, int meshIdToShow)
        {
            skinFx.fx.Parameters["Bones"].SetValue(skinShaderMatrices);
            for (int i = 0; i < meshes.Length; i++)
            {
                var m = meshes[i];
                AssignMaterials(m, true);
                if (i == meshIdToShow)
                {
                    skinFx.fx.Parameters["World"].SetValue(world * m.CombinedFinalTransform);
                    skinFx.fx.CurrentTechnique.Passes[0].Apply();
                    gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, m.vertices, 0, m.vertices.Length, m.indices, 0, m.indices.Length / 3, VertexNormMapSkin.VertexDeclaration);
                }
            }
        }
        
        #endregion // draws


        #region ---Getting Socket Info --
        public Matrix GetBoneTransform(int targetNode)
        {
            int cnt = animations[currentAnim].animatedNodes.Count;             // loop thru animated nodes
            if (targetNode > cnt) throw new ArgumentException("target node is outside target animations range");    //make sure target node is good
            AnimNodes base_ = animations[currentAnim].animatedNodes[targetNode]; // get animation keyframe lists from base (each animNode)
            ModelNode node = base_.nodeRef;                         // get bone associated with this animNode (could be mesh-node)
            return node.combined_mtx;
        }
        #endregion


        #region NESTED CLASSES -------------------------------------------------------------------------------------------------------------------------------------

        // CLASS MODEL BONE 
        public class ModelBone
        {
            public string name;                  // bone name
            public int meshIndex = -1;        // which mesh? 
            public int boneIndex = -1;        // which bone? 
            public int numWeightedVerts = 0;  // number of weighted verts that use this bone for influence
            public Matrix InvOffset_mtx { get { return Matrix.Invert(offset_mtx); } } // inverse bind transforms (need vectors at model origin(0,0,0) when doing animation [add to parent's final transform after])
            public Matrix offset_mtx;            // bind-pose transforms
        } // Model Bone Class



        // CLASS MODEL NODE 
        // a transform - some are bones(joints) - some not (part of tree). Each could link to more than 1 mesh and so have more than 1 offset.    
        public class ModelNode
        {
            public string name;                                // mesh or bone name (whichever it is)
            public ModelNode parent;                              // parent node       (usually parent bone) 
            public List<ModelNode> children = new List<ModelNode>();    // child tree

            public bool hasRealBone, isBoneOnRoute, isMeshNode; // used for debugging:       

            // Each mesh has a list of shader-matrices - this keeps track of which meshes these bones apply to (and the bone index)
            public List<ModelBone> uniqueMeshBones = new List<ModelBone>();  // points to mesh & bone that corresponds to this node bone.

            public Matrix local_mtx;     // transform relative to parent         
            public Matrix combined_mtx;  // tree-accumulated transforms  (global-space transform for shader to use - skin matrix)
        } // Model Node Class



        // CLASS SKIN MESH 
        public class SkinMesh
        {
            public ModelNode node_with_anim_trans;  // reference of node containing animated transform
            public string Name = "";
            public int meshNumber;
            public bool hasBones;
            public bool hasMeshAnimAttachments = false;
            public string tex_name;
            public string tex_normMap_name;
            public string tex_heightMap_name;
            public string tex_reflectionMap_name;
            public string tex_specular_name;
            public Texture2D tex_diffuse;
            public Texture2D tex_specular;
            public Texture2D tex_normalMap;
            public Texture2D tex_heightMap;
            public Texture2D tex_reflectionMap;
            //public Texture2D tex_lightMap, tex_ambientOcclusion;     // maybe these 2 are better baked into tex_diffuse?            
            public VertexNormMapSkin[] vertices;
            public int[] indices;
            public ModelBone[] meshBones;
            public Matrix[] shader_matrices;

            public int material_index;
            public string material_name;               // (for this index)
            public Matrix CombinedFinalTransform { get { return node_with_anim_trans.combined_mtx; } }  // Final world position of mesh itself
            public Vector3 min, max, mid;

            // MESH MATERIAL:
            public Vector4 ambient = Vector4.One;   // minimum light color
            public Vector4 diffuse = Vector4.One;   // regular material colorization
            public Vector4 specular = Vector4.One;   // specular highlight color 
            public Vector4 emissive = Vector4.One;   // amplify a color brightness (not requiring light - similar to ambient really - kind of a glow without light)                
            public float opacity = 1.0f;     // how opaque or see-through is it?          
            public float reflectivity = 0.0f;     // strength of reflections
            public float shininess = 0.0f;     // how much light shines off
            public float shineStrength = 1.0f;     // probably specular power (can use to narrow & intensifies highlights - ie: more wet or metallic looking)
            public float bumpScale = 0.0f;     // amplify or reduce normal-map effect  
            public bool isTwoSided = false;    // useful for glass and ice
        } // Skin Mesh Class



        // CLASS RIG ANIMATION 
        public class RigAnimation
        {
            public string animation_name = "";
            public double DurationInTicks;        // how many ticks for whole animation
            public double DurationInSeconds;      // same in seconds
            public double DurationInSecondsAdded; // added seconds
            public double TicksPerSecond;         // ticks/sec (play speed)

            public bool HasMeshAnims;           // contains mesh transform animations (usually no) 
            public bool HasNodeAnims;           // any node-based animations? 
            public List<AnimNodes> animatedNodes; // holds the animated nodes

            // INTERPOLATE
            ///<summary> animation blending between key-frames </summary>
            public Matrix Interpolate(double animTime, AnimNodes nodeAnim)
            {
                var durationSecs = DurationInSeconds + DurationInSecondsAdded;

                while (animTime > durationSecs)        // If the requested play-time is past the end of the animation, loop it (ie: time = 20 but duration = 16 so time becomes 4)
                    animTime -= durationSecs;
                Quaternion q1 = nodeAnim.qrot[0], q2 = q1;   // init rot as entry 0 for both keys (init may be needed cuz conditional value assignment can upset compiler)
                Vector3 p1 = nodeAnim.position[0], p2 = p1;   // " pos
                Vector3 s1 = nodeAnim.scale[0], s2 = s1;   // " scale
                double tq1 = nodeAnim.qrotTime[0], tq2 = tq1; // " rot-time
                double tp1 = nodeAnim.positionTime[0], tp2 = tp1; // " pos-time
                double ts1 = nodeAnim.scaleTime[0], ts2 = ts1; // " scale-time

                // GET ROTATION KEYFRAMES
                int end_t_index = nodeAnim.qrotTime.Count - 1;      // final time's index (starting with qrot cuz we do it first - we'll cahnge this variable for pos and scale)
                int end_index = nodeAnim.qrot.Count - 1;          // final rot frame
                var end_time = nodeAnim.qrotTime[end_t_index];   // get final rotation-time
                if (animTime > end_time)
                {                          // if animTime is past final rotation-time: Set to interpolate between last and first frames (for animation-loop)
                    tq1 = end_time;                             // key 1 time is last keyframe and time 2 is time taken after to get to frame 0 (see below) 
                    tq2 += durationSecs;                         // total duration accounting for time to loop from last frame to frame 0 (with DurationInSecondsAdded)
                    q1 = nodeAnim.qrot[end_index];             // get final quaternion (count-1),       NOTE: q2 already set above (key frame 0)                                                                      
                }
                else
                {
                    int frame2 = end_index, frame1;              //                  animTime   t =  frame2
                    for (; frame2 > -1; frame2--)
                    {              // loop from last index to 0 (until find correct place on timeline):
                        var t = nodeAnim.qrotTime[frame2];       // what's the time at this frame?
                        if (t < animTime) break;                 // if the current_time > the frame time then we've found the spot we're looking for (break out)                                                    
                    }
                    if (frame2 < end_index) frame2++;            // at this point the frame2 is 1 less than what we're looking for so add 1
                    q2 = nodeAnim.qrot[frame2];
                    tq2 = nodeAnim.qrotTime[frame2];
                    frame1 = frame2 - 1;
                    if (frame1 < 0)
                    {
                        frame1 = end_index;                             // loop frame1 to last frame
                        tq1 = nodeAnim.qrotTime[frame1] - durationSecs; // Using: frame2time - frame1time, so we need time1 to be less _ thus: subtract durationSecs to fix it
                    }
                    else tq1 = nodeAnim.qrotTime[frame1];               // get time1 
                    q1 = nodeAnim.qrot[frame1];
                }
                // GET POSITION KEY FRAMES
                end_t_index = nodeAnim.positionTime.Count - 1;      // final time's index
                end_index = nodeAnim.position.Count - 1;          // final pos frame
                end_time = nodeAnim.positionTime[end_t_index];   // get final position-time
                if (animTime > end_time)
                {                          // if animTime is past final pos-time: Set to interpolate between last and first frames (for animation-loop)
                    tp1 = end_time;                                // key 1 time is last keyframe and time 2 is time taken after to get to frame 0 (see below) 
                    tp2 += durationSecs;                            // total duration accounting for time to loop from last frame to frame 0 (with DurationInSecondsAdded)
                    p1 = nodeAnim.position[end_index];            // get final position (count-1),       NOTE: q2 already set above (key frame 0)                                                                      
                }
                else
                {
                    int frame2 = end_index, frame1;
                    for (; frame2 > -1; frame2--)
                    {                 // loop from last index to 0 (until find correct place on timeline):
                        var t = nodeAnim.positionTime[frame2];      // what's the time at this frame?
                        if (t < animTime) break;                    // if the current_time > the frame time then we've found the spot we're looking for (break out)                                                    
                    }
                    if (frame2 < end_index) frame2++;               // at this point the frame2 is 1 less than what we're looking for so add 1
                    p2 = nodeAnim.position[frame2];
                    tp2 = nodeAnim.positionTime[frame2];
                    frame1 = frame2 - 1;
                    if (frame1 < 0)
                    {
                        frame1 = end_index;                                 // loop frame1 to last frame
                        tp1 = nodeAnim.positionTime[frame1] - durationSecs; // Using: frame2time - frame1time, so we need time1 to be less _ thus: subtract durationSecs to fix it
                    }
                    else tp1 = nodeAnim.positionTime[frame1];               // get time1 
                    p1 = nodeAnim.position[frame1];
                }
                // GET SCALE KEYFRAMES 
                end_t_index = nodeAnim.scaleTime.Count - 1;         // final time's index
                end_index = nodeAnim.scale.Count - 1;             // final scale frame
                end_time = nodeAnim.scaleTime[end_t_index];      // get final scale-time
                if (animTime > end_time)
                {                          // if animTime is past final scale-time: Set to interpolate between last and first frames (for animation-loop)
                    ts1 = end_time;                                // key 1 time is last keyframe and time 2 is time taken after to get to frame 0 (see below) 
                    ts2 += durationSecs;                            // total duration accounting for time to loop from last frame to frame 0 (with DurationInSecondsAdded)
                    s1 = nodeAnim.scale[end_index];               // get final scale (count-1),       NOTE: q2 already set above (key frame 0)                                                                      
                }
                else
                {
                    int frame2 = end_index, frame1;
                    for (; frame2 > -1; frame2--)
                    {                 // loop from last index to 0 (until find correct place on timeline):
                        var t = nodeAnim.scaleTime[frame2];         // what's the time at this frame?
                        if (animTime > t) break;                    // if the current_time > the frame time then we've found the spot we're looking for (break out)                                                    
                    }
                    if (frame2 < end_index) frame2++;               // at this point the frame2 is 1 less than what we're looking for so add 1
                    s2 = nodeAnim.scale[frame2];
                    ts2 = nodeAnim.scaleTime[frame2];
                    frame1 = frame2 - 1;
                    if (frame1 < 0)
                    {
                        frame1 = end_index;                               // loop frame1 to last frame
                        ts1 = nodeAnim.scaleTime[frame1] - durationSecs;  // Using: frame2time - frame1time, so we need time1 to be less _ thus: subtract durationSecs to fix it
                    }
                    else ts1 = nodeAnim.scaleTime[frame1];                // get time1 
                    s1 = nodeAnim.scale[frame1];
                }

                float tqi = 0, tpi = 0, tsi = 0;

                Quaternion q;
                tqi = (float)GetInterpolateTimePercent(tq1, tq2, animTime); // get the time% (0-1)
                q = Quaternion.Slerp(q1, q2, tqi);                          // blend the rotation between keys using the time percent

                Vector3 p;
                tpi = (float)GetInterpolateTimePercent(tp1, tp2, animTime); // "
                p = Vector3.Lerp(p1, p2, tpi);

                Vector3 s;
                tsi = (float)GetInterpolateTimePercent(ts1, ts2, animTime); // "
                s = Vector3.Lerp(s1, s2, tsi);

                var ms = Matrix.CreateScale(s);
                var mr = Matrix.CreateFromQuaternion(q);
                var mt = Matrix.CreateTranslation(p);

                var m = ms * mr * mt; // S,R,T
                return m;
            } // Interpolate


            // GET INTERPOLATE TIME PERCENT
            public double GetInterpolateTimePercent(double s, double e, double val)
            {
                if (val < s || val > e)
                    throw new Exception(this.ToString()+".cs RigAnimation GetInterpolateTimePercent :  Value " + val + " passed to the method must be within the start and end time. ");
                if (s == e) throw new Exception("SkinModel.cs RigAnimation GetInterpolateTimePercent :  e - s :  " + e + "-" + s + "=0  - Divide by zero error.");
                return (val - s) / (e - s);
            }

        } // rig animation class




        // CLASS ANIM NODES 
        /// <summary> Nodes contain animation frames. Initial trans are copied from assimp - then interpolated frame sets are built. (keeping original S,R,T if want to later edit) </summary>
        public class AnimNodes
        {
            public ModelNode nodeRef;
            public string nodeName = "";

            // in model tick time
            public List<double> positionTime = new List<double>();
            public List<double> scaleTime = new List<double>();
            public List<double> qrotTime = new List<double>();
            public List<Vector3> position = new List<Vector3>();
            public List<Vector3> scale = new List<Vector3>();
            public List<Quaternion> qrot = new List<Quaternion>();
        } // Anim Nodes Class
        #endregion

    } // Skin Model Class

    //--------------------------------------
    #region VERTEX  STRUCTURE 
    public struct VertexNormMapSkin : IVertexType
    {
        public Vector3 pos, norm;
        public Vector2 uv;
        public Vector3 tangent;
        public Vector3 biTangent;
        public Vector4 blendIndices, blendWeights;

        public static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
              new VertexElement(BYT.Ini(3), VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
              new VertexElement(BYT.Off(3), VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
              new VertexElement(BYT.Off(2), VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
              new VertexElement(BYT.Off(3), VertexElementFormat.Vector3, VertexElementUsage.Normal, 1),
              new VertexElement(BYT.Off(3), VertexElementFormat.Vector3, VertexElementUsage.Normal, 2),
              new VertexElement(BYT.Off(4), VertexElementFormat.Vector4, VertexElementUsage.BlendIndices, 0),
              new VertexElement(BYT.Off(4), VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0)
        );
        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    }
    // BOFF (adjusts byte offset for each entry in a vertex declaration)
    public struct BYT
    {
        public static int byt = 0;
        public static int Ini(int b_size) { b_size *= 4; byt = 0; byt += b_size; return 0; }
        public static int Off(int b_size) { b_size *= 4; byt += b_size; return byt - b_size; }
    }
    #endregion

}
