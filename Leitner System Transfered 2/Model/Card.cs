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
    /// <summary>
    /// Keep answer, question, supportive and visual information about card
    /// </summary>
    [DataContract]
    public class Card
    {
        
        //--------------------------------------------------------------------------------
        //------------------------------------- DATA MEMBERS ---------------------------------
        //--------------------------------------------------------------------------------
        [DataMember]
        public string Question { get; private set; }
        [DataMember]
        public string Answer { get; private set; }
        [DataMember]
        public string RelativeToDeckFolderQuestionImagePath { get; private set; }
        [DataMember]
        public string RelativeToDeckFolderAnswerImagePath { get; private set; }
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
        public Card(Deck parentDeck, string question, string answer, string relativeToDeckFolderAnswerImagePath="", string relativeToDeckFolderQuestionImagePath = "")
        {
            this.ParentDeck = parentDeck;
            Question = question;
            Answer = answer;
            RelativeToDeckFolderQuestionImagePath = relativeToDeckFolderQuestionImagePath;
            RelativeToDeckFolderAnswerImagePath = relativeToDeckFolderAnswerImagePath;
            LastRepetitionTime = DateTime.Now;
            LastReverseRepetitionTime = DateTime.Now;
            RepitionFrequensy = RepitionFrequensy.Daily;
            ReverseRepitionFrequensy = RepitionFrequensy.Daily;
        }
        public void SetNewFields(string question, string answer, string relativeToDeckFolderQuestionImagePath, string relativeToDeckFolderAnswerImagePath)
        {
            Question = question;
            Answer = answer;
            RelativeToDeckFolderQuestionImagePath = relativeToDeckFolderQuestionImagePath;
            RelativeToDeckFolderAnswerImagePath = relativeToDeckFolderAnswerImagePath;
        }
        ///<summary>
        ///Update last repitition time by answer 
        ///</summary>
        ///<param name="answer">If the answer is correct or not</param>
        public void UpdateLastRepitionTime(bool answer)
        {
            if (answer && RepitionFrequensy != RepitionFrequensy.OnceAYear)
                RepitionFrequensy++;
            if (!answer)
                RepitionFrequensy = RepitionFrequensy.Daily;
            LastRepetitionTime = DateTime.Now;
        }
        ///<summary>
        ///Update last reverse repitition time by answer 
        ///</summary>
        ///<param name="answer">If the answer is correct or not</param>
        public void UpdateLastReverseRepitionTime(bool answer)
        {
            if (answer && ReverseRepitionFrequensy != RepitionFrequensy.OnceAYear)
                ReverseRepitionFrequensy++;
            if (!answer)
                ReverseRepitionFrequensy = RepitionFrequensy.Daily;
            LastReverseRepetitionTime = DateTime.Now;
        }
        /// <summary>
        /// Return the string described this card
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Question;
        }
        public int DaysSinceCardSholdHaveBeenRepeated(bool straightRepititition)
        {
            int daysSinceCardSholdHaveBeenRepeated = -1;
            if (straightRepititition)
            {
                TimeSpan span = DateTime.Now - LastRepetitionTime;
                if (RepitionFrequensy == RepitionFrequensy.Daily && (span.Days >= 1 || (span.Days == 0 && span.Hours >= 0)))
                    daysSinceCardSholdHaveBeenRepeated = span.Days;
                if (RepitionFrequensy == RepitionFrequensy.EveryTwoDays && span.Days >= 2)
                    daysSinceCardSholdHaveBeenRepeated = span.Days - 2;
                if (RepitionFrequensy == RepitionFrequensy.Weekly && span.Days >= 7)
                    daysSinceCardSholdHaveBeenRepeated = span.Days - 7;
                if (RepitionFrequensy == RepitionFrequensy.Monthly && span.Days >= 30)
                    daysSinceCardSholdHaveBeenRepeated = span.Days - 30;
                if (RepitionFrequensy == RepitionFrequensy.OnceInTwoMonthes && span.Days >= 60)
                    daysSinceCardSholdHaveBeenRepeated = span.Days - 60;
                if (RepitionFrequensy == RepitionFrequensy.OnceInAHalfOfYear && span.Days >= 180)
                    daysSinceCardSholdHaveBeenRepeated = span.Days - 180;
                if (RepitionFrequensy == RepitionFrequensy.OnceAYear && span.Days >= 365)
                    daysSinceCardSholdHaveBeenRepeated = span.Days - 365;
            }
            else
            {
                TimeSpan span = DateTime.Now - LastReverseRepetitionTime;
                if (ReverseRepitionFrequensy == RepitionFrequensy.Daily && (span.Days >= 1 || (span.Days == 0 && span.Hours >= 0)))
                    daysSinceCardSholdHaveBeenRepeated = span.Days;
                if (ReverseRepitionFrequensy == RepitionFrequensy.EveryTwoDays && span.Days >= 2)
                    daysSinceCardSholdHaveBeenRepeated = span.Days - 2;
                if (ReverseRepitionFrequensy == RepitionFrequensy.Weekly && span.Days >= 7)
                    daysSinceCardSholdHaveBeenRepeated = span.Days - 7;
                if (ReverseRepitionFrequensy == RepitionFrequensy.Monthly && span.Days >= 30)
                    daysSinceCardSholdHaveBeenRepeated = span.Days - 30;
                if (ReverseRepitionFrequensy == RepitionFrequensy.OnceInTwoMonthes && span.Days >= 60)
                    daysSinceCardSholdHaveBeenRepeated = span.Days - 60;
                if (ReverseRepitionFrequensy == RepitionFrequensy.OnceInAHalfOfYear && span.Days >= 180)
                    daysSinceCardSholdHaveBeenRepeated = span.Days - 180;
                if (ReverseRepitionFrequensy == RepitionFrequensy.OnceAYear && span.Days >= 365)
                    daysSinceCardSholdHaveBeenRepeated = span.Days - 365;
            }
            return daysSinceCardSholdHaveBeenRepeated;
        }
        public bool CardIsAppropriateForTraining(bool straight)
        {
            if(DaysSinceCardSholdHaveBeenRepeated(straight)>=0)
                return true;
            return false;
        }
    }
    /// <summary>
    /// Repitition intervals
    /// </summary>
    public enum RepitionFrequensy
    {
        Daily = 1,
        EveryTwoDays = 2,
        Weekly = 3,
        Monthly = 4,
        OnceInTwoMonthes=5,
        OnceInAHalfOfYear = 6,
        OnceAYear=7,
    }

}
