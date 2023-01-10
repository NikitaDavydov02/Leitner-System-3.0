using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leitner_System_Transfered_2.Model;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;

namespace Leitner_System_Transfered_2.ViewModel
{
    public  class TrainingViewModel : INotifyPropertyChanged
    {
        //--------------------------------------------------------------------------------
        //------------------------------------- PUBLIC ---------------------------------
        //--------------------------------------------------------------------------------
        public CardViewModel CurrentTrainingCard { get; private set; }
        public string CardsAnswered { get; private set; }
        public bool TrainingIsComleted 
        {
            get { return trainingModel.TrainingIsComleted; }
        }
        public string CorrectAnswersCount { get; private set; } 
        public string WrongAnswersCount { get; private set; }
        public string Procentage { get; private set; }
        public ObservableCollection<CardResultViewModel> Results { get; private set; }
        //--------------------------------------------------------------------------------
        //------------------------------------- PRIVATE ---------------------------------
        //--------------------------------------------------------------------------------
        private TrainingModel trainingModel;
        //--------------------------------------------------------------------------------
        //------------------------------------- METHODS ---------------------------------
        //--------------------------------------------------------------------------------
        /// <summary>
        /// Create new TrainingViewModel object and start training
        /// </summary>
        /// <param name="trainingModel"></param>
        public TrainingViewModel(TrainingModel trainingModel)
        {
            this.trainingModel = trainingModel;
            trainingModel.NextCardEvent += NextCardEventHandler;
            trainingModel.CompleteTrainingEvent += CompleteTrainingEventHandler;
            trainingModel.StartTraining();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(name));
        }
        /// <summary>
        /// Display next training card and hide answer for it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void NextCardEventHandler(object sender, EventArgs args)
        {
            NextCardEventArgs nextCardEventArgs = args as NextCardEventArgs;
            CurrentTrainingCard = new CardViewModel(nextCardEventArgs.Card, nextCardEventArgs.StraightOrReverse);
            CurrentTrainingCard.AnswerIsVisible = false;
            CurrentTrainingCard.LoadImages();
            CardsAnswered = (trainingModel.CurrentTrainingCardIndex).ToString() + "/" + trainingModel.CurrentTrainingCardsCount.ToString();
            //AnswerIsVisible = false;
            OnPropertyChanged("CurrentTrainingCard");
            OnPropertyChanged("CardsAnswered");
            //OnPropertyChanged("AnswerIsVisible");
        }
        /// <summary>
        /// Display finish paneland statistics
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CompleteTrainingEventHandler(object sender, EventArgs args)
        {
            CardsAnswered = (trainingModel.CurrentTrainingCardIndex).ToString() + "/" + trainingModel.CurrentTrainingCardsCount.ToString();
            CurrentTrainingCard = null;

            int wrongAnswers = 0;
            int correctAnswers = 0;
            //Statistics
            foreach(Result i in trainingModel.Results.Values)
            {
                if (i==Result.Right)
                    correctAnswers++;
                else if (i == Result.Wrong)
                    wrongAnswers++;
            }
            CorrectAnswersCount = correctAnswers.ToString();
            WrongAnswersCount = wrongAnswers.ToString();
            Procentage = (Math.Round(((double)correctAnswers / ((double)wrongAnswers + (double)correctAnswers)) * 100,2).ToString()+"%");
            //Fill result collection with contnent
            Results = new ObservableCollection<CardResultViewModel>();
            foreach (Card c in trainingModel.Results.Keys)
            {
                CardResultViewModel cardResultViewModel = new CardResultViewModel() { Name = c.Question, Result = trainingModel.Results[c], Card = c };
                cardResultViewModel.CardResultChanged += ChangeResultOfCard;
                Results.Add(cardResultViewModel);
            }
                
            //EndEffects and savings
            OnPropertyChanged("CurrentTrainingCard");
            OnPropertyChanged("TrainingIsComleted");
            OnPropertyChanged("CardsAnswered");
            OnPropertyChanged("CorrectAnswersCount");
            OnPropertyChanged("WrongAnswersCount");
            OnPropertyChanged("Procentage");
            OnPropertyChanged("Results");
        }
        /// <summary>
        /// Show answer for current displaied card
        /// </summary>
        public void ShowAnswer()
        {
            CurrentTrainingCard.AnswerIsVisible = true;
            OnPropertyChanged("AnswerIsVisible");
        }
        /// <summary>
        /// Send answer to model and call next training card
        /// </summary>
        /// <param name="answer"></param>
        public void GetAnswer(bool answer)
        {
            if (!TrainingIsComleted)
            {
                if (CurrentTrainingCard.UnsavedChanges)
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show("У вас есть несохранённые изменения в картах. Вы действительно хотите продолжить?", "", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.No)
                        return;
                }
                trainingModel.NextCard(answer);
            }
        }
        public void Earlyfinish()
        {
            trainingModel.CompleteTraining();
        }
        public void DeleteCurrentCard()
        {
            trainingModel.DeleteCurrentCard();
        }
        public void ChangeResultOfCard(object sender, EventArgs args)
        {
            CardResultChangedEventArgs eventArgs = args as CardResultChangedEventArgs;
            if (eventArgs != null)
            {
                Card card = eventArgs.Card;
                Result newResult = eventArgs.Result;
                trainingModel.UpdateResultOfCard(card, newResult);
            }
        }
        public void LeavePage()
        {
            trainingModel.LeavePage();
        }
    }
}
