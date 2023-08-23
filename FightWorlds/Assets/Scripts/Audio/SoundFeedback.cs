using UnityEngine;

public class SoundFeedback : MonoBehaviour
{
    [SerializeField]
    private AudioClip clickSound, placeSound, wrongPlacementSound, breakSound;

    [SerializeField]
    private AudioSource audioSource;

    public void PlaySound(SoundType soundType)
    {
        switch (soundType)
        {
            case SoundType.Click:
                audioSource.PlayOneShot(clickSound);
                break;
            case SoundType.Place:
                audioSource.PlayOneShot(placeSound);
                break;
            case SoundType.WrongPlacement:
                audioSource.PlayOneShot(wrongPlacementSound);
                break;
            case SoundType.Break:
                audioSource.PlayOneShot(breakSound);
                break;
            default:
                break;
        }
    }

    public void PlayMusic()
    {
        // TODO add some track + implement music playing
    }
}