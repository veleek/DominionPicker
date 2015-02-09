namespace Ben.Dominion
{
    public class MainModel
    {
        private static MainModel instance;

        public static MainModel Instance => instance ?? (instance = new MainModel());
    }
}