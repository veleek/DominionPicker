using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Ben.Dominion;
using System.Linq;
using System.Collections.Generic;

namespace DominionPickerTests
{
    [TestClass]
    public class PickerTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void MinimumCardsPerSetAllowsFewerSetsThanMaxiumAllowed()
        {
            SettingsViewModel settings = new SettingsViewModel
            {
                MinimumCardsPerSet =
                {
                    Enabled = true,
                    OptionValue = 5,
                },
                SelectedSets = Cards.AllSets.ToList(),
                
            };

            Picker picker = new Picker();

            Dictionary<int, int> counts = new Dictionary<int, int>();
            for (int i = 0; i < 1000; i++)
            {
                var list = picker.GenerateCardList(settings);
                int setCount = list.Cards.GroupBy(c => c.Set).Count();

                int timesOccurred = 0;
                counts.TryGetValue(setCount, out timesOccurred);
                counts[setCount] = timesOccurred+1;
            }

            foreach (var set in counts)
            {
                Console.WriteLine("Sets in Result: {0}, Times Occurred: {1}", set.Key, set.Value);
                Assert.AreNotEqual(0, set.Value, string.Format("Result with {0} sets never occurred", set.Key));
            }

            Assert.Fail("So I can see output");
        }
    }
}
