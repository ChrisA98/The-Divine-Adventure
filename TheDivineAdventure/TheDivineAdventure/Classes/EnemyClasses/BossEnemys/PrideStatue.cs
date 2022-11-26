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
    class PrideStatue : Enemy
    {

        //Constructor ----------
        public PrideStatue(List<SoundEffect> s, Vector3 spawnLoc, PlayScene parent, SkinModel model, ContentManager content) : base(s, "NA", spawnLoc, parent, model, content)
        {
            //Capsule Collider
            boundingCollider = new CapsuleCollider(140, 40, pos, rot, parent.gpu, Color.White);
            animWeights = new float[100];

            health = 350;

            vel = 0;
            speedMax = 0;
            runMod = 0;
            accel = 0;
            prefDistance = 10;

            attackRange = 20;
            attackSpeed = 20;
            attackDamage = 45f;
            attackLength = 20f;
            maxAttackRange = 400;

            isActive = false;

            animations[IDLE] = model;
        }

        //Update Function
        public override void Update(float dt, GameTime gametime, Camera cam)
        {
            //end animation updates
            world = Matrix.CreateScale(1f) *
                        Matrix.CreateRotationY(MathHelper.ToRadians(Rot.Y)) *
                        Matrix.CreateTranslation(Pos);

            //update hitbox
            boundingCollider.Update(Pos);

            if (waking <= wakeTime) waking += 1; //enemy is waking 
            if (waking == wakeTime)
            {
                skinFx.SetDiffuseCol(Color.White.ToVector4());
                skinFx.SetSpecularCol(Color.Red.ToVector3());
                skinFx.SetSpecularPow(255f);
                skinFx.SetFogColor(Color.Red);

                foreach (SkinModel sk in animations)
                {
                    if (sk != null && sk != animations[BASE])
                    {
                        sk.BeginAnimation(0, gametime);
                    }
                }
                waking += 1;
            }

            if (!isActive) return; //enemy is turned off

            foreach (Attack p in projList)
            {
                if (p.TimeToDestroy)
                {
                    projList.Remove(p);
                    break;
                }
            }
            if (health <= 0)
            {
                //death of entity
                PlayScene.score += (int)attackDamage * 8;
                timeToDestroy = true;
            }

            //update AI
            AIProcessing(cam);
        }

        //AI Processing
        protected override void AIProcessing(Camera cam)
        {
            if (attackTimer <= attackSpeed) { attackTimer += 1f; return; }
            Attack(false, cam);
        }


        public override void Damage(float amt, float force)
        {
            if (!isActive) return;
            health -= amt;
        }

        //Attack Function
        protected override void Attack(bool isMelee, Camera cam)
        {
            isAttacking = true;
            if (!isMelee)
            {
                isMeleeAttacking = true;
                if (attDurTimer < attackLength) { attDurTimer += 1; return; }
                //perform attack
                soundEffects[0].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                AttackPattern.PrideStatBig(world.Translation + world.Up * 100, player.Pos, attackDamage / 2, projList, cam);
                
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
            base.Attack(isMelee, cam);
        }


    }
}
