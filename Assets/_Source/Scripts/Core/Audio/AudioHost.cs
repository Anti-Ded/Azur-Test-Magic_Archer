using UnityEngine;

namespace MagicArcher.Core.Audio
{
    public sealed class AudioHost : MonoBehaviour
    {
        [SerializeField] AudioClip _music;
        [SerializeField] AudioClip[] _bowReleases;
        [SerializeField] AudioClip[] _orcDeaths;
        [SerializeField] AudioClip _orcHit;
        [SerializeField] AudioClip _coins;
        [SerializeField] AudioClip _merge;
        [SerializeField] AudioClip _unitBuy;
        [SerializeField] AudioClip _victory;
        [SerializeField] AudioClip _defeat;

        AudioSource _musicSource;
        AudioSource _sfxSource;

        public IAudioService CreateService()
        {
            EnsureSources();
            return new AudioService(
                _musicSource,
                _sfxSource,
                _music,
                _bowReleases,
                _orcDeaths,
                _orcHit,
                _coins,
                _merge,
                _unitBuy,
                _victory,
                _defeat);
        }

        void EnsureSources()
        {
            if (_musicSource == null)
            {
                _musicSource = gameObject.AddComponent<AudioSource>();
                _musicSource.playOnAwake = false;
                _musicSource.loop = true;
                _musicSource.volume = 0.35f;
            }

            if (_sfxSource == null)
            {
                _sfxSource = gameObject.AddComponent<AudioSource>();
                _sfxSource.playOnAwake = false;
                _sfxSource.loop = false;
                _sfxSource.volume = 0.9f;
            }
        }
    }
}
