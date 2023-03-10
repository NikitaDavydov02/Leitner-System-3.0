using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Leitner_System_Transfered_2.Model
{
    /// <summary>
    /// Keep cards and supportive information
    /// </summary>
    [DataContract]
    public class Deck
    {
        //--------------------------------------------------------------------------------
        //------------------------------------- DATA MEMBERS ---------------------------------
        //--------------------------------------------------------------------------------
        [DataMember]
        public bool Compressed { get; private set; } = false;
        [DataMember]
        public List<Card> Cards { get; private set; }
        [DataMember]
        public string Name { get; private set; }
        //--------------------------------------------------------------------------------
        //------------------------------------- METHODS ---------------------------------
        //--------------------------------------------------------------------------------
        /// <summary>
        /// Create new deck with defined name and empty cards
        /// </summary>
        /// <param name="name">Name of this deck</param>
        public Deck(string name)
        {
            Cards = new List<Card>();
            Name = name;
        }
        /// <summary>
        /// Return string with name of this deck
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
        /// <summary>
        /// Set this deck as parent to every child card
        /// </summary>
        public void SetParentDeckToCards()
        {
            foreach (Card card in Cards)
                card.ParentDeck=this;
        }
        /// <summary>
        /// Change name of this deck
        /// </summary>
        /// <param name="newName">New name of this deck</param>
        public void Rename(string newName)
        {
            if (!FileManager.UpdateNameOfDeckDeckFileAndDeckFolder(this, newName))
                return;
            Name = newName;
            FileManager.SaveDeckOrUpdateDeckFile(this);
        }
        /// <summary>
        /// Create new default card in this deck and return it
        /// </summary>
        public Card CreateNewCard()
        {
            Card newCard = new Card(this, "New card", "");
            Cards.Add(newCard);
            return newCard;
        }
        /// <summary>
        /// Delete determined cards from list from this deck and save deck after this
        /// </summary>
        /// <param name="card">Card to delete</param>
        public void DeleteSelectedCard(List<Card> cardsToRemove)
        {
            foreach (Card card in cardsToRemove)
                Cards.Remove(card);
            FileManager.SaveDeckOrUpdateDeckFile(this);
        }
        public void Compress()
        {
            foreach (Card card in Cards)
            {
                if (card.QuestionImageByte == null)
                {
                    string absoluetPathOfQuestionImage = FileManager.PathOfImage(card.RelativeToDeckFolderQuestionImagePath);
                    byte[] questionImageByte = FileManager.ByteFromImageFile(absoluetPathOfQuestionImage);
                    card.QuestionImageByte = questionImageByte;
                }
                if (card.AnswerImageByte == null)
                {
                    string absoluetPathOfAnswerImage = FileManager.PathOfImage(card.RelativeToDeckFolderAnswerImagePath);
                    byte[] answerImageByte = FileManager.ByteFromImageFile(absoluetPathOfAnswerImage);
                    card.AnswerImageByte = answerImageByte;
                }
                card.RelativeToDeckFolderQuestionImagePath = "";
                card.RelativeToDeckFolderAnswerImagePath = "";
            }
            Compressed = true;
        }
        public void Decompress()
        {
            //FileManager.CompressDeck(this);
            
            Compressed = false;
        }
    }
}
