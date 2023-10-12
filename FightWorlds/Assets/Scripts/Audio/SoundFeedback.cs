using UnityEngine;

namespace FightWorlds.Audio
{
    public class SoundFeedback : MonoBehaviour
    {
        [SerializeField] private SoundsDatabase database;
        [SerializeField] private AudioSource audioSource;

        public void PlaySound(SoundType soundType)
        {
            audioSource.PlayOneShot(database
                .sounds.Find(s => s.Type == soundType).Clip);
        }

        public void PlayMusic()
        {
            // TODO add some track + implement music playing
        }
    }
}