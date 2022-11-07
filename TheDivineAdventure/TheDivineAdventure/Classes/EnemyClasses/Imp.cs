using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using TheDivineAdventure.SkinModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TheDivineAdventure
{
    class Imp : Enemy
    {

        //Constructor ----------
        public Imp(List<SoundEffect> s, string role_, Vector3 spawnLoc, PlayScene parent, SkinModel model, ContentManager content) : base(s, role_, spawnLoc, parent, model, content)
        {
            //Capsule Collider
            boundingCollider = new CapsuleCollider(28, 8, pos, rot, parent.gpu, Color.White);
            animWeights = new float[100];

            health = 75;

            vel = 0;
            speedMax = 0.88f;
            runMod = 1.1f;
            accel = .015f;
            prefDistance = 10;

            attackRange = 20;
            attackSpeed = 70;
            attackDamage = 15f;
            attackLength = 20f;
            maxAttackRange = 250f;

            animations[IDLE] = loader.Load("MOD_Imp/ANIM_Imp_Idle.fbx", "MOD_Imp", true, 4, skinFx, rescale: 3f);
            animations[WALK] = loader.Load("MOD_Imp/ANIM_Imp_Walk.fbx", "MOD_Imp", true, 4, skinFx, rescale: 3f);
            animations[RUN] = loader.Load("MOD_Imp/ANIM_Imp_Run.fbx", "MOD_Imp", true, 4, skinFx, rescale: 3f);
            animations[ATTACKR] = loader.Load("MOD_Imp/ANIM_Imp_Attack_Ranged.fbx", "MOD_Imp", true, 4, skinFx, rescale: 3f);
            animations[ATTACKR].loopAnimation = false;
            animations[ATTACKM] = loader.Load("MOD_Imp/ANIM_Imp_Attack_Melee.fbx", "MOD_Imp", true, 4, skinFx, rescale: 3f);
            animations[ATTACKM].loopAnimation = false;
        }

        //Update Function
        public override void Update(float dt, GameTime gametime, Camera cam)
        {
            base.Update(dt, gametime, cam);
        }

        //AI Processing
        protected override void AIProcessing(Camera cam)
        {
            if (!isAttacking)
            {
                FacePlayer();
                Move();
            }
            if (attackTimer <= attackSpeed) { attackTimer += 1f; return; }
            if (Vector3.Distance(player.Pos, world.Translation) < attackRange)
            {
                Attack(true, cam);
            }
            else if (Vector3.Distance(player.Pos, world.Translation + (Vector3.One * player.width)) > prefDistance && Vector3.Distance(player.Pos, world.Translation) < maxAttackRange)
            {
                Attack(false, cam);
            }
        }


        //Attack Function
        protected override void Attack(bool isMelee, Camera cam)
        {
            isAttacking = true;
            if (isMelee)
            {
                isMeleeAttacking = true;
                if (attDurTimer < attackLength) { attDurTimer += 1; return; }
                //perform attack
                soundEffects[0].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                AttackPattern.singleMel(world.Translation + world.Backward * 5, world.Backward * 2, rot, attackDamage, projList, cam);

                //process projectile
                foreach (Attack p in projList)
                {
                    if (p.TimeToDestroy)
                    {
                        projList.Remove(p);
                        break;
                    }
                }

            }
            else
            {
                isMeleeAttacking = false;
                if (attDurTimer < attackLength) { attDurTimer += 1; return; }
                soundEffects[0].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                AttackPattern.ImpProj(world.Translation + world.Up * 18 + world.Left * 10, player.Pos, attackDamage / 2, projList, cam);
            }
            base.Attack(isMelee, cam);
        }

        //Blend Animations
        protected override void BlendAnims(GameTime gameTime)
        {
            //reset weights for blending
            SetAnimBlends(1);
            float percent;
            //walk/run blending
            percent = vel / speedMax;
            animations[BASE].UpdateBlendAnim(animations, BASE, WALK, percent, animWeights);   //idle -> walk

            percent = (vel / runMod) / speedMax * runMod;
            animations[BASE].UpdateBlendAnim(animations, BASE, RUN, percent, animWeights);   //walk -> run

            //Main Attack
            if (isAttacking && !animations[ATTACKM].animationRunning) animations[ATTACKM].BeginAnimation(0, gameTime); //start animation
            if (animations[ATTACKM].animationRunning)
            {
                animations[ATTACKM].Update(gameTime);
                percent = 1;
                animations[BASE].UpdateBlendAnim(animations, BASE, ATTACKM, percent, animWeights);   //base -> Main Attack
                if (animations[ATTACKM].currentAnimFrameTime > 0.3)
                {
                    isAttacking = false; //end attack
                    animations[BASE].BeginAnimation(0, gameTime);
                    animations[WALK].BeginAnimation(0, gameTime);
                    animations[RUN].BeginAnimation(0, gameTime);
                }
            }
            //walk/run blending
            percent = vel / speedMax;
            animations[BASE].UpdateBlendAnim(animations, BASE, WALK, percent, animWeights);   //idle -> walk

            percent = (vel / runMod) / speedMax * runMod;
            animations[BASE].UpdateBlendAnim(animations, BASE, RUN, percent, animWeights);   //walk -> run

            //Main Attack

            if (isAttacking && !animations[ATTACKM].animationRunning && isMeleeAttacking) animations[ATTACKM].BeginAnimation(0, gameTime); //start animation
            else if (isAttacking && !animations[ATTACKR].animationRunning && !isMeleeAttacking) animations[ATTACKR].BeginAnimation(0, gameTime); //start animation

            if (animations[ATTACKM].animationRunning)
            {
                animations[ATTACKM].Update(gameTime);

                //interpolate mixing animations
                percent = 1;
                if (animations[ATTACKM].currentAnimFrameTime > 0.4) percent = 1 - (animations[ATTACKM].currentAnimFrameTime - 0.4f) / 0.6f;

                animations[BASE].UpdateBlendAnim(animations, BASE, ATTACKM, percent, animWeights);   //base -> Main Attack
                if (animations[ATTACKM].currentAnimFrameTime > 0.7)
                {
                    isAttacking = false; //end attack
                }
            }

            if (animations[ATTACKR].animationRunning)
            {
                animations[ATTACKR].Update(gameTime);

                //interpolate mixing animations
                if (animations[ATTACKR].currentAnimFrameTime > 0.4) percent = 1 - (animations[ATTACKR].currentAnimFrameTime - 0.4f) / 0.6f;

                animations[BASE].UpdateBlendAnim(animations, BASE, ATTACKR, percent, animWeights);   //base -> Main Attack
                if (animations[ATTACKR].currentAnimFrameTime > 0.1)
                {
                    isAttacking = false; //end attack
                }
            }
        }

    }
}
