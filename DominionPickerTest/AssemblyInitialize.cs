using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ben.Dominion;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace DominionPickerTest
{
    [TestClass]
    public class AssemblySetup
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext testContext)
        {
            // Force all the cards to load now.
            Cards.Load().Wait();
        }


    }
}
