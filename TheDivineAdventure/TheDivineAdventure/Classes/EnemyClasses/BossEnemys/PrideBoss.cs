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
    class PrideBoss : Boss
    {
        protected const int BASE = 0, IDLE = 1, WALK = 2, RECHARGE = 3, ATTCKRANGE = 4, ATTACKMELEE = 5, ATTCKRWU = 6;  //Animation IDs
        public bool statuesAlive;
        private bool attack1, attack2, recharging;
        private int barrageTimer, barrageDuration, barrageSpeed;


        public PrideBoss(List<SoundEffect> s, Vector3 spawnLoc, PlayScene parent, ContentManager content) : base(s, spawnLoc, parent, content)
        {
            //Capsule Collider
            boundingCollider = new CapsuleCollider(65, 18, pos, rot, parent.gpu, Color.White);
            animWeights = new float[100];
            name = "The Archdemon Pride";

            maxHealth = 800;
            health = 800;

            vel = 0;
            speedMax = 1.7f;
            runMod = 1.3f;
            accel = .015f;
            prefDistance = 10;

            statuesAlive = true;
            phaseTimer = 0;
            phase = 2;
            recharging = true;
            barrageDuration = 120;
            barrageTimer = barrageDuration * -1;
            barrageSpeed = 8;
                

            animations[BASE] = loader.Load("MOD_Pride/ANIM_Pride_Base.fbx", "MOD_Pride", true, 3, skinFx, rescale: 2.7f);
            animations[IDLE] = loader.Load("MOD_Pride/ANIM_Pride_Idle.fbx", "MOD_Pride", true, 4, skinFx, rescale: 2.7f);
            animations[WALK] = loader.Load("MOD_Pride/ANIM_Pride_Walk.fbx", "MOD_Pride", true, 4, skinFx, rescale: 2.7f);
            animations[RECHARGE] = loader.Load("MOD_Pride/ANIM_Pride_Recharge.fbx", "MOD_Pride", true, 4, skinFx, rescale: 2.7f);
            animations[ATTACKMELEE] = loader.Load("MOD_Pride/ANIM_Pride_Attack_Melee.fbx", "MOD_Pride", true, 4, skinFx, rescale: 2.7f);
            animations[ATTACKMELEE].loopAnimation = false;
            animations[ATTCKRANGE] = loader.Load("MOD_Pride/ANIM_Pride_Attack_Range.fbx", "MOD_Pride", true, 4, skinFx, rescale: 2.7f);
            animations[ATTCKRWU] = loader.Load("MOD_Pride/ANIM_Pride_Attack_Range_WU.fbx", "MOD_Pride", true, 4, skinFx, rescale: 2.7f);
            animations[ATTCKRWU].loopAnimation = false;
        }

        public override void Update(float dt, GameTime gametime, Camera cam)
        {
            speedMax = 1.7f;
            if (timeToDestroy)
            {
                parentScene.thisLevel.MissionUpdate(3, gametime);
            }
            base.Update(dt, gametime, cam);

            if (parentScene.worldEnemyList.Count <= 0 && isActive) statuesAlive = false;
        }

        protected override void PhaseUpdate(GameTime gt)
        {
            phaseTimer++;
            //Rage phase
            if (phase !=0 && !statuesAlive)
            {
                attackRange = 60;
                attackSpeed = 50;
                attackDamage = 30;
                attackLength = 25f;
                Pos = new Vector3(0, 22, -900)*2.2f;
                recharging = false;
                phase = 0;

                parentScene.thisLevel.StaticMeshes.Remove(parentScene.thisLevel.StaticMeshes[^1]);

                return;
            }
            //Recharge Phase
            else if(phase == 2 && (phaseStartHealth-health >=150 || phaseTimer >= 900))
            {
                phase = 1;
                phaseStartHealth = health;
                phaseTimer = 0;
                Pos = new Vector3(0, 22, -1070)*2.2f;
                recharging = true;


                attack2 = false;
                barrageTimer = barrageDuration * -1;

                foreach (Enemy enemy in parentScene.worldEnemyList)
                {
                    enemy.isActive = true;
                }
                return;
            }
            //Base Phase Phase
            else if (phase == 1 && (health - phaseStartHealth >= 75 || phaseTimer >= 900))
            {
                attackRange = 60;
                attackSpeed = 50;
                attackDamage = 30;
                attackLength = 25f;

                phase = 2;
                phaseStartHealth = health;
                phaseTimer = 0;
                Pos = new Vector3(0, 22, -900)*2.2f;
                recharging = false;

                foreach (Enemy enemy in parentScene.worldEnemyList)
                {
                    enemy.isActive = false;
                    enemy.projList = new List<Attack>();
                }
                return;
            }
        }


        protected override void AIProcessing(int Phase, Camera cam)
        {
            switch (phase)
            {
                case 0:
                    if (!attack1)
                    {
                        FaceTarget(parentScene.player.world);
                        Move();
                    }
                    if (attackTimer <= attackSpeed) { attackTimer += 1f; return; }
                    //check distance from player
                    if (Vector3.Distance(player.Pos, world.Translation) <= attackRange)
                    {
                        Attack(false, cam);
                    }
                    break;
                case 1:
                    FaceTarget(parentScene.player.world);
                    health += .1f;
                    break;
                case 2:
                    if (!attack2 && !attack1)
                    {
                        FaceTarget(parentScene.player.world);
                        Move();
                    }
                    if (barrageTimer < 0)
                    {
                        if (attackTimer <= attackSpeed) { attackTimer += 1f; return; }
                        //check distance from player
                        if (Vector3.Distance(player.Pos, world.Translation) <= attackRange)
                        {
                            Attack(false, cam);
                        }
                    }
                    if (barrageTimer < barrageDuration) barrageTimer++;
                    BarrageAttack(cam);
                    break;

            }

        }

        private void BarrageAttack(Camera cam)
        {
            FaceTarget(parentScene.player.world);
            //reset barage timer
            if (barrageTimer > barrageDuration)
            {
                soundEffects[0].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                animations[ATTCKRWU].StopAnimation();
                attack2 = false;
                barrageTimer = barrageDuration * -1;
            }

            if (barrageTimer > 30) attack2 = true;

            if (attack2 && barrageSpeed <=0 )
            {
                soundEffects[0].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                AttackPattern.PrideProjBarrage(world.Translation + world.Up * 50, player.Pos, attackDamage / 8, projList, cam);
                barrageSpeed = 8;
            }
            barrageSpeed--;
        }

        //Basic Attack
        protected override void Attack(bool isMelee, Camera cam)
        {
            isAttacking = true;
            attack1 = true;
            if (attDurTimer < attackLength) { attDurTimer += 1; return; }
            //Perform attack
            soundEffects[0].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
            AttackPattern.PrideRage(world.Translation+ world.Backward*4, world.Backward, rot, attackDamage, projList, cam);

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


        public override void Damage(float amt)
        {
            if (recharging) return;
            base.Damage(amt);
        }


        //Blend aniamtions
        protected override void BlendAnims(GameTime gameTime)
        {
            if (barrageTimer == 0) animations[ATTCKRWU].BeginAnimation(0, gameTime);

            //reset weights for blending
            SetAnimBlends(1);
            float percent;
            //walk/run blending
            percent = vel / speedMax;
            animations[BASE].UpdateBlendAnim(animations, BASE, WALK, percent, animWeights);   //idle -> walk

            //Melee Attack
            if (attack1 && !animations[ATTACKMELEE].animationRunning) animations[ATTACKMELEE].BeginAnimation(0, gameTime); //start animation
            if (animations[ATTACKMELEE].animationRunning)
            {
                animations[ATTACKMELEE].Update(gameTime);
                percent = 1;
                animations[BASE].UpdateBlendAnim(animations, BASE, ATTACKMELEE, percent, animWeights);   //base -> Main Attack
                if (animations[ATTACKMELEE].currentAnimFrameTime > 0.3)
                {
                    isAttacking = false; //end attack
                    attack1 = false; //end attack1
                }
            }
            if (recharging)
            {
                animations[RECHARGE].Update(gameTime);
                percent = 1;
                animations[BASE].UpdateBlendAnim(animations, BASE, RECHARGE, percent, animWeights);   //base -> Main Attack
            }
            if(barrageTimer>0 && !attack2)
            {
                animations[ATTCKRWU].Update(gameTime);
                percent = 1;
                animations[BASE].UpdateBlendAnim(animations, BASE, ATTCKRWU, percent, animWeights);   //base -> Main Attack

            }
            if (attack2)
            {
                animations[ATTCKRANGE].Update(gameTime);
                percent = 1;
                animations[BASE].UpdateBlendAnim(animations, BASE, ATTCKRANGE, percent, animWeights);   //base -> Main Attack
            }
        }
    }
}
