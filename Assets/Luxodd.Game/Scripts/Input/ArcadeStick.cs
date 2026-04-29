using UnityEngine;

namespace Luxodd.Game.Scripts.Input
{
    
    public readonly struct ArcadeStick 
    {
        public readonly float X;    // -1..1
        public readonly float Y;    // -1..1
        public readonly Vector2 Raw;

        public ArcadeStick(float x, float y)
        {
            X  = x;
            Y = y;
            Raw = new Vector2(x, y);
        }
        
        public Vector2 Vector => new Vector2(X, Y);
        public float Magnitude => Raw.magnitude;
        
        public bool IsCentered(float epsilon = 0.001f) => Magnitude <= epsilon;

        public override string ToString()
        {
            return $"X: {X}, Y: {Y}, Magnitude: {Magnitude}, Vector: {Vector},  IsCentered: {IsCentered()}";
        }
    }
}
