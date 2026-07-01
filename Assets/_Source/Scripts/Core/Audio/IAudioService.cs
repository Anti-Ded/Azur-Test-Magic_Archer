namespace MagicArcher.Core.Audio
{
    public interface IAudioService
    {
        void PlayMusic();
        void PlayBowRelease();
        void PlayOrcHit();
        void PlayOrcDeath();
        void PlayCoins();
        void PlayMerge();
        void PlayUnitBuy();
        void PlayVictory();
        void PlayDefeat();
    }
}
