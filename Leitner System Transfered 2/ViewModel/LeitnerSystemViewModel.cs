using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Leitner_System_Transfered_2.Model;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.IO;
using System.Windows;

namespace Leitner_System_Transfered_2.ViewModel
{
    public class LeitnerSystemViewModel: INotifyPropertyChanged
    {
        //--------------------------------------------------------------------------------
        //------------------------------------- PUBLIC ---------------------------------
        //--------------------------------------------------------------------------------
        public ObservableCollection<DeckViewModel> Decks { get; private set; }
        public ObservableCollection<CardViewModel> Cards { get; private set; }
        //public string CurrentDeckName { get; set; }
        public DeckViewModel CurrentDeck { get; private set; }
        public CardViewModel CurrentCard
        {
            get;
            private set;
        }
        private string findCardRequest = "";
        public string FindCardRequest
        {
            get { return findCardRequest; }
            set
            {
                findCardRequest = value;
                ReloadCardList();
            }
        }
        //--------------------------------------------------------------------------------
        //------------------------------------- PRIVATE ---------------------------------
        //--------------------------------------------------------------------------------
        private DeckManager model;
        //--------------------------------------------------------------------------------
        //------------------------------------- METHODS ---------------------------------
        //--------------------------------------------------------------------------------
        /// <summary>
        /// [Optimized] Create new LeitnerSystemViewModel object, reload displaied decks list
        /// </summary>
        public LeitnerSystemViewModel()
        {
            Decks = new ObservableCollection<DeckViewModel>();
            Cards = new ObservableCollection<CardViewModel>();
            model = new DeckManager();
            //model.DeckAdd += AddDeckHandler;
            //model.DeckRemove += RemoveDeckHandler;
            //model.DecksReload += DecksReloadHandler;
            ReloadDeckList();
            ReloadCardList();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// Update displaied deck list
        /// </summary>
        private void ReloadDeckList()
        {
            Decks.Clear();
            if (model.Decks == null)
                return;
            foreach (Deck deck in model.Decks)
                Decks.Add(new DeckViewModel(deck));
            Cards.Clear();
            CurrentCard = null;
            CurrentDeck = null;
            FindCardRequest = "";
            OnPropertyChanged("FindCardRequest");
            OnPropertyChanged("CurrentCard");
            OnPropertyChanged("CurrentDeck");
        }
        /// <summary>
        /// [Optimozed] Update displaied card list from the model.CurrentDeck if it is possible
        /// </summary>
        /// 
        private void ReloadCardList()
        {
            Cards.Clear();
            if (model.CurrentDeck != null)
            {
                if (model.CurrentDeck.Cards == null)
                    return;
                foreach (Card card in model.CurrentDeck.Cards)
                {
                    if (card.Question.Contains(FindCardRequest) || card.Answer.Contains(FindCardRequest)||String.IsNullOrEmpty(FindCardRequest))
                    {
                        CardViewModel cardViewModel = new CardViewModel(card);
                        Cards.Add(cardViewModel);
                    }
                }
            }
            CurrentCard = null;
            OnPropertyChanged("CurrentCard");
        }
        /// <summary>
        /// Update fields of current displaied card
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void CurrentSelectedCardFieldsChangedHandler(object sender, EventArgs args)
        {
            OnPropertyChanged("CurrentCard");
            if (CurrentCard == null)
                return;
            CurrentCard.OnPropertyChanged("Question");
            CurrentCard.OnPropertyChanged("Answer");
        }
        public void AddCardToCurrentDeck()
        {
            model.AddCardToCurrentDeck();
            if (model.CurrentDeck != null)
            {
                CardViewModel cardViewModel = new CardViewModel(model.CurrentDeck.Cards[model.CurrentDeck.Cards.Count - 1]); ;
                Cards.Add(cardViewModel);
                CurrentDeck.Count = model.CurrentDeck.Cards.Count;
            }
        }
        public void AddDeck()
        {
            model.CreateNewDeck();
            Decks.Add(new DeckViewModel(model.Decks[model.Decks.Count - 1]));
        }
        public void ChooseFolder()
        {
            if (CheckForUnsavedCards())
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("У вас есть несохранённые изменения в картах. Вы действительно хотите продолжить?", "", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes)
                    return;
            }
            string folderPath = "";
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.ShowDialog();
            folderPath = folderBrowserDialog1.SelectedPath;
            model.ReloadDecksFromFolder(folderPath);
            ReloadDeckList();
            ReloadCardList();
        }
        public void UpdateCurrentSelectedCard(int index)
        {
            model.UpdateSelectionCurrentCard(index);
            if (model.CurrentCard != null)
            {
                CurrentCard = Cards[index];
                CurrentCard.LoadImages();
            }
            OnPropertyChanged("CurrentCard");
        }
        public void UpdateCurrentDeckIndex(int index)
        {
            if (CheckForUnsavedCards())
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("У вас есть несохранённые изменения в картах. Вы действительно хотите продолжить?", "", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes)
                    return;
            }
            model.ChangeSelectionOfDeck(index);
            ReloadCardList();
            FindCardRequest = "";
            OnPropertyChanged("FindCardRequest");
            if (model.CurrentDeck != null)
                    CurrentDeck = Decks[index];
            OnPropertyChanged("CurrentDeck");
        }
        public void DeleteSelectedCards(List<int> indexesOfSelectedCards)
        {
            if (CurrentDeck == null)
                return;
            List<CardViewModel> cardsToRemove = new List<CardViewModel>();
            foreach (int i in indexesOfSelectedCards)
                cardsToRemove.Add(Cards[i]);
            model.DeleteSelectedCardsFromCurrentDeck(indexesOfSelectedCards);
            //foreach (CardViewModel card in cardsToRemove)
            //    Cards.Remove(card);
            //CurrentDeck.Count = model.CurrentDeck.Cards.Count;
            ReloadCardList();
            CurrentDeck.Count = model.CurrentDeck.Cards.Count;
        }
        private Dictionary<Deck,int> GetDecksForTraining()
        {
            Dictionary<Deck, int> output = new Dictionary<Deck, int>();
            foreach (DeckViewModel deckViewModel in Decks)
                if (deckViewModel.DeckIsSelectedForTraining)
                    output.Add(deckViewModel.Deck,deckViewModel.ReverseSetting);
            return output;
        }
        public TrainingViewModel StartNewTraining()
        {
            if (CheckForUnsavedCards())
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("У вас есть несохранённые изменения в картах. Вы действительно хотите продолжить?", "", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes)
                    return null;
            }
            TrainingModel trainingModel = new TrainingModel(GetDecksForTraining());
            if (trainingModel == null)
                return null;
            return new TrainingViewModel(trainingModel);
        }
        public void SelectAllDecks()
        {
            foreach (DeckViewModel deckViewModel in Decks)
                deckViewModel.SelectDeck();
        }
        public void GeneralReverseSettingsChanged(int newGeneralSettings)
        {
            switch (newGeneralSettings)
            {
                case 0:
                    foreach (DeckViewModel deckViewModel in Decks)
                    {
                        deckViewModel.ChangeReverseOfDeck(0);
                        deckViewModel.ChangeReverseChangingEnableOfDeck(false);
                    }
                    break;
                case 1:
                    foreach (DeckViewModel deckViewModel in Decks)
                    {
                        deckViewModel.ChangeReverseOfDeck(1);
                        deckViewModel.ChangeReverseChangingEnableOfDeck(false);
                    }
                    break;
                case 2:
                    foreach (DeckViewModel deckViewModel in Decks)
                    {
                        deckViewModel.ChangeReverseOfDeck(2);
                        deckViewModel.ChangeReverseChangingEnableOfDeck(false);
                    }
                    break;
                case 3:
                    foreach (DeckViewModel deckViewModel in Decks)
                    {
                        deckViewModel.ChangeReverseChangingEnableOfDeck(true);
                    }
                    break;
            }
        }
        public void CopyCardsInBuffer(List<int> indexesOfSelectedCards)
        {
            model.CopyCardsInBuffer(indexesOfSelectedCards);
        }
        public void PasteCardsFromBuffer()
        {
            if (CurrentDeck == null)
                return;
            model.PasteCardsFromBuffer();
            ReloadCardList();
            for (int i = Cards.Count; i < model.CurrentDeck.Cards.Count;i++)
                Cards.Add(new CardViewModel(model.CurrentDeck.Cards[i]));
            CurrentDeck.Count = model.CurrentDeck.Cards.Count;
        }
        public void CopyDecksInBuffer(List<int> indexesOfSelectedDecks)
        {
            model.CopyDecksInBuffer(indexesOfSelectedDecks);
        }
        public void PasteDecksFromBuffer()
        {
            model.PasteDecksFromBuffer();
            ReloadDeckList();
        }
        public void DeleteSelectedDecks(List<int> indexesOfSelectedDecks)
        {
            if (CheckForUnsavedCards())
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("У вас есть несохранённые изменения в картах. Вы действительно хотите продолжить?", "", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes)
                    return;
            }
            model.DeleteSelectedDecks(indexesOfSelectedDecks);
            ReloadDeckList();
            ReloadCardList();
            OnPropertyChanged("CurrentDeck");
        }
        public void SaveSelectedCards(List<int> selectedCardIndexes)
        {
            foreach (int i in selectedCardIndexes)
                Cards[i].SaveThisCard();
        }
        public void TrainingIsFinished()
        {
            model.ReloadDecksFromFolder();
            ReloadCardList();
            ReloadDeckList();
        }
        public bool CheckForUnsavedCards()
        {
            foreach (CardViewModel card in Cards)
                if (card.UnsavedChanges)
                    return true;
            return false;
        }
        public void UpdateCardListByCardFindRequest(string request)
        {
            //ReloadCardList(request);
        }
        public void ImportExcelFileToCurrentDeck(string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
                return;
            if (model.CurrentDeck == null)
                return;
            model.ImportExcelFileToCurrentDeck(filePath);
            ReloadCardList();
            CurrentDeck.Count = model.CurrentDeck.Cards.Count;
        }
        public void ExportCurrentDeckToExcelFile(string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
                return;
            model.ExportCurrentDeckInExcelFile(filePath);
        }
    }
  }
