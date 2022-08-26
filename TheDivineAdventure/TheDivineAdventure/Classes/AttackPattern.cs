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
        public static void singleProj(Vector3 origin, Vector3 target, float speed, float damage,  List<Attack> projList)
        {
            projList.Add(new Attack(origin, target, speed, damage));
        }

        public static void tripleProj(Vector3 origin, Vector3 target, float speed, float damage, List<Attack> projList)
        {
            projList.Add(new Attack(origin + new Vector3(0, 0, -5), target + new Vector3(30, 0, 0), speed, damage));
            projList.Add(new Attack(origin, target, speed, damage));
            projList.Add(new Attack(origin + new Vector3(0, 0, -5), target - new Vector3(30, 0, 0), speed, damage));
        }

        public static void quinProj(Vector3 origin, Vector3 target, float speed, float damage, List<Attack> projList)
        {
            projList.Add(new Attack(origin + new Vector3(0, 0, -10), target + new Vector3(70, 0, 0), speed, damage));
            projList.Add(new Attack(origin + new Vector3(0, 0, -5), target + new Vector3(40, 0, 0), speed, damage));
            projList.Add(new Attack(origin, target, speed, damage));
            projList.Add(new Attack(origin + new Vector3(0, 0, -5), target - new Vector3(40, 0, 0), speed, damage));
            projList.Add(new Attack(origin + new Vector3(0, 0, -10), target - new Vector3(70, 0, 0), speed, damage));
        }

        ///MELEE///
        public static void singleMel(Vector3 origin, Vector3 target, float speed, float damage, List<Attack> projList)
        {
            projList.Add(new Attack(origin + target, target, speed, damage));
        }
        public static void tripleMel(Vector3 origin, Vector3 target, float speed, float damage, List<Attack> projList)
        {
            projList.Add(new Attack(origin + new Vector3(0, 0, 10), target, speed, damage));
            projList.Add(new Attack(origin + new Vector3(20, 0, 10), target, speed, damage));
            projList.Add(new Attack(origin + new Vector3(-20, 0, 10), target, speed, damage));
        }
    }
}
