namespace MagicArcher.Core.Cta
{
    public interface ICtaService
    {
        bool IsActive { get; }
        void Activate();
        void TryInvoke();
    }
}
