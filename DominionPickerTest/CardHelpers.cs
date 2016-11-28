using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ben.Dominion;
using Ben.Dominion.ViewModels;

namespace DominionPickerTest
{
    public class CardHelpers
    {
        private static Random rng = new Random();

        public static Card CreateRandomCard(int? id = null)
        {
            Guid identifier = Guid.NewGuid();

            return new Card
            {
                Name = $"Name_{identifier}",
                DisplayName = $"DisplayName_{identifier}",
                ID = id ?? rng.Next(100),
                Cost = $"{rng.Next(8)}{(rng.NextBool(0.1) ? "P" : "")}",
                Set = rng.NextEnum<CardSet>(),
                Type = rng.NextEnum<CardType>(),
                Label = rng.NextString(15)
            };
        }

        public static CardGroup NextCardGroup()
        {
            return new CardGroup
            {
                
            };
        }

        public static CardList CreateRandomCardList(int count)
        {
            return Enumerable.Range(0, count).Select(i => CreateRandomCard(i)).ToCardList();
        }
    }
}
