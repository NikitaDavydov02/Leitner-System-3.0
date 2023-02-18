using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.IO;

namespace Leitner_System_Transfered_2.Model
{
    [DataContract]
    class CardCompressed
    {
        //--------------------------------------------------------------------------------
        //------------------------------------- DATA MEMBERS ---------------------------------
        //--------------------------------------------------------------------------------
        [DataMember]
        public string Question { get;set; }
        [DataMember]
        public string Answer { get; set; }
        [DataMember]
        public byte[] QuestionImageByte { get; set; }
        [DataMember]
        public byte[] AnswerImageByte { get; set; }
        [DataMember]
        public DateTime LastRepetitionTime { get; private set; }
        [DataMember]
        public RepitionFrequensy RepitionFrequensy { get; private set; }
        [DataMember]
        public DateTime LastReverseRepetitionTime { get; private set; }
        [DataMember]
        public RepitionFrequensy ReverseRepitionFrequensy { get; private set; }
        public Deck ParentDeck { get; set; }
        //--------------------------------------------------------------------------------
        //------------------------------------- METHODS ---------------------------------
        //--------------------------------------------------------------------------------
        /// <summary>
        /// Create new Card, attached to the Deck
        /// </summary>
        /// <param name="parentDeck">Deck that is parent to this card</param>
        /// <param name="question">Question of this card</param>
        /// <param name="answer">Answer of this card</param>
        /// <param name="relativeToDeckFolderAnswerImagePath">Relative to deck folder path of answer image</param>
        /// <param name="relativeToDeckFolderQuestionImagePath">Relative to deck folder path of question image</param>
        //public CardCompressed(Deck parentDeck, string question, string answer, string relativeToFoderWithDecksAnswerImagePath = "", string relativeToFoderWithDecksQuestionImagePath = "")
        //{
        //    this.ParentDeck = parentDeck;
        //    Question = question;
        //    Answer = answer;
        //    RelativeToDeckFolderQuestionImagePath = relativeToFoderWithDecksQuestionImagePath;
        //    RelativeToDeckFolderAnswerImagePath = relativeToFoderWithDecksAnswerImagePath;
        //    LastRepetitionTime = DateTime.Now;
        //    LastReverseRepetitionTime = DateTime.Now;
        //    RepitionFrequensy = RepitionFrequensy.Daily;
        //    ReverseRepitionFrequensy = RepitionFrequensy.Daily;
        //}
    }
}
