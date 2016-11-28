using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;

namespace Ben.Dominion
{
    [XmlRoot("Cards")]
    public class CardList
       : ObservableCollection<Card>, IXmlSerializable
    {

        public CardList()
        {
        }

        public CardList(IEnumerable<Card> collection)
        : base(collection)
        {
        }

        public CardList(List<Card> list)
        : base(list)
        {
        }

        public List<int> Ids
        {
            get
            {
                return this.Select(c => c.ID).ToList();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<string> Names
        {
            get
            {
                return this.Select(c => c.Name).ToList();
            }
        }

        public Card Draw()
        {
            Card c = null;
            if (this.Count > 0)
            {
                c = this[0];
                this.RemoveAt(0);
            }
            return c;
        }

        public CardList Draw(int count)
        {
            CardList cards = new CardList();
            for (int i = 0; i < count; i++)
            {
                cards.Add(this.Draw());
            }
            return cards;
        }

        public void AddRange(IEnumerable<Card> cards)
        {
            foreach (Card card in cards)
            {
                this.Add(card);
            }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            this.Clear();
            var content = reader.ReadElementContentAsString();
            var cardIds = content.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (cardIds.Length == 0)
            {
                return;
            }
            foreach (var cardId in cardIds)
            {
                int id = int.Parse(cardId);
                this.Add(Card.FromId(id));
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteValue(this.Ids.Aggregate("", (a, b) => a + b + ",", s => s.TrimEnd(',')));
        }

    }

    public static class CardListExtensions
    {
        public static CardList ToCardList(this IEnumerable<Card> cards)
        {
            return new CardList(cards);
        }
    }
}