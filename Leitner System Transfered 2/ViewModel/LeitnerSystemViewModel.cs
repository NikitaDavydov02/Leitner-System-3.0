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
        //------------------------------------- METHODS ---------------------------------
        //--------------------------------------------------------------------------------
        /// <summary>
        /// [Optimized] Create new LeitnerSystemViewModel object, reload displaied decks list
        /// </summary>
        public LeitnerSystemViewModel()
        {
            Decks = new ObservableCollection<DeckViewModel>();
            Cards = new ObservableCollection<CardViewModel>();
            DeckManager.Inicializer();
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
            if (DeckManager.Decks == null)
                return;
            foreach (Deck deck in DeckManager.Decks)
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
        private bool ReloadCardList()
        {
            //if (!CheckForUnsavedCardsDoWeContinue())
            //    return false;
            Cards.Clear();
            if (DeckManager.CurrentDeck != null)
            {
                if (DeckManager.CurrentDeck.Cards == null)
                    return true;
                foreach (Card card in DeckManager.CurrentDeck.Cards)
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
            return true;
        }
        public void AddCardToCurrentDeck()
        {
            DeckManager.AddCardsToCurrentDeck();
            if (DeckManager.CurrentDeck != null)
            {
                CardViewModel cardViewModel = new CardViewModel(DeckManager.CurrentDeck.Cards[DeckManager.CurrentDeck.Cards.Count - 1]); ;
                Cards.Add(cardViewModel);
                CurrentDeck.UpdateCount();
            }
        }
        public void AddDeck()
        {
            DeckManager.CreateNewDeck();
            Decks.Add(new DeckViewModel(DeckManager.Decks[DeckManager.Decks.Count - 1]));
        }
        public void ChooseFolder()
        {
            if (!CheckForUnsavedCardsDoWeContinue())
                return;
            string folderPath = "";
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.ShowDialog();
            folderPath = folderBrowserDialog1.SelectedPath;
            DeckManager.ReloadDecksFromFolder(folderPath);
            ReloadDeckList();
            ReloadCardList();
        }
        public void UpdateCurrentSelectedCard(int index)
        {
            DeckManager.UpdateSelectionCurrentCard(index);
            if (DeckManager.CurrentCard != null)
            {
                CurrentCard = Cards[index];
                CurrentCard.LoadImages();
            }
                
            OnPropertyChanged("CurrentCard");
        }
        public bool UpdateCurrentDeckIndex(int index)
        {
            if (index == Decks.IndexOf(CurrentDeck))
                return false;
            if (!CheckForUnsavedCardsDoWeContinue())
                return false;
            DeckManager.ChangeSelectionOfDeck(index);
            ReloadCardList();
            FindCardRequest = "";
            OnPropertyChanged("FindCardRequest");
            if (DeckManager.CurrentDeck != null)
                    CurrentDeck = Decks[index];
            OnPropertyChanged("CurrentDeck");
            return true;
        }
        public void DeleteSelectedCards(List<int> indexesOfSelectedCards)
        {
            if (CurrentDeck == null)
                return;
            List<CardViewModel> cardsViewModelsToRemove = new List<CardViewModel>();
            List<Card> cardsModelsToRemove = new List<Card>();
            foreach (int i in indexesOfSelectedCards)
            {
                cardsViewModelsToRemove.Add(Cards[i]);
                cardsModelsToRemove.Add(Cards[i].Card);
            }
            DeckManager.DeleteSelectedCardsFromCurrentDeck(cardsModelsToRemove);
            foreach (CardViewModel cardViewModel in cardsViewModelsToRemove)
                Cards.Remove(cardViewModel);
            CurrentCard = null;
            OnPropertyChanged("CurrentCard");
            CurrentDeck.UpdateCount();
        }
        private Dictionary<Deck,ReverseSettings> GetDecksForTraining()
        {
            Dictionary<Deck, ReverseSettings> output = new Dictionary<Deck, ReverseSettings>();
            foreach (DeckViewModel deckViewModel in Decks)
                if (deckViewModel.DeckIsSelectedForTraining)
                    output.Add(deckViewModel.Deck,deckViewModel.ReverseSetting);
            return output;
        }
        public TrainingViewModel StartNewTraining()
        {
            if (!CheckForUnsavedCardsDoWeContinue())
                return null;
            Dictionary<Deck, ReverseSettings> decksWithReverseSettings = GetDecksForTraining();
            if (decksWithReverseSettings.Count == 0)
            {
                System.Windows.MessageBox.Show("There is no decks for training. Click on checkboxes corresponding to decks to select deck for training.");
                return null;
            }
            TrainingModel trainingModel = new TrainingModel(GetDecksForTraining());
            if (trainingModel.Results.Count == 0)
            {
                System.Windows.MessageBox.Show("There is no cards for training. Try to repeat tomorrow.");
                return null;
            }
            if (trainingModel == null)
                return null;
            return new TrainingViewModel(trainingModel);
        }
        public void SelectAllDecks()
        {
            bool allDecksAreSelected = true;
            foreach(DeckViewModel deck in Decks)
                if (!deck.DeckIsSelectedForTraining)
                {
                    allDecksAreSelected = false;
                    break;
                }
            foreach (DeckViewModel deckViewModel in Decks)
                deckViewModel.SelectDeck(!allDecksAreSelected);
        }
        public void GeneralReverseSettingsChanged(ReverseSettings newGeneralSettings)
        {
            bool reverseChangingEnable = false;
            if (newGeneralSettings == ReverseSettings.Manual)
                reverseChangingEnable = true;
            foreach (DeckViewModel deckViewModel in Decks)
            {
                if(newGeneralSettings!=ReverseSettings.Manual)
                    deckViewModel.ChangeReverseOfDeck(newGeneralSettings);
                deckViewModel.ChangeReverseChangingEnableOfDeck(reverseChangingEnable);
            }
        }
        public void CopyCardsInBuffer(List<int> indexesOfSelectedCards)
        {
            List<Card> cardsToCopy = new List<Card>();
            foreach (int i in indexesOfSelectedCards)
                cardsToCopy.Add(Cards[i].Card);
            DeckManager.CopyCardsInBuffer(cardsToCopy);
        }
        public void PasteCardsFromBuffer()
        {
            if (CurrentDeck == null)
                return;
            DeckManager.PasteCardsFromBuffer();
            //ReloadCardList();
            for (int i = Cards.Count; i < DeckManager.CurrentDeck.Cards.Count;i++)
                Cards.Add(new CardViewModel(DeckManager.CurrentDeck.Cards[i]));
            CurrentDeck.UpdateCount();
        }
        public void CopyDecksInBuffer(List<int> indexesOfSelectedDecks)
        {
            List<Deck> decksToCopy = new List<Deck>();
            foreach (int i in indexesOfSelectedDecks)
                decksToCopy.Add(Decks[i].Deck);
            DeckManager.CopyDecksInBuffer(decksToCopy);
        }
        public void PasteDecksFromBuffer()
        {
            if (!CheckForUnsavedCardsDoWeContinue())
                return;
            DeckManager.PasteDecksFromBuffer();
            ReloadDeckList();
        }
        public void DeleteSelectedDecks(List<int> indexesOfSelectedDecks)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show("Do you want to delete this deck?", "", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes)
                return;
            List<Deck> decksToDelete = new List<Deck>();
            foreach (int i in indexesOfSelectedDecks)
                decksToDelete.Add(Decks[i].Deck);
                
            DeckManager.DeleteSelectedDecks(decksToDelete);
            ReloadDeckList();
            ReloadCardList();
            CurrentCard = null;
            OnPropertyChanged("CurrentDeck");
        }
        public void SaveSelectedCards(List<int> selectedCardIndexes)
        {
            foreach (int i in selectedCardIndexes)
                Cards[i].SaveThisCard();
        }
        public void TrainingIsFinished()
        {
            DeckManager.ReloadDecksFromFolder();
            ReloadCardList();
            ReloadDeckList();
        }
        public bool CheckForUnsavedCardsDoWeContinue()
        {
            foreach (CardViewModel card in Cards)
                if (card.UnsavedChanges)
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show("У вас есть несохранённые изменения в картах. Вы действительно хотите продолжить?", "", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                        return true;
                    else
                        return false;
                }
            return true;
        }
        public void ImportExcelFileToCurrentDeck(string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
                return;
            if (DeckManager.CurrentDeck == null||!CheckForUnsavedCardsDoWeContinue())
                return;
            DeckManager.ImportExcelFileToCurrentDeck(filePath);
            for (int i = Cards.Count; i < DeckManager.CurrentDeck.Cards.Count; i++)
                Cards.Add(new CardViewModel(DeckManager.CurrentDeck.Cards[i]));
            CurrentDeck.UpdateCount();
        }
        public void ExportCurrentDeckToExcelFile(string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
                return;
            DeckManager.ExportCurrentDeckInExcelFile(filePath);
        }
    }
  }
