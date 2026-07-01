using UnityEngine;

namespace MagicArcher.Core.Audio
{
    public sealed class AudioService : IAudioService
    {
        readonly AudioSource _musicSource;
        readonly AudioSource _sfxSource;
        readonly AudioClip[] _bowReleases;
        readonly AudioClip[] _orcDeaths;
        readonly AudioClip _orcHit;
        readonly AudioClip _coins;
        readonly AudioClip _merge;
        readonly AudioClip _unitBuy;
        readonly AudioClip _victory;
        readonly AudioClip _defeat;

        public AudioService(
            AudioSource musicSource,
            AudioSource sfxSource,
            AudioClip music,
            AudioClip[] bowReleases,
            AudioClip[] orcDeaths,
            AudioClip orcHit,
            AudioClip coins,
            AudioClip merge,
            AudioClip unitBuy,
            AudioClip victory,
            AudioClip defeat)
        {
            _musicSource = musicSource;
            _sfxSource = sfxSource;
            _bowReleases = bowReleases;
            _orcDeaths = orcDeaths;
            _orcHit = orcHit;
            _coins = coins;
            _merge = merge;
            _unitBuy = unitBuy;
            _victory = victory;
            _defeat = defeat;

            if (_musicSource != null && music != null)
            {
                _musicSource.clip = music;
                _musicSource.loop = true;
            }
        }

        public void PlayMusic()
        {
            if (_musicSource == null || _musicSource.clip == null || _musicSource.isPlaying)
                return;

            _musicSource.Play();
        }

        public void PlayBowRelease()
        {
            PlayRandom(_bowReleases);
        }

        public void PlayOrcHit()
        {
            PlayOneShot(_orcHit);
        }

        public void PlayOrcDeath()
        {
            PlayRandom(_orcDeaths);
        }

        public void PlayCoins()
        {
            PlayOneShot(_coins);
        }

        public void PlayMerge()
        {
            PlayOneShot(_merge);
        }

        public void PlayUnitBuy()
        {
            PlayOneShot(_unitBuy);
        }

        public void PlayVictory()
        {
            PlayOneShot(_victory);
        }

        public void PlayDefeat()
        {
            PlayOneShot(_defeat);
        }

        void PlayOneShot(AudioClip clip)
        {
            if (_sfxSource == null || clip == null)
                return;

            _sfxSource.PlayOneShot(clip);
        }

        void PlayRandom(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0)
                return;

            var clip = clips[Random.Range(0, clips.Length)];
            PlayOneShot(clip);
        }
    }
}
