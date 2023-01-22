using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Leitner_System_Transfered_2.Model;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Leitner_System_Transfered_2.ViewModel
{
    public class CardViewModel:INotifyPropertyChanged
    {
        //--------------------------------------------------------------------------------
        //------------------------------------- PUBLIC ---------------------------------
        //--------------------------------------------------------------------------------
        public DateTime LastRepititionTime { get { return Card.LastRepetitionTime; } }
        public RepitionFrequensy RepitionFrequensy { get { return Card.RepitionFrequensy; } }
        public DateTime LastReverseRepititionTime { get { return Card.LastReverseRepetitionTime; } }
        public RepitionFrequensy ReverseRepitionFrequensy { get { return Card.ReverseRepitionFrequensy; } }
        public bool AnswerIsVisible
        {
            get { return answerIsVisible; }
            set { answerIsVisible = value;
                OnPropertyChanged("AnswerIsVisible");
            }
        }
        private bool answerIsVisible=true;
        public string NameOfCard
        {
            get
            {
                string output = Question;
                if (UnsavedChanges)
                    output = "*" + output;
                return output;
            }
        }
        public bool UnsavedChanges
        {
            get
            {
                string cardAbsoluteAnswerImagePath = Path.Combine(FileManager.currentFolderWithDecksFullPath, Card.RelativeToDeckFolderAnswerImagePath);
                if (String.IsNullOrEmpty(Card.RelativeToDeckFolderAnswerImagePath))
                    cardAbsoluteAnswerImagePath = Path.Combine(FileManager.currentFolderWithDecksFullPath);
                string cardAbsoluteQuestionImagePath = Path.Combine(FileManager.currentFolderWithDecksFullPath, Card.RelativeToDeckFolderQuestionImagePath);
                if (String.IsNullOrEmpty(Card.RelativeToDeckFolderQuestionImagePath))
                    cardAbsoluteQuestionImagePath = Path.Combine(FileManager.currentFolderWithDecksFullPath);
                if ((!straightOrReverse && Question == Card.Question &&
                Answer == Card.Answer &&
                AbsoluteQuestionImagePath == cardAbsoluteQuestionImagePath &&
                AbsoluteAnswerImagePath == cardAbsoluteAnswerImagePath) || (straightOrReverse && Question == Card.Answer &&
                Answer == Card.Question &&
                AbsoluteAnswerImagePath == cardAbsoluteQuestionImagePath &&
                AbsoluteQuestionImagePath == cardAbsoluteAnswerImagePath))
                    return false;
                else
                    return true;
            }
        }
        public Card Card { get; private set; }
        public string Question
        {
            get
            {
                return question;
            }
            set
            {
                question = value;
                OnPropertyChanged("Question");
                OnPropertyChanged("NameOfCard");
            }
        }
        private string question;
        public string Answer
        {
            get
            {
                return answer;
            }
            set
            {
                answer = value;
                OnPropertyChanged("Answer");
                OnPropertyChanged("NameOfCard");
            }
        }
        private string answer;

        private string AbsoluteQuestionImagePath;
        private string AbsoluteAnswerImagePath;
        public BitmapImage QuestionImage { get; private set; }
        public BitmapImage AnswerImage { get; private set; }
        private bool straightOrReverse;
        //--------------------------------------------------------------------------------
        //------------------------------------- METHODS ---------------------------------
        //--------------------------------------------------------------------------------
        public CardViewModel(Card card, bool straightOrReverse = false)
        {
            MakeCardViewModelFromCardModel(card, straightOrReverse);
        }
        private void MakeCardViewModelFromCardModel(Card card, bool straightOrReverse = false)
        {
            this.Card = card;
            this.straightOrReverse = straightOrReverse;

            if (!straightOrReverse)
            {
                Question = card.Question;
                Answer = card.Answer;
                AbsoluteQuestionImagePath = Path.Combine(FileManager.currentFolderWithDecksFullPath, card.RelativeToDeckFolderQuestionImagePath);
                AbsoluteAnswerImagePath = Path.Combine(FileManager.currentFolderWithDecksFullPath, card.RelativeToDeckFolderAnswerImagePath);
            }
            else
            {
                Answer = card.Question;
                Question = card.Answer;
                AbsoluteAnswerImagePath = Path.Combine(FileManager.currentFolderWithDecksFullPath, card.RelativeToDeckFolderQuestionImagePath);
                AbsoluteQuestionImagePath = Path.Combine(FileManager.currentFolderWithDecksFullPath, card.RelativeToDeckFolderAnswerImagePath);
            }
            
            //Было добавлено
            OnPropertyChanged("AnswerImage");
            OnPropertyChanged("QuestionImage");
            OnPropertyChanged("Answer");
            OnPropertyChanged("Question");
            OnPropertyChanged("NameOfCard");
        }
        public void LoadImages()
        {
            AnswerImage = FileManager.CreateImageWithFullPath(AbsoluteAnswerImagePath);
            QuestionImage = FileManager.CreateImageWithFullPath(AbsoluteQuestionImagePath);
        }
        public void SaveThisCard()
        {
            if (!straightOrReverse)
                DeckManager.SaveCards(new List<Card>() { Card }, new List<string>() { Question }, new List<string>() { Answer }, new List<string>() { AbsoluteQuestionImagePath }, new List<string>() { AbsoluteAnswerImagePath });
            else
                DeckManager.SaveCards(new List<Card>() { Card }, new List<string>() { Answer }, new List<string>() { Question }, new List<string>() { AbsoluteAnswerImagePath }, new List<string>() { AbsoluteQuestionImagePath });

            MakeCardViewModelFromCardModel(Card, straightOrReverse);
        }
        public override string ToString()
        {
            return Question;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        public void UpdateQuestionImage(string absoluteImagePath)
        {
            AbsoluteQuestionImagePath = absoluteImagePath;
            QuestionImage = FileManager.CreateImageWithFullPath(absoluteImagePath);
            OnPropertyChanged("QuestionImage");
            OnPropertyChanged("NameOfCard");
        }
        public void UpdateAnswerImage(string absoluteImagePath)
        {
            AbsoluteAnswerImagePath = absoluteImagePath;
            AnswerImage = FileManager.CreateImageWithFullPath(absoluteImagePath);
            OnPropertyChanged("AnswerImage");
            OnPropertyChanged("NameOfCard");
        }
        public void DontSaveCurrentCard()
        {
            MakeCardViewModelFromCardModel(Card, straightOrReverse);
        }
    }
}
