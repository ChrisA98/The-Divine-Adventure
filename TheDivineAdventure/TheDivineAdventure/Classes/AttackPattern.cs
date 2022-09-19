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
            projList.Add(new Attack(origin.Translation + origin.Left * 5, target + origin.Left * 5, speed, damage,cam_));
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

        ///MELEE///
        public static void singleMel(Vector3 origin, Vector3 target, float damage, List<Attack> projList)
        {
            projList.Add(new Attack(origin + target, target, damage));
        }
        public static void tripleMel(Vector3 origin, Vector3 target, float damage, List<Attack> projList)
        {
            projList.Add(new Attack(origin + new Vector3(0, 0, 10), target, damage));
            projList.Add(new Attack(origin + new Vector3(20, 0, 10), target, damage));
            projList.Add(new Attack(origin + new Vector3(-20, 0, 10), target, damage));
        }
    }
}
