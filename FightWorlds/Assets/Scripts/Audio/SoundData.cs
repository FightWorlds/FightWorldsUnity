using System;
using System.Collections.Generic;
using UnityEngine;

namespace FightWorlds.Audio
{
    [CreateAssetMenu(fileName = "SoundsDatabase",
    menuName = "ScriptableObjects/SoundsDatabase")]
    public class SoundsDatabase : ScriptableObject
    {
        public List<SoundData> sounds;
    }

    [Serializable]
    public class SoundData
    {
        [field: SerializeField] public SoundType Type { get; private set; }
        [field: SerializeField] public AudioClip Clip { get; private set; }
    }
}