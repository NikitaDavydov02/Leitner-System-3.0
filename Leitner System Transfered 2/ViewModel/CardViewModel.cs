﻿using System;
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
using System.Drawing;

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
                //string cardAbsoluteAnswerImagePath = Path.Combine(FileManager.currentFolderWithDecksFullPath, Card.RelativeToDeckFolderAnswerImagePath);
                //if (Card.AnswerImage==null)
                //    cardAbsoluteAnswerImagePath = Path.Combine(FileManager.currentFolderWithDecksFullPath);
                //string cardAbsoluteQuestionImagePath = Path.Combine(FileManager.currentFolderWithDecksFullPath, Card.RelativeToDeckFolderQuestionImagePath);
                //if (Card.QuestionImage==null)
                //    cardAbsoluteQuestionImagePath = Path.Combine(FileManager.currentFolderWithDecksFullPath);
                if ((!straightOrReverse && Question == Card.Question &&
                Answer == Card.Answer &&
                AnswerImageByte == Card.AnswerImageByte &&
                QuestionImageByte==Card.QuestionImageByte) || (straightOrReverse && Question == Card.Answer &&
                Answer == Card.Question &&
                AnswerImageByte == Card.QuestionImageByte &&
                QuestionImageByte == Card.AnswerImageByte))
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

        private byte[] QuestionImageByte;
        private byte[] AnswerImageByte;
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
                QuestionImageByte = card.QuestionImageByte;
                AnswerImageByte = card.AnswerImageByte;
            }
            else
            {
                Answer = card.Question;
                Question = card.Answer;
                QuestionImageByte = card.AnswerImageByte;
                AnswerImageByte = card.QuestionImageByte;
            }
            QuestionImage = FileManager.ImageFromByteArray(QuestionImageByte);
            AnswerImage = FileManager.ImageFromByteArray(AnswerImageByte);
            //Было добавлено
            OnPropertyChanged("AnswerImage");
            OnPropertyChanged("QuestionImage");
            OnPropertyChanged("Answer");
            OnPropertyChanged("Question");
            OnPropertyChanged("NameOfCard");
            OnPropertyChanged("SuggestedAnswrImage");
            OnPropertyChanged("SuggestedQuestionImage");
        }
        public void LoadImages()
        {
            //AnswerImage = FileManager.CreateImageWithFullPath(AbsoluteAnswerImagePath);
            //QuestionImage = FileManager.CreateImageWithFullPath(AbsoluteQuestionImagePath);
        }
        public void SaveThisCard()
        {
            if (!straightOrReverse)
                DeckManager.SaveCards(new List<Card>() { Card }, new List<string>() { Question }, new List<string>() { Answer }, new List<byte[]>() { QuestionImageByte }, new List<byte[]>() { AnswerImageByte });
            else
                DeckManager.SaveCards(new List<Card>() { Card }, new List<string>() { Answer }, new List<string>() { Question }, new List<byte[]>() { AnswerImageByte }, new List<byte[]>() { QuestionImageByte });

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
            //AbsoluteQuestionImagePath = absoluteImagePath;
            QuestionImage = FileManager.CreateImageWithFullPath(absoluteImagePath);
            QuestionImageByte= FileManager.ByteFromImageFile(absoluteImagePath);
            OnPropertyChanged("QuestionImage");
            OnPropertyChanged("NameOfCard");
        }
        public void UpdateAnswerImage(string absoluteImagePath)
        {
            //AbsoluteAnswerImagePath = absoluteImagePath;
            AnswerImage = FileManager.CreateImageWithFullPath(absoluteImagePath);
            AnswerImageByte = FileManager.ByteFromImageFile(absoluteImagePath);
            OnPropertyChanged("AnswerImage");
            OnPropertyChanged("NameOfCard");
        }
        public void DontSaveCurrentCard()
        {
            MakeCardViewModelFromCardModel(Card, straightOrReverse);
        }
    }
}
