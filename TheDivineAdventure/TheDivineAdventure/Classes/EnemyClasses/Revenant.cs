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
    class Revenant : Enemy
    {
        private float flyHeight;
        private float flyBobFunc;

        //Constructor ----------
        public Revenant(List<SoundEffect> s, string role_, Vector3 spawnLoc, PlayScene parent, SkinModel model, ContentManager content) : base(s, role_, spawnLoc, parent, model, content)
        {

            health = 50;

            vel = 0;
            speedMax = 1.3f;
            runMod = 1.3f;
            accel = .015f;
            prefDistance = 120;
            flyHeight = 20f;

            attackRange = 30;
            attackSpeed = 75;
            attackDamage = 15f;
            attackLength = 20f;

            animations[IDLE] = loader.Load("MOD_Revenant/ANIM_Revenant_Idle.fbx", "MOD_Revenant", true, 4, skinFx, rescale: 2.2f, yRotation: -90);
            animations[WALK] = loader.Load("MOD_Revenant/ANIM_Revenant_Walk.fbx", "MOD_Revenant", true, 4, skinFx, rescale: 2.2f, yRotation: -90);

            //Capsule Collider
            boundingCollider = new CapsuleCollider(10, 8, pos, rot, parent.gpu, Color.White);
            animWeights = new float[100];
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
            float distance = (Vector3.Distance(player.Pos, world.Translation + (Vector3.One * player.width)));
            if (distance > attackRange && distance < 200)
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
            soundEffects[0].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
            AttackPattern.RevenantProj(world.Translation + world.Backward * 5, player.Pos, attackDamage, projList, cam);
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
            percent = 1;
            animations[BASE].UpdateBlendAnim(animations, BASE, WALK, percent, animWeights);   //idle -> walk

            percent = 1;
            animations[BASE].UpdateBlendAnim(animations, BASE, WALK, percent, animWeights);   //walk -> run
            isAttacking = false;
        }

        protected override void Fall()
        {
            FlyBob();
            foreach (Level.FloorTile floor in parentScene.thisLevel.FloorTiles)
            {
                Ray floorCheck = new Ray(boundingCollider.Position, world.Down);
                if (Vector3.Distance(floor.Position, pos) < 500)
                {
                    if (floor.collider.Intersects(floorCheck) != null)
                    {
                        flyHeight += GetHighestSurface();
                        if (flyHeight > pos.Y) pos.Y += world.Up.Y*.4f;
                        else pos.Y -= world.Up.Y * 0.2f;
                        fallSpeed = 0;
                        onGround = true;
                        break;
                    }
                    onGround = false;
                }
            }

            if (!onGround) fallSpeed += parentScene.parent.gravity;
            pos.Y -= fallSpeed;

            // Once on the ground dont fall more
            if (onGround)
            {
                fallSpeed = 0;
            }
        }

        private float GetHighestSurface()
        {
            float height = 0;
            Ray floorCheck = new Ray(boundingCollider.Position, world.Down);
            if(Vector3.Distance(pos,player.Pos)<= attackRange)
            {
                return player.Pos.Y + 30;
            }
            foreach (Level.FloorTile floor in parentScene.thisLevel.FloorTiles)
            {
                if (floor.collider.Intersects(floorCheck) != null)
                {
                    if (floor.Position.Y+30 > height) height = floor.Position.Y + 30;
                }
            }
            foreach (Level.DeathBox floor in parentScene.thisLevel.DeathBoxes)
            {
                if (Vector3.Distance(floor.Position, pos) < 400) continue;
                if (floor.collider.Intersects(floorCheck) != null)
                {
                    if (floor.Position.Y+ 130 > height) height = floor.Position.Y + 130;
                }
            }
            return height;
        }


        private void FlyBob()
        {
            flyBobFunc += 0.13f;
            flyHeight = 10 + (float)Math.Sin(flyBobFunc)*-2;
        }
    }
}
