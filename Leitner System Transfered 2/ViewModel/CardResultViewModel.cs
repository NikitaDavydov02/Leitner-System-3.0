using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leitner_System_Transfered_2.Model;

namespace Leitner_System_Transfered_2.ViewModel
{
    public class CardResultViewModel
    {
        public string Name { get; set; }
        public Card Card { get; set; }
        public int Result
        {
            get { return result; }
            set 
            {
                result = value;
                OnCardResultChanged(Card,value);
            }
        }
        private int result;

        public EventHandler CardResultChanged;
        public void OnCardResultChanged(Card card, int indexOfResult)
        {
            EventHandler handler = CardResultChanged;
            if (handler != null)
                handler(this, new CardResultChangedEventArgs(card,indexOfResult));
        }
    }
    public class CardResultChangedEventArgs : EventArgs 
    { 
        public Card Card { get; set; }
        public int IndexOfResult { get; set; }
        public CardResultChangedEventArgs(Card card, int indexOfResult)
        {
            Card = card;
            IndexOfResult = indexOfResult;
        }
    }

}
