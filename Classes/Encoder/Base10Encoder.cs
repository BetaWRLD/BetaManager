namespace BetaManager
{
    public class Base10Encoder : BaseEncoder
    {
        public Base10Encoder()
            : base("0123456789".ToCharArray(), false) { }
    }
}
