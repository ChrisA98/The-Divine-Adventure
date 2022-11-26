using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace TheDivineAdventure
{
    public class SourceLight
    {
        //Types of lights
        private Vector3 position;
        private Color lightColor;
        private float lightArea;    //area affected by light


        #region Constructors
        public SourceLight(Vector3 position_, float range, Vector3 color)
        {
            position = position_*2.2f;
            lightArea = range;
            lightColor = new Color(color);
        }
        #endregion

        #region Light Logic

        public float? IsLighting(Vector3 target)
        {
            float dist = Vector3.Distance(position, target);
            dist = 1 / (dist * dist) * lightArea;
            //Debug.WriteLine(dist);
            if (dist >= 0.00008f)
            {
                return dist;   //inverse square law for light falloff
            }
            return null;    //light is too far away
        }

        public Vector3 DirectionFrom(Vector3 target)
        {
            return target - position;   //direction of light
        }

        #endregion

        #region Getters and Setters
        public Vector3 LightColor
        {
            get { return lightColor.ToVector3();}
            set { lightColor = new Color(value);}
        }
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }
        public float LightArea
        {
            get { return lightArea; }
        }
        #endregion

    }
}
