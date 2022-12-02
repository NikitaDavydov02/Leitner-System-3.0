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
        public string DeckName { get; set; }
        public bool DeckIsSelectedForTraining { get; set; }
        public bool ReverseSettingChangingEnable { get; private set; }
        public int ReverseSetting { get; set; } = 0;
        private int count;
        public int Count
        {
            get
            {
                return count;
            }
            set
            {
                count = value;
                OnPropertyChanged("Count");
            }
        }
        public Deck Deck { get; private set; }
        public DeckViewModel(Deck deck)
        {
            DeckName = deck.Name;
            Count = deck.Cards.Count;
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
        public void SelectDeck()
        {
            DeckIsSelectedForTraining = true;
            OnPropertyChanged("DeckIsSelectedForTraining");
        }
        public void ChangeReverseOfDeck(int newReverseSettings)
        {
            ReverseSetting = newReverseSettings;
            OnPropertyChanged("ReverseSetting");
        }
        public void ChangeReverseChangingEnableOfDeck(bool newValue)
        {
            ReverseSettingChangingEnable = newValue;
            OnPropertyChanged("ReverseSettingChangingEnable");
        }
        public void SaveRenameThisDeck()
        {
            Deck.Rename(DeckName);
            OnPropertyChanged("DeckName");
        }
    }
}
