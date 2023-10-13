using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FightWorlds.Audio
{
    public class SoundFeedback : MonoBehaviour
    {
        [SerializeField] private SoundsDatabase database;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private int playSoundsPerOnce;

        private const float pitchDif = 0.2f;

        private Dictionary<SoundType, int> playSoundCounters;

        public void PlaySound(SoundType soundType)
        {
            if (playSoundCounters[soundType] >= playSoundsPerOnce)
                return;
            playSoundCounters[soundType]++;
            var clip = database.sounds.Find(s => s.Type == soundType).Clip;
            audioSource.pitch = GetPitch();
            audioSource.PlayOneShot(clip);
            StartCoroutine(ReduceCounter(soundType, clip.length));
        }

        public void PlayMusic()
        {
            // TODO add some track + implement music playing
        }

        private IEnumerator ReduceCounter(SoundType type, float sec)
        {
            yield return new WaitForSeconds(sec);
            playSoundCounters[type]--;
        }

        private void Awake()
        {
            playSoundCounters = new();
            foreach (var sound in database.sounds)
                playSoundCounters.Add(sound.Type, 0);
        }

        private float GetPitch()
        {
            return 1 + Random.Range(-1, 2) * pitchDif;
        }
    }
}