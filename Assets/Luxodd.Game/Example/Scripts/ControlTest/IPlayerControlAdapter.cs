using System;
using UnityEngine;

namespace Luxodd.Game.Example.Scripts.ControlTest
{
    public interface IPlayerControlAdapter 
    {
        Vector2 MovementVector { get; }
        event Action JumpButtonPressed;
        event Action UseItemButtonPressed;

        bool IsFireButtonPressed { get; }
    }
}
