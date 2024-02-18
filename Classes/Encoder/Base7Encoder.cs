namespace BetaManager
{
    public class Base7Encoder : BaseEncoder
    {
        public Base7Encoder()
            : base("01234567".ToCharArray(), false) { }
    }
}
