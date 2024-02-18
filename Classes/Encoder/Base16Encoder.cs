namespace BetaManager
{
    public class Base16Encoder : BaseEncoder
    {
        public Base16Encoder()
            : base("0123456789ABCDEF".ToCharArray(), false) { }
    }
}
