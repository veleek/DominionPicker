using System;
using System.Diagnostics;
using System.Linq;
using Ben.Dominion;
using Ben.Dominion.Models;
using Ben.Utilities;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace DominionPickerTest
{
    [TestClass]
    public class ViewModelTests
    {
        public static CardList AllCards = CardHelpers.CreateRandomCardList(100);

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            Cards.allCards = new System.Collections.ObjectModel.ReadOnlyCollection<Card>(AllCards);
        }

        [TestMethod]
        public void BlackMarketSerialization()
        {

            BlackMarketViewModel blackMarket = new BlackMarketViewModel
            {
                Deck = Cards.AllCards.ToCardList(),
            };

            blackMarket.Draw();
            blackMarket.Pick(blackMarket.Hand[0]);

            TestSerialization(blackMarket);
        }

        [TestMethod]
        public void ConfigurationSerialization()
        {
            ConfigurationModel configuration = new ConfigurationModel
            {
                
            };
            TestSerialization(configuration);
        }

        [TestMethod]
        public void FavoritesSerialization()
        {
            MainViewModel viewModel = new MainViewModel();
            TestSerialization(viewModel.Favorites);
        }

        [TestMethod]
        public void PickerResultSerialization()
        {
            MainViewModel viewModel = new MainViewModel();
            TestSerialization(viewModel.Result);
        }

        [TestMethod]
        public void SettingsSerialization()
        {
            MainViewModel viewModel = new MainViewModel();
            TestSerialization(viewModel.Settings);
        }

        [TestMethod]
        public void MainViewModelSerialization()
        {
            MainViewModel viewModel = new MainViewModel();
            TestSerialization(viewModel);
        }

        public T TestSerialization<T>(T value)
            where T : class
        {
            string serializedValue = GenericXmlSerializer.Serialize(value);
            Debug.WriteLine($"Serialized Value: {serializedValue}");

            T deserializedValue = GenericXmlSerializer.Deserialize<T>(serializedValue);
            if(value != null)
            {
                Assert.IsNotNull(deserializedValue);
            }

            return deserializedValue;
        }
    }
}
