using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace Leitner_System_Transfered_2.Model
{
    /// <summary>
    /// Manage operations with decks and cardss
    /// </summary>
    class DeckManager
    {
        //--------------------------------------------------------------------------------
        //------------------------------------- PROPERTIES & FIELDS ---------------------------------
        //--------------------------------------------------------------------------------
        //---------------------------------------PUBLIC---------------------------
        //--------------------------------------------------------------------------------
        public List<Deck> Decks { get; private set; } = new List<Deck>();
        public Deck CurrentDeck { get; private set; } = null;
        public Card CurrentCard { get; private set; } = null;
        //--------------------------------------------------------------------------------
        //---------------------------------------PRIVATE----------------------------------
        //--------------------------------------------------------------------------------
        private List<Card> buffer = new List<Card>();
        private List<Deck> deckBuffer = new List<Deck>();
        //--------------------------------------------------------------------------------
        //---------------------------------------METHODS----------------------------------
        //--------------------------------------------------------------------------------
        /// <summary>
        /// Reload decks from default folder of FileManager
        /// </summary>
        public DeckManager()
        {
            FileManager.UploadSettings();
            ReloadDecksFromFolder();
        }
        /// <summary>
        /// Reload decks from specified folder (default folder if it's empty) in Decks collection, make backup and swithced off selections of card and deck
        /// </summary>
        /// <param name="folderPath">Source folder with decks (default folder if it's empty)</param>
        public void ReloadDecksFromFolder(string folderPath = "")
        {
            Decks = FileManager.GetDecksFromFolder(folderPath);
            CurrentDeck = null;
            CurrentCard = null;
        }
        /// <summary>
        /// Create new deck in Decks collection and save it in current folder with decks, return new Deck
        /// </summary>
        public Deck CreateNewDeck()
        {
            Deck newDeck = new Deck(FileManager.FindNameForNewDeckFolderInCurrentFolderWithDecks());
            Decks.Add(newDeck);
            FileManager.SaveDeckOrUpdateDeckFile(newDeck);
            return newDeck;
        }
        /// <summary>
        /// Delete all selected decks
        /// </summary>
        /// <param name="indexesOfDecksToRemove">List of indexes of decks that sholud be removed</param>
        public void DeleteSelectedDecks(List<int> indexesOfDecksToRemove)
        {
            if (indexesOfDecksToRemove.Count == 0)
            {
                MessageBox.Show("Deck is not choosen");
                return;
            }
            List<Deck> decksToRemove = new List<Deck>();
            foreach (int i in indexesOfDecksToRemove)
                decksToRemove.Add(Decks[i]);
            int removingDeckIndex;

            foreach (Deck deck in decksToRemove)
            {
                removingDeckIndex = Decks.IndexOf(deck);
                FileManager.DeleteDeckFromCurrentFolder(deck.Name);
                Decks.Remove(deck);
            }
            CurrentCard = null;
        }
        /// <summary>
        /// [Optimized] Change CurrentDecks by new index
        /// </summary>
        /// <param name="newCurrentDeckIndex">new index of deck that must be selected</param>
        public void ChangeSelectionOfDeck(int newCurrentDeckIndex)
        {
            if (newCurrentDeckIndex >= 0 && newCurrentDeckIndex < Decks.Count)
                CurrentDeck = Decks[newCurrentDeckIndex];
            else
                CurrentDeck = null;
        }
        /// <summary>
        /// Create new card in selected deck
        /// </summary>
        public void AddCardToCurrentDeck()
        {
            if (CurrentDeck == null)
            {
                MessageBox.Show("Deck is not choosen");
                return;
            }
            CurrentDeck.CreateNewCard();
        }
        /// <summary>
        /// Delete all selected cards from current deck
        /// </summary>
        /// <param name="indexesOfCardsToRemove">list of indexes of cards to delete</param>
        public void DeleteSelectedCardsFromCurrentDeck(List<int> indexesOfCardsToRemove)
        {
            if (CurrentDeck == null)
                return;
            CurrentDeck.DeleteSelectedCard(indexesOfCardsToRemove);
        }
        /// <summary>
        /// Update Current card property by index of new card
        /// </summary>
        /// <param name="newCurrentSelectedCardIndex">index of new card</param>
        public void UpdateSelectionCurrentCard(int newCurrentSelectedCardIndex)
        {
            if (CurrentDeck == null || newCurrentSelectedCardIndex < 0)
            {
                CurrentCard = null;
                newCurrentSelectedCardIndex = -1;
            }
            else if(CurrentDeck.Cards.Count>0 && CurrentDeck.Cards.Count>newCurrentSelectedCardIndex)
            {
                CurrentCard = CurrentDeck.Cards[newCurrentSelectedCardIndex];
            }
            else
            {
                CurrentCard = null;
                newCurrentSelectedCardIndex = -1;
            }
        }
        /// <summary>
        /// Copy cards by its indexes to card's buffer
        /// </summary>
        /// <param name="indexesOfSelectedCards">list of indexes of cards to copy</param>
        public void CopyCardsInBuffer(List<int> indexesOfSelectedCards)
        {
            deckBuffer.Clear();
            buffer.Clear();
            foreach(int index in indexesOfSelectedCards)
                buffer.Add(CurrentDeck.Cards[index]);
        }
        /// <summary>
        /// paste all cards from card's buffer to current deck
        /// </summary>
        public void PasteCardsFromBuffer()
        {
            if (CurrentDeck == null)
                return;
            foreach (Card card in buffer)
                CopyCardToDeck(card, CurrentDeck);
        }
        private void CopyCardToDeck(Card cardToCopy,Deck deck)
        {
            if (cardToCopy == null || deck == null)
                return;
            Card newCard = deck.CreateNewCard();
            string answrAbsolutImagPath = "";
            string qustionAbsolutImagPath = "";
            if (!String.IsNullOrEmpty(cardToCopy.RelativeToDeckFolderQuestionImagePath))
                qustionAbsolutImagPath = Path.Combine(FileManager.currentFolderWithDecksFullPath, cardToCopy.parentDeck.Name, cardToCopy.RelativeToDeckFolderQuestionImagePath);
            if (!String.IsNullOrEmpty(cardToCopy.RelativeToDeckFolderAnswerImagePath))
                answrAbsolutImagPath = Path.Combine(FileManager.currentFolderWithDecksFullPath, cardToCopy.parentDeck.Name, cardToCopy.RelativeToDeckFolderAnswerImagePath);
            newCard.SaveThisCard(cardToCopy.Question, cardToCopy.Answer, qustionAbsolutImagPath, answrAbsolutImagPath);
        }
        /// <summary>
        /// Copy decks by its indexes to deck's buffer
        /// </summary>
        /// <param name="indexesOfSelectedCards">list of indexes of decks to copy</param>
        public void CopyDecksInBuffer(List<int> indexesOfSelectedDecks)
        {
            deckBuffer.Clear();
            buffer.Clear();
            foreach (int index in indexesOfSelectedDecks)
                deckBuffer.Add(Decks[index]);
        }
        /// <summary>
        /// paste all decks from deck's buffer to current deck
        /// </summary>
        public void PasteDecksFromBuffer()
        {
            foreach (Deck deckToCopy in deckBuffer)
            {
                Deck createdDeck = CreateNewDeck();
                createdDeck.Rename(FileManager.FindNameForNewDeckFolderInCurrentFolderWithDecks(deckToCopy.Name + "_Copy"));
                foreach (Card card in deckToCopy.Cards)
                    CopyCardToDeck(card, createdDeck);
            }
        }
        public void ImportExcelFileToCurrentDeck(string absolutePath)
        {
            if (String.IsNullOrEmpty(absolutePath))
                return;
            if (CurrentDeck == null)
            {
                MessageBox.Show("Deck is not choosen is not choosen");
                return;
            }
            Dictionary<string, string> importContent = FileManager.ImportExcelFile(absolutePath);
            foreach (string s in importContent.Keys)
            {
                string question = s;
                string answer = importContent[s];
                Card newCard = CurrentDeck.CreateNewCard();
                newCard.SaveThisCard(question, answer, "", "");
            }
        }
        public void ExportCurrentDeckInExcelFile(string absolutePath)
        {
            if (String.IsNullOrEmpty(absolutePath))
                return;
            if (CurrentDeck == null)
            {
                MessageBox.Show("Deck is not choosen is not choosen");
                return;
            }
            Dictionary<string, string> exportContent = new Dictionary<string, string>();
            foreach(Card card in CurrentDeck.Cards)
            {
                if (!exportContent.Keys.Contains(card.Question))
                    exportContent.Add(card.Question, card.Answer);
            }
            FileManager.ExportExcelFile(absolutePath, exportContent);

        }
    }
}
