namespace Ben.Data
{
    public interface ILocalizable
    {
        object[] LocalizedContext { get; }

        string ToString(Localizer localizer);
    }
}
