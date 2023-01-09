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
        public DateTime LastRepititionTime { get; set; }
        public RepitionFrequensy RepitionFrequensy { get; set; }
        public DateTime LastReverseRepititionTime { get; set; }
        public RepitionFrequensy ReverseRepitionFrequensy { get; set; }
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
                string cardAbsoluteAnswerImagePath = Path.Combine(FileManager.currentFolderWithDecksFullPath, Card.parentDeck.Name, Card.RelativeToDeckFolderAnswerImagePath);
                if (String.IsNullOrEmpty(Card.RelativeToDeckFolderAnswerImagePath))
                    cardAbsoluteAnswerImagePath = Path.Combine(FileManager.currentFolderWithDecksFullPath, Card.parentDeck.Name);
                string cardAbsoluteQuestionImagePath = Path.Combine(FileManager.currentFolderWithDecksFullPath, Card.parentDeck.Name, Card.RelativeToDeckFolderQuestionImagePath);
                if (String.IsNullOrEmpty(Card.RelativeToDeckFolderQuestionImagePath))
                    cardAbsoluteQuestionImagePath = Path.Combine(FileManager.currentFolderWithDecksFullPath, Card.parentDeck.Name);
                if ((!straightOrReverse && Question == Card.Question &&
                Answer == Card.Answer &&
                AbsoluteQuestionImagePath == cardAbsoluteQuestionImagePath &&
                AbsoluteAnswerImagePath == cardAbsoluteAnswerImagePath) || (straightOrReverse && Question == Card.Answer &&
                Answer == Card.Question &&
                AbsoluteAnswerImagePath == cardAbsoluteQuestionImagePath &&
                AbsoluteQuestionImagePath == cardAbsoluteAnswerImagePath))
                {
                    return false;
                }
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

        public string AbsoluteQuestionImagePath { get; private set; }
        public string AbsoluteAnswerImagePath { get; private set; }
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
            LastRepititionTime = card.LastRepetitionTime;
            LastReverseRepititionTime = card.LastReverseRepetitionTime;
            RepitionFrequensy = card.RepitionFrequensy;
            ReverseRepitionFrequensy = card.ReverseRepitionFrequensy;
            this.straightOrReverse = straightOrReverse;
            if (!straightOrReverse)
            {
                Question = card.Question;
                Answer = card.Answer;
                AbsoluteQuestionImagePath = Path.Combine(FileManager.currentFolderWithDecksFullPath, card.parentDeck.Name, card.RelativeToDeckFolderQuestionImagePath);
                AbsoluteAnswerImagePath = Path.Combine(FileManager.currentFolderWithDecksFullPath, card.parentDeck.Name, card.RelativeToDeckFolderAnswerImagePath);
            }
            else
            {
                Answer = card.Question;
                Question = card.Answer;
                AbsoluteAnswerImagePath = Path.Combine(FileManager.currentFolderWithDecksFullPath, card.parentDeck.Name, card.RelativeToDeckFolderQuestionImagePath);
                AbsoluteQuestionImagePath = Path.Combine(FileManager.currentFolderWithDecksFullPath, card.parentDeck.Name, card.RelativeToDeckFolderAnswerImagePath);
            }
            //Было добавлено
            OnPropertyChanged("AnswerImage");
            OnPropertyChanged("QuestionImage");
            OnPropertyChanged("Answer");
            OnPropertyChanged("Question");
            OnPropertyChanged("NameOfCard");
        }
        public void SaveThisCard()
        {
            if (!straightOrReverse)
                DeckManager.SaveCards(new List<Card>() { Card }, new List<string>() { Question }, new List<string>() { Answer }, new List<string>() { AbsoluteQuestionImagePath }, new List<string>() { AbsoluteAnswerImagePath });
                //Card.SaveThisCard(Question, Answer, AbsoluteQuestionImagePath, AbsoluteAnswerImagePath);
            else
                DeckManager.SaveCards(new List<Card>() { Card }, new List<string>() { Question }, new List<string>() { Answer }, new List<string>() { AbsoluteAnswerImagePath }, new List<string>() { AbsoluteQuestionImagePath });

            //Card.SaveThisCard(Answer, Question, AbsoluteAnswerImagePath, AbsoluteQuestionImagePath);
            //Изображение сохранено в колоду и теперь может быть перезагружено из неё
            //AbsoluteAnswerImagePath = Path.Combine(FileManager.currentFolderWithDecksFullPath, Card.parentDeck.Name, Card.RelativeToDeckFolderAnswerImagePath);
            //AbsoluteQuestionImagePath = Path.Combine(FileManager.currentFolderWithDecksFullPath, Card.parentDeck.Name, Card.RelativeToDeckFolderQuestionImagePath);
            //AnswerImage = CreateImageWithFullPath(AbsoluteAnswerImagePath);            
            //QuestionImage = CreateImageWithFullPath(AbsoluteQuestionImagePath);
            //OnPropertyChanged("AnswerImage");
            //OnPropertyChanged("QuestionImage");
            //OnPropertyChanged("Answer");
            //OnPropertyChanged("Question");
            //OnPropertyChanged("NameOfCard");
            MakeCardViewModelFromCardModel(Card, straightOrReverse);
        }
        public void LoadImages()
        {
                AnswerImage = CreateImageWithFullPath(AbsoluteAnswerImagePath);
                QuestionImage = CreateImageWithFullPath(AbsoluteQuestionImagePath);
                
        }
        private CardViewModel(CardViewModel example)
        {
            this.Question = example.Question;
            this.Answer = example.Answer;
            this.AnswerImage = example.AnswerImage;
            this.QuestionImage = example.QuestionImage;
            this.AbsoluteAnswerImagePath = example.AbsoluteAnswerImagePath;
            this.AbsoluteQuestionImagePath = example.AbsoluteQuestionImagePath;
            this.LastRepititionTime = example.LastRepititionTime;
            this.LastReverseRepititionTime = example.LastReverseRepititionTime;
            this.RepitionFrequensy = example.RepitionFrequensy;
            this.ReverseRepitionFrequensy = example.ReverseRepitionFrequensy;
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
        private BitmapImage CreateImageWithFullPath(string path)
        {
            if (String.IsNullOrEmpty(path) || !File.Exists(path))
                return null;
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    Stream stream = File.OpenRead(path);
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    stream.Close();
                     return bitmap;
                }
                catch
                {
                    BitmapImage bitmap = new BitmapImage(); 
                    bitmap.BeginInit();
                    string pathOfReservedImage = Path.Combine(Environment.CurrentDirectory, "Assets\\imageUploadingIsNotSucesful.jpg");
                    Stream stream = File.OpenRead(pathOfReservedImage);
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    stream.Close();
                    return bitmap;
                }
                
            //}
            //catch(Exception e)
            //{
            //    MessageBox.Show("Не удалось сохдать изображение " + e.Message);
            //    return null;
            //}
        }
        public void UpdateQuestionImage(string absoluteImagePath)
        {
            AbsoluteQuestionImagePath = absoluteImagePath;
            QuestionImage = CreateImageWithFullPath(absoluteImagePath);
            OnPropertyChanged("QuestionImage");
            OnPropertyChanged("NameOfCard");
        }
        public void UpdateAnswerImage(string absoluteImagePath)
        {
            AbsoluteAnswerImagePath = absoluteImagePath;
            AnswerImage = CreateImageWithFullPath(absoluteImagePath);
            OnPropertyChanged("AnswerImage");
            OnPropertyChanged("NameOfCard");
        }
        public void DontSaveCurrentCard()
        {
            MakeCardViewModelFromCardModel(Card, straightOrReverse);
            LoadImages();
        }
    }
}
