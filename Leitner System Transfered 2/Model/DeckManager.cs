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
            foreach (Deck deck in Decks)
                deck.SetParentDeckToCards();
            CurrentDeck = null;
            CurrentCard = null;
        }
        /// <summary>
        /// Create new deck in Decks collection and save it in current folder with decks, return new Deck
        /// </summary>
        public static Deck CreateNewDeck()
        {
            //FileManager Should find name for deck by itself
            Deck newDeck = new Deck(FileManager.FindNameForNewDeckInCurrentFolderWithDecks());
            Decks.Add(newDeck);
            FileManager.SaveDeckOrUpdateDeckFile(newDeck);
            return newDeck;
        }
        /// <summary>
        /// Delete all selected decks
        /// </summary>
        /// <param name="indexesOfDecksToRemove">List of indexes of decks that sholud be removed</param>
        public static void DeleteSelectedDecks(List<Deck> decksToRemove)
        {
            if (decksToRemove.Count==0)
            {
                MessageBox.Show("Deck is not choosen");
                return;
            }
            foreach (Deck deck in decksToRemove)
            {
                FileManager.DeleteDeckFromCurrentFolder(deck);
                Decks.Remove(deck);
            }
            CurrentCard = null;
            CurrentDeck = null;
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
        public static void AddCardsToCurrentDeck(int n = 1)
        {
            if (CurrentDeck == null)
            {
                MessageBox.Show("Deck is not choosen");
                return;
            }
            for(int i=0;i<n;i++)
                CurrentDeck.CreateNewCard();
            FileManager.SaveDeckOrUpdateDeckFile(CurrentDeck);
        }
        /// <summary>
        /// Delete all selected cards from current deck
        /// </summary>
        /// <param name="indexesOfCardsToRemove">list of indexes of cards to delete</param>
        public static void DeleteSelectedCardsFromCurrentDeck(List<Card>cardsToRemove)
        {
            if (CurrentDeck == null)
                return;
            CurrentDeck.DeleteSelectedCard(cardsToRemove);
        }
        /// <summary>
        /// Update Current card property by index of new card
        /// </summary>
        /// <param name="newCurrentSelectedCardIndex">index of new card</param>
        public static void UpdateSelectionCurrentCard(int newCurrentSelectedCardIndex)
        {
            if (CurrentDeck == null || newCurrentSelectedCardIndex < 0)
                CurrentCard = null;
            else if(CurrentDeck.Cards.Count>0 && CurrentDeck.Cards.Count>newCurrentSelectedCardIndex)
                CurrentCard = CurrentDeck.Cards[newCurrentSelectedCardIndex];
            else
                CurrentCard = null;
        }
        /// <summary>
        /// Copy cards by its indexes to card's buffer
        /// </summary>
        /// <param name="indexesOfSelectedCards">list of indexes of cards to copy</param>
        public static void CopyCardsInBuffer(List<Card> selectedCards)
        {
            deckBuffer.Clear();
            buffer.Clear();
            foreach(Card card in selectedCards)
                buffer.Add(card);
        }
        /// <summary>
        /// paste all cards from card's buffer to current deck
        /// </summary>
        public static void PasteCardsFromBuffer()
        {
            if (CurrentDeck == null)
                return;
            CopyCardsToDeck(buffer, CurrentDeck);
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
                //cards[i].SetNewFields(question[i], answer[i], relativeToDeckFolderQuestionImagePath, relativeToDeckFolderAnswerImagePath);
                cards[i].Question = question[i];
                cards[i].Answer = answer[i];
                cards[i].RelativeToDeckFolderQuestionImagePath = relativeToDeckFolderQuestionImagePath;
                cards[i].RelativeToDeckFolderAnswerImagePath = relativeToDeckFolderAnswerImagePath;
                //dsdvs
            }
            FileManager.SaveDeckOrUpdateDeckFile(cards[0].ParentDeck);
        }
        private static void CopyCardsToDeck(List<Card> cardsToCopy,Deck deck)
        {
            if (cardsToCopy == null || deck == null||cardsToCopy.Count==0)
                return;
            foreach(Card cardToCopy in cardsToCopy)
            {
                Card newCard = deck.CreateNewCard();
                string answrAbsolutImagPath = "";
                string qustionAbsolutImagPath = "";
                if (!String.IsNullOrEmpty(cardToCopy.RelativeToDeckFolderQuestionImagePath))
                    qustionAbsolutImagPath = Path.Combine(FileManager.currentFolderWithDecksFullPath, cardToCopy.RelativeToDeckFolderQuestionImagePath);
                if (!String.IsNullOrEmpty(cardToCopy.RelativeToDeckFolderAnswerImagePath))
                    answrAbsolutImagPath = Path.Combine(FileManager.currentFolderWithDecksFullPath, cardToCopy.RelativeToDeckFolderAnswerImagePath);
                SaveCards(new List<Card>() { newCard }, new List<string>() { cardToCopy.Question }, new List<string>() { cardToCopy.Answer }, new List<string>() { qustionAbsolutImagPath }, new List<string>() { answrAbsolutImagPath });
                
            }
             FileManager.SaveDeckOrUpdateDeckFile(deck);
        }
        /// <summary>
        /// Copy decks by its indexes to deck's buffer
        /// </summary>
        /// <param name="indexesOfSelectedCards">list of indexes of decks to copy</param>
        public static void CopyDecksInBuffer(List<Deck> decksToCopy)
        {
            deckBuffer.Clear();
            buffer.Clear();
            foreach (Deck deck in decksToCopy)
                deckBuffer.Add(deck);
        }
        /// <summary>
        /// paste all decks from deck's buffer to current deck
        /// </summary>
        public static void PasteDecksFromBuffer()
        {
            foreach (Deck deckToCopy in deckBuffer)
            {
                Deck createdDeck = CreateNewDeck();
                createdDeck.Rename(FileManager.FindNameForNewDeckInCurrentFolderWithDecks(deckToCopy.Name + "_Copy"));
                CopyCardsToDeck(deckToCopy.Cards, CurrentDeck);
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
        public static void DecompressDeck(string pathOfFile)
        {
           Deck decompressedDeck = FileManager.DecompressDeck(pathOfFile);
           Decks.Add(decompressedDeck);
           FileManager.SaveDeckOrUpdateDeckFile(decompressedDeck);
        }
        public static void CompressCurrentDeck(string pathOfFile)
        {
            if (String.IsNullOrEmpty(pathOfFile))
                return;
            if (CurrentDeck == null)
            {
                MessageBox.Show("Deck is not choosen is not choosen");
                return;
            }
            Deck compressingDeck = new Deck(CurrentDeck.Name);
            foreach(Card card in CurrentDeck.Cards)
            {
                compressingDeck.CreateNewCard();
                Card newCard = compressingDeck.Cards[compressingDeck.Cards.Count - 1];
                newCard.Question = card.Question;
                newCard.Answer = card.Answer;
                newCard.RelativeToDeckFolderAnswerImagePath = card.RelativeToDeckFolderAnswerImagePath;
                newCard.RelativeToDeckFolderQuestionImagePath = card.RelativeToDeckFolderQuestionImagePath;
                newCard.AnswerImageByte = card.AnswerImageByte;
                newCard.QuestionImageByte = card.QuestionImageByte;
            }
            compressingDeck.Compress();
            FileManager.SaveDeckOrUpdateDeckFile(compressingDeck, pathOfFile);
        }
    }
}
