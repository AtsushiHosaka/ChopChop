using System;
using UnityEngine;

namespace ChopChop.Core
{
    [Serializable]
    public struct PlayerId : IEquatable<PlayerId>
    {
        [SerializeField] private int value;

        public int Value => value;

        public PlayerId(int value)
        {
            this.value = Mathf.Max(0, value);
        }

        public bool Equals(PlayerId other)
        {
            return value == other.value;
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return value;
        }

        public override string ToString()
        {
            return $"P{value + 1}";
        }
    }
}
