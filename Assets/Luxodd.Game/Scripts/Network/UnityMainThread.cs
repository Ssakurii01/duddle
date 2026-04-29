using System;
using System.Collections.Generic;
using UnityEngine;

namespace Luxodd.Game.Scripts.Network
{
    internal class UnityMainThread : MonoBehaviour
    {
        internal static UnityMainThread Worker;
        private readonly Queue<Action> _jobs = new Queue<Action>();

        void Awake()
        {
            Worker = this;
        }

        void Update()
        {
            while (_jobs.Count > 0)
            {
                _jobs.Dequeue().Invoke();
            }
        }

        internal void AddJob(Action newJob)
        {
            _jobs.Enqueue(newJob);
        }
    }
}