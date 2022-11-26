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
    class HellHound : Enemy
    {

        //Constructor ----------
        public HellHound(List<SoundEffect> s, string role_, Vector3 spawnLoc, PlayScene parent, SkinModel model, ContentManager content) : base(s, role_, spawnLoc, parent, model, content)
        {
            //Capsule Collider
            boundingCollider = new CapsuleCollider(10, 8, pos, rot, parent.gpu, Color.White);
            animWeights = new float[100];

            health = 50;

            vel = 0;
            speedMax = 1.3f;
            runMod = 1.3f;
            accel = .015f;
            prefDistance = 10;

            attackRange = 20;
            attackSpeed = 40;
            attackDamage = 10f;
            attackLength = 20f;

            animations[IDLE] = loader.Load("MOD_HellHound/ANIM_Hellhound_Idle.fbx", "MOD_HellHound", true, 4, skinFx, rescale: 2.2f, yRotation: -90);
            animations[WALK] = loader.Load("MOD_HellHound/ANIM_Hellhound_Walk.fbx", "MOD_HellHound", true, 4, skinFx, rescale: 2.2f, yRotation: -90);
            animations[RUN] = loader.Load("MOD_HellHound/ANIM_Hellhound_Run.fbx", "MOD_HellHound", true, 4, skinFx, rescale: 2.2f, yRotation: -90);
            animations[ATTACKM] = loader.Load("MOD_HellHound/ANIM_Hellhound_Attack.fbx", "MOD_HellHound", true, 4, skinFx, rescale: 2.2f, yRotation: -90);
            animations[ATTACKM].loopAnimation = false;
        }

        //Update Function
        public override void Update(float dt, GameTime gametime, Camera cam)
        {
            base.Update(dt, gametime, cam);
        }

        //AIProcessing
        protected override void AIProcessing(Camera cam)
        {
            if (!isAttacking)
            {
                FacePlayer();
                Move();
            }
            if (attackTimer <= attackSpeed) { attackTimer += 1f; return; }
            //check distance from player
            if (Vector3.Distance(player.Pos, world.Translation) <= attackRange)
            {
                Attack(false, cam);
            }
        }

        //Attack Function
        protected override void Attack(bool isMelee, Camera cam)
        {
            isAttacking = true;
            isMeleeAttacking = true;
            if (attDurTimer < attackLength) { attDurTimer += 1; return; }

            //perform attack
            soundEffects[2].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
            AttackPattern.HoundBite(world.Translation + world.Backward * 5, world.Backward * 2, rot, attackDamage, projList, cam);

            //process projectile
            foreach (Attack p in projList)
            {
                if (p.TimeToDestroy)
                {
                    projList.Remove(p);
                    break;
                }
            }
            base.Attack(isMelee, cam);
        }

        //Blend aniamtions
        protected override void BlendAnims(GameTime gameTime)
        {
            //reset weights for blending
            SetAnimBlends(1);
            float percent;
            //walk/run blending
            percent = vel / speedMax;
            animations[BASE].UpdateBlendAnim(animations, BASE, WALK, percent, animWeights);   //idle -> walk

            percent = (vel / runMod) / speedMax * runMod;
            animations[BASE].UpdateBlendAnim(animations, BASE, RUN, percent, animWeights);    //walk -> run

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
        }

    }
}
