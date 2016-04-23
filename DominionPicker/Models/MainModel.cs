namespace Ben.Dominion
{
    public class MainModel
    {
        private static MainModel instance;

        public static MainModel Instance { get { return instance ?? (instance = new MainModel()); } }
    }
}