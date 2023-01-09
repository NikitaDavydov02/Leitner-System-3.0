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
    static class DeckManager
    {
        //--------------------------------------------------------------------------------
        //------------------------------------- PROPERTIES & FIELDS ---------------------------------
        //--------------------------------------------------------------------------------
        //---------------------------------------PUBLIC---------------------------
        //--------------------------------------------------------------------------------
        public static List<Deck> Decks { get; private set; } = new List<Deck>();
        public static Deck CurrentDeck { get; private set; } = null;
        public static Card CurrentCard { get; private set; } = null;
        //--------------------------------------------------------------------------------
        //---------------------------------------PRIVATE----------------------------------
        //--------------------------------------------------------------------------------
        private static List<Card> buffer = new List<Card>();
        private static List<Deck> deckBuffer = new List<Deck>();
        //--------------------------------------------------------------------------------
        //---------------------------------------METHODS----------------------------------
        //--------------------------------------------------------------------------------
        /// <summary>
        /// Reload decks from default folder of FileManager
        /// </summary>
        public static void Inicializer()
        {
            FileManager.UploadSettings();
            ReloadDecksFromFolder();
        }
        /// <summary>
        /// Reload decks from specified folder (default folder if it's empty) in Decks collection, make backup and swithced off selections of card and deck
        /// </summary>
        /// <param name="folderPath">Source folder with decks (default folder if it's empty)</param>
        public static void ReloadDecksFromFolder(string folderPath = "")
        {
            Decks = FileManager.GetDecksFromFolder(folderPath);
            CurrentDeck = null;
            CurrentCard = null;
        }
        /// <summary>
        /// Create new deck in Decks collection and save it in current folder with decks, return new Deck
        /// </summary>
        public static Deck CreateNewDeck()
        {
            //FileManager Should find name for deck by itself
            Deck newDeck = new Deck(FileManager.FindNameForNewDeckFolderInCurrentFolderWithDecks());
            Decks.Add(newDeck);
            FileManager.SaveDeckOrUpdateDeckFile(newDeck);
            return newDeck;
        }
        /// <summary>
        /// Delete all selected decks
        /// </summary>
        /// <param name="indexesOfDecksToRemove">List of indexes of decks that sholud be removed</param>
        public static void DeleteSelectedDecks(List<int> indexesOfDecksToRemove)
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
        public static void ChangeSelectionOfDeck(int newCurrentDeckIndex)
        {
            if (newCurrentDeckIndex >= 0 && newCurrentDeckIndex < Decks.Count)
                CurrentDeck = Decks[newCurrentDeckIndex];
            else
                CurrentDeck = null;
        }
        /// <summary>
        /// Create new card in selected deck
        /// </summary>
        public static void AddCardToCurrentDeck()
        {
            if (CurrentDeck == null)
            {
                MessageBox.Show("Deck is not choosen");
                return;
            }
            CurrentDeck.CreateNewCard();
            FileManager.SaveDeckOrUpdateDeckFile(CurrentDeck);
        }
        /// <summary>
        /// Delete all selected cards from current deck
        /// </summary>
        /// <param name="indexesOfCardsToRemove">list of indexes of cards to delete</param>
        public static void DeleteSelectedCardsFromCurrentDeck(List<int> indexesOfCardsToRemove)
        {
            if (CurrentDeck == null)
                return;
            CurrentDeck.DeleteSelectedCard(indexesOfCardsToRemove);
        }
        /// <summary>
        /// Update Current card property by index of new card
        /// </summary>
        /// <param name="newCurrentSelectedCardIndex">index of new card</param>
        public static void UpdateSelectionCurrentCard(int newCurrentSelectedCardIndex)
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
        public static void CopyCardsInBuffer(List<int> indexesOfSelectedCards)
        {
            deckBuffer.Clear();
            buffer.Clear();
            foreach(int index in indexesOfSelectedCards)
                buffer.Add(CurrentDeck.Cards[index]);
        }
        /// <summary>
        /// paste all cards from card's buffer to current deck
        /// </summary>
        public static void PasteCardsFromBuffer()
        {
            if (CurrentDeck == null)
                return;
            foreach (Card card in buffer)
                CopyCardToDeck(card, CurrentDeck);
        }
        public static void SaveCards(List<Card> cards, List<string> question, List<string> answer, List<string> absoluteQuestionImagePath, List<string> absoluteAnswerImagePath)
        {
            //Cards fromthe same deck!!!!
            if (cards.Count == 0)
                return;
            for(int i = 0; i < cards.Count; i++)
            {
                string relativeToDeckFolderQuestionImagePath = "";
                string relativeToDeckFolderAnswerImagePath = "";
                if (!String.IsNullOrEmpty(absoluteAnswerImagePath[i]) || !String.IsNullOrEmpty(absoluteQuestionImagePath[i]))
                    FileManager.CopyNewImagesToDeckFolder(cards[i], absoluteQuestionImagePath[i], absoluteAnswerImagePath[i], out relativeToDeckFolderAnswerImagePath, out relativeToDeckFolderQuestionImagePath);
                cards[i].SetNewFields(question[i], answer[i], relativeToDeckFolderQuestionImagePath, relativeToDeckFolderAnswerImagePath);

                //dsdvs
            }
            FileManager.SaveDeckOrUpdateDeckFile(cards[0].parentDeck);
        }
        private static void CopyCardToDeck(Card cardToCopy,Deck deck)
        {
            if (cardToCopy == null || deck == null)
                return;
            Card newCard = deck.CreateNewCard();
            FileManager.SaveDeckOrUpdateDeckFile(deck);
            string answrAbsolutImagPath = "";
            string qustionAbsolutImagPath = "";
            if (!String.IsNullOrEmpty(cardToCopy.RelativeToDeckFolderQuestionImagePath))
                qustionAbsolutImagPath = Path.Combine(FileManager.currentFolderWithDecksFullPath, cardToCopy.parentDeck.Name, cardToCopy.RelativeToDeckFolderQuestionImagePath);
            if (!String.IsNullOrEmpty(cardToCopy.RelativeToDeckFolderAnswerImagePath))
                answrAbsolutImagPath = Path.Combine(FileManager.currentFolderWithDecksFullPath, cardToCopy.parentDeck.Name, cardToCopy.RelativeToDeckFolderAnswerImagePath);
            SaveCards(new List<Card>() { newCard}, new List<string>() { cardToCopy.Question }, new List<string>() { cardToCopy.Answer }, new List<string>() { qustionAbsolutImagPath }, new List<string>() { answrAbsolutImagPath });
        }
        /// <summary>
        /// Copy decks by its indexes to deck's buffer
        /// </summary>
        /// <param name="indexesOfSelectedCards">list of indexes of decks to copy</param>
        public static void CopyDecksInBuffer(List<int> indexesOfSelectedDecks)
        {
            deckBuffer.Clear();
            buffer.Clear();
            foreach (int index in indexesOfSelectedDecks)
                deckBuffer.Add(Decks[index]);
        }
        /// <summary>
        /// paste all decks from deck's buffer to current deck
        /// </summary>
        public static void PasteDecksFromBuffer()
        {
            foreach (Deck deckToCopy in deckBuffer)
            {
                Deck createdDeck = CreateNewDeck();
                createdDeck.Rename(FileManager.FindNameForNewDeckFolderInCurrentFolderWithDecks(deckToCopy.Name + "_Copy"));
                foreach (Card card in deckToCopy.Cards)
                    CopyCardToDeck(card, createdDeck);
            }
        }
        public static void ImportExcelFileToCurrentDeck(string absolutePath)
        {
            if (String.IsNullOrEmpty(absolutePath))
                return;
            if (CurrentDeck == null)
            {
                MessageBox.Show("Deck is not choosen is not choosen");
                return;
            }
            Dictionary<string, string> importContent = FileManager.ImportExcelFile(absolutePath);
            List<Card> importedCards = new List<Card>();
            List<string> questionImagesPaths = new List<string>();

            for(int a=0;a<importContent.Keys.Count;a++)
            {
                importedCards.Add(CurrentDeck.CreateNewCard());
                questionImagesPaths.Add("");
                //newCard.SaveThisCard(question, answer, "", "");
            }
            SaveCards(importedCards, importContent.Keys.ToList<string>(), importContent.Values.ToList<string>(), questionImagesPaths, questionImagesPaths);
        }
        public static void ExportCurrentDeckInExcelFile(string absolutePath)
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
