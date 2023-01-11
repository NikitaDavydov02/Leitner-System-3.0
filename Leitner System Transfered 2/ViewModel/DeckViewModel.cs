using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Leitner_System_Transfered_2.Model;
using System.ComponentModel;

namespace Leitner_System_Transfered_2.ViewModel
{
    public class DeckViewModel:INotifyPropertyChanged
    {
        private string deckName;
        public string DeckName
        {
            get { return Deck.Name; }
            set { deckName = value; }
        }
        
        public bool DeckIsSelectedForTraining { get; set; }
        public bool ReverseSettingChangingEnable { get; private set; }
        public ReverseSettings ReverseSetting { get; set; } = 0;
        public int Count { get { return Deck.Cards.Count; }
        }
        public Deck Deck { get; private set; }
        public DeckViewModel(Deck deck)
        {
            DeckName = deck.Name;
            DeckIsSelectedForTraining = false;
            ReverseSettingChangingEnable = false; ;
            ReverseSetting = 0;
            this.Deck = deck;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return DeckName;
        }
        public void SelectDeck(bool value)
        {
            DeckIsSelectedForTraining = value;
            OnPropertyChanged("DeckIsSelectedForTraining");
        }
        public void ChangeReverseOfDeck(ReverseSettings reverseSettings)
        {
            ReverseSetting = reverseSettings;
            OnPropertyChanged("ReverseSetting");
        }
        public void ChangeReverseChangingEnableOfDeck(bool newValue)
        {
            ReverseSettingChangingEnable = newValue;
            OnPropertyChanged("ReverseSettingChangingEnable");
        }
        public void SaveRenameThisDeck()
        {
            Deck.Rename(deckName);
            //DeckName = Deck.Name;
            OnPropertyChanged("DeckName");
        }
        public void UpdateCount()
        {
            OnPropertyChanged("Count");
        }
    }
}
