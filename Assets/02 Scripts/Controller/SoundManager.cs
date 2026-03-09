using UnityEngine;

namespace Won
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private AudioSource _sfxSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PlayBGM(AudioClip clip)
        {
            _bgmSource.clip = clip;
            _bgmSource.Play();
        }

        public void StopBGM()
        {
            _bgmSource.Stop();
        }

        public void PlaySFX(AudioClip clip)
        {
            _sfxSource.PlayOneShot(clip);
        }
    }
}