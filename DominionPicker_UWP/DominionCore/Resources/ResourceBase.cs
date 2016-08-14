using Windows.ApplicationModel.Resources;

namespace Ben.Dominion.Resources
{
    public class ResourceBase
    {
        private ResourceLoader loader;

        public ResourceBase(string name)
        {
            loader = ResourceLoader.GetForCurrentView(name);
        }

    }
}
