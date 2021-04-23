namespace MHRTalismanManager.Shared
{
    public class TalismanDto : Talisman
    {
        public TalismanOperation Operation { get; set; } = TalismanOperation.Ignore;
    }
}
