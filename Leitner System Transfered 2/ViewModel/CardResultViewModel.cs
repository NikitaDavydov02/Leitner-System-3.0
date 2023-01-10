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
        public Result Result
        {
            get { return result; }
            set 
            {
                result = value;
                OnCardResultChanged(Card,value);
            }
        }
        private Result result;

        public EventHandler CardResultChanged;
        public void OnCardResultChanged(Card card, Result result)
        {
            EventHandler handler = CardResultChanged;
            if (handler != null)
                handler(this, new CardResultChangedEventArgs(card,result));
        }
    }
    public class CardResultChangedEventArgs : EventArgs 
    { 
        public Card Card { get; set; }
        public Result result { get; set; }
        public CardResultChangedEventArgs(Card card, Result result)
        {
            Card = card;
            this.result = result;
        }
    }

}
