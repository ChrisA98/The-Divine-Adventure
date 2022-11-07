using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace TheDivineAdventure
{
    class AttackPattern
    {
        /////////////////
        ///CONSTRUCTOR///
        /////////////////
        AttackPattern()
        {

        }

        ///////////////
        ///FUNCTIONS///
        ///////////////
        ///PROJECTILES///
        public static void singleProj(Vector3 origin, Vector3 target, float speed, float damage,  List<Attack> projList, Camera cam_)
        {
            projList.Add(new Attack(origin, target, speed, damage, cam_));
        }

        public static void tripleProj(Matrix origin, Vector3 target, float speed, float damage, List<Attack> projList, Camera cam_)
        {
            projList.Add(new Attack(origin.Translation + origin.Left * 5, target + origin.Left * 5, speed, damage, cam_));
            projList.Add(new Attack(origin.Translation, target, speed, damage, cam_));
            projList.Add(new Attack(origin.Translation + origin.Right * 5, target - origin.Left * 5, speed, damage, cam_));
        }

        public static void quinProj(Matrix origin, Vector3 target, float speed, float damage, List<Attack> projList, Camera cam_)
        {
            projList.Add(new Attack(origin.Translation + origin.Left*10, target + new Vector3(30, 0, 0), speed, damage, cam_));
            projList.Add(new Attack(origin.Translation + origin.Left*5, target + new Vector3(12, 0, 0), speed, damage, cam_));
            projList.Add(new Attack(origin.Translation, target, speed, damage, cam_));
            projList.Add(new Attack(origin.Translation + origin.Right * 5, target - new Vector3(12, 0, 0), speed, damage, cam_));
            projList.Add(new Attack(origin.Translation + origin.Right * 10, target - new Vector3(30, 0, 0), speed, damage, cam_));
        }
        public static void MageMain(Matrix origin, Vector3 target, float speed, float damage, List<Attack> projList, Camera cam_)
        {
            origin.Translation += origin.Up * 10 + origin.Left * 8;
            projList.Add(new Attack(origin.Translation, target, speed, damage, cam_));
            projList.Add(new Attack(origin.Translation + origin.Right * 5, target, speed, damage, cam_));
            projList.Add(new Attack(origin.Translation + origin.Left*5, target, speed, damage, cam_));
            projList.Add(new Attack(origin.Translation + new Vector3(0, 5, 0), target, speed, damage, cam_));
            projList.Add(new Attack(origin.Translation + new Vector3(0, -5, 0), target, speed, damage, cam_));
        }
        public static void MageAlt(Matrix origin, float speed, float damage, List<Attack> projList, Camera cam_)
        {
            Vector3[] targetPoints = new Vector3[16];
            targetPoints[0] = new Vector3(25, 40, 0);
            targetPoints[1] = new Vector3(23, 40, 9);
            targetPoints[2] = new Vector3(18, 40, 17);
            targetPoints[3] = new Vector3(10, 40, 23);
            targetPoints[4] = new Vector3(1, 40, 25);
            targetPoints[5] = new Vector3(-9, 40, 23);
            targetPoints[6] = new Vector3(-17, 40, 19);
            targetPoints[7] = new Vector3(-22, 40, 11);
            targetPoints[8] = new Vector3(-25, 40, 2);
            targetPoints[9] = new Vector3(-24, 40, -8);
            targetPoints[10] = new Vector3(-19, 40, -16);
            targetPoints[11] = new Vector3(-12, 40, -22);
            targetPoints[12] = new Vector3(-3, 40, -25);
            targetPoints[13] = new Vector3(7, 40, -24);
            targetPoints[14] = new Vector3(15, 40, -20);
            targetPoints[15] = new Vector3(22, 40, -13);

            foreach (Vector3 point in targetPoints)
            {
                projList.Add(new Attack(origin.Translation+point, origin.Translation+(origin.Down*point), speed, damage, cam_));
            }

            targetPoints[0] = new Vector3(50, 40, 0);
            targetPoints[1] = new Vector3(43, 40, 25);
            targetPoints[2] = new Vector3(25, 40, 43);
            targetPoints[3] = new Vector3(0, 40, 50);
            targetPoints[4] = new Vector3(-25, 40, 43);
            targetPoints[5] = new Vector3(-43, 40, 25);
            targetPoints[6] = new Vector3(-50, 40, 0);
            targetPoints[7] = new Vector3(-43, 40, -25);
            targetPoints[8] = new Vector3(-25, 40, -43);
            targetPoints[9] = new Vector3(0, 40, -50);
            targetPoints[10] = new Vector3(25, 40, -43);
            targetPoints[11] = new Vector3(43, 40, -25);
            targetPoints[12] = new Vector3(50, 40, 0);
            targetPoints[13] = new Vector3(43, 40, 25);
            targetPoints[14] = new Vector3(25, 40, 43);
            targetPoints[15] = new Vector3(0, 40, 50);

            foreach (Vector3 point in targetPoints)
            {
                projList.Add(new Attack(origin.Translation + point, origin.Translation + (origin.Down * point), speed, damage, cam_));
            }

            targetPoints[0] = new Vector3(50, 40, 0);
            targetPoints[1] = new Vector3(46, 40, 19);
            targetPoints[2] = new Vector3(36, 40, 35);
            targetPoints[3] = new Vector3(20, 40, 46);
            targetPoints[4] = new Vector3(2, 40, 50);
            targetPoints[5] = new Vector3(-17, 40, 47);
            targetPoints[6] = new Vector3(-33, 40, 37);
            targetPoints[7] = new Vector3(-45, 40, 22);
            targetPoints[8] = new Vector3(-50, 40, 3);
            targetPoints[9] = new Vector3(-48, 40, -15);
            targetPoints[10] = new Vector3(-38, 40, -32);
            targetPoints[11] = new Vector3(-23, 40, -44);
            targetPoints[12] = new Vector3(-5, 40, -50);
            targetPoints[13] = new Vector3(14, 40, -48);
            targetPoints[14] = new Vector3(31, 40, -39);
            targetPoints[15] = new Vector3(43, 40, -25);

            foreach (Vector3 point in targetPoints)
            {
                projList.Add(new Attack(origin.Translation + point, origin.Translation + (origin.Down * point), speed, damage, cam_));
            }
        }
        public static void ImpProj(Vector3 origin, Vector3 target, float damage, List<Attack> projList, Camera cam_)
        {
            projList.Add(new Attack(origin, target, 15f, damage, cam_));
        }
        public static void RevenantProj(Vector3 origin, Vector3 target, float damage, List<Attack> projList, Camera cam_)
        {
            projList.Add(new Attack(origin, target, 4, damage, cam_));
        }

        ///MELEE///
        public static void singleMel(Vector3 origin, Vector3 target, Vector3 rotation, float damage, List<Attack> projList, Camera cam_)
        {
            projList.Add(new Attack(origin + target, target, rotation, damage, new Vector3(15,75,15), cam_));
        }
        public static void tripleMel(Vector3 origin, Vector3 target, Vector3 rotation, float damage, List<Attack> projList, Camera cam_)
        {
            projList.Add(new Attack(origin + target, target, rotation, damage, new Vector3(90, 75, 15), cam_));
        }
        public static void WarriorHeavy(Vector3 origin, Vector3 target, Vector3 rotation, float damage, List<Attack> projList, Camera cam_)
        {
            projList.Add(new Attack(origin + (target* 36), target, rotation, damage, new Vector3(8, 40, 100), cam_));
        }
        public static void RogueMain(Vector3 origin, Vector3 target, Vector3 rotation, float damage, List<Attack> projList, Camera cam_)
        {
            projList.Add(new Attack(origin + target, target, rotation, damage, new Vector3(12, 75, 12), cam_));
        }
        public static void RogueHeavy(Vector3 origin, Vector3 target, Vector3 rotation, float damage, List<Attack> projList, Camera cam_)
        {
            projList.Add(new Attack(origin, Vector3.Zero, rotation, damage, new Vector3(40, 12, 40), cam_, 15));
        }
        public static void HoundBite(Vector3 origin, Vector3 target, Vector3 rotation, float damage, List<Attack> projList, Camera cam_)
        {
            projList.Add(new Attack(origin + target, target, rotation, damage, new Vector3(10, 20, 10), cam_));
        }
        public static void MinotaurCharge(Vector3 origin, Vector3 target, Vector3 rotation, float damage, List<Attack> projList, Camera cam_, float force_)
        {
            projList.Add(new Attack(origin + target, target, rotation, damage, new Vector3(40, 40, 15), cam_, force_));
        }
        public static void MinotaurMain(Vector3 origin, Vector3 target, Vector3 rotation, float damage, List<Attack> projList, Camera cam_)
        {
            projList.Add(new Attack(origin + target, target, rotation, damage, new Vector3(100, 75, 15), cam_));
        }
    }
}
