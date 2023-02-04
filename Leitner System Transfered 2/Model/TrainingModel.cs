
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;

namespace Leitner_System_Transfered_2.Model
{
    public class TrainingModel
    {
        //--------------------------------------------------------------------------------
        //------------------------------------- PUBLIC PROPERTIES ---------------------------------
        //--------------------------------------------------------------------------------
        public int CurrentTrainingCardIndex { get; private set; } = -1;
        public int CurrentTrainingCardsCount
        {
            get { return trainingCards.Count; }
        }
        public bool TrainingIsComleted { get; private set; } = false;
        //--------------------------------------------------------------------------------
        //------------------------------------- PRIVATE PROPERTIES ---------------------------------
        //--------------------------------------------------------------------------------
        private Dictionary<Card, bool> trainingCardsWithReverse;
        //private Dictionary<Card, Deck> trainingCardsDictionary;
        private List<Card> trainingCards;
        private List<Deck> trainingDecks;
        public Dictionary<Card, Result> Results { get; private set; }
        //0 - undef
        //1 - correct
        //2 -wrong
        //3 - remove
        private int maxTrainingCardsCount;
        private TrainingTemplate currentTrainingTemplate;
        //-1 - without Template
        //0 - straight
        //1 - reverse
        //2 - random
        //--------------------------------------------------------------------------------
        //------------------------------------- METHODS ---------------------------------
        //--------------------------------------------------------------------------------
        /// <summary>
        /// Create new TrainingModel instance, select appropriatew training cards from training decks
        /// </summary>
        /// <param name="trainingDecks"></param>
        /// <param name="fullFolderWithDecksPath"></param>
        /// <param name="model"></param>
        public TrainingModel(Dictionary<Deck,ReverseSettings> trainingDecksWithReverseSettings)
        {
            //Find cuurent training template
            if (FileManager.settings.CurrentTrainingTemplate != -1)
                currentTrainingTemplate = FileManager.settings.TrainingTemplates[FileManager.settings.CurrentTrainingTemplate];
            else
                currentTrainingTemplate = FileManager.settings.GetDefaultTrainingTemplate();
            maxTrainingCardsCount = currentTrainingTemplate.maxCardsCount;

            this.trainingDecks = trainingDecksWithReverseSettings.Keys.ToList<Deck>();
            trainingCardsWithReverse = SelectTrainingCardsFromDeck(trainingDecksWithReverseSettings);
            if(trainingCardsWithReverse != null)
                trainingCards = trainingCardsWithReverse.Keys.ToList();
            trainingCards = ShuffleList(trainingCards);
            Results = new Dictionary<Card, Result>();
            foreach (Card card in trainingCards)
                Results.Add(card, Result.NoAnswer);
            CurrentTrainingCardIndex = 0;
            if (Results.Count == 0)
            {
                System.Windows.MessageBox.Show("There is no cards for training. Try tomorrow.");
            }
        }
        private List<Card> ShuffleList(List<Card> inputList)
        {
            if (inputList == null)
                return null;
            Random random = new Random();
            for (int i = 0; i < inputList.Count; i++)
            {
                int j = random.Next(0, inputList.Count);
                Card copy = inputList[j];
                inputList[j] = inputList[i];
                inputList[i] = copy;
            }
            return inputList;
        }
        private Dictionary<Card, bool> SelectCardsThatCanBeRepeated(Dictionary<Deck, ReverseSettings> trainingDecksWithReverse)
        {
            Dictionary<Card, bool> cardsWithReverse = new Dictionary<Card, bool>();
            Random random = new Random();
            foreach (Deck deck in trainingDecksWithReverse.Keys.ToList())
                foreach (Card card in deck.Cards)
                {
                    if (trainingDecksWithReverse[deck] == ReverseSettings.Straight)
                    {
                        if (card.CardIsAppropriateForTraining(true))
                            cardsWithReverse.Add(card, false);
                    }
                    else if (trainingDecksWithReverse[deck] == ReverseSettings.Reverse)
                    {
                        if (card.CardIsAppropriateForTraining(false))
                            cardsWithReverse.Add(card, true);
                    }
                    else
                    {
                        int straightOrReverse = random.Next(2);
                        if (straightOrReverse == 0)
                        {
                            if (card.CardIsAppropriateForTraining(true))
                                cardsWithReverse.Add(card, false);
                        }
                        else
                        {
                            if (card.CardIsAppropriateForTraining(false))
                                cardsWithReverse.Add(card, true);
                        }
                    }
                }
            return cardsWithReverse;
        }
        /// <summary>
        /// Select appropriate training cards from training decks
        /// </summary>
        private Dictionary<Card, bool> SelectTrainingCardsFromDeck(Dictionary<Deck, ReverseSettings> trainingDecksWithReverse)
        {
            if (trainingDecksWithReverse == null)
                return null;
            Dictionary<Card, bool> cardsWithReverse = SelectCardsThatCanBeRepeated(trainingDecksWithReverse);

            List<Card> sortedCards = new List<Card>();
            foreach (Card card in cardsWithReverse.Keys.ToList())
                sortedCards.Add(card);
            SortCardsListByDaysSinceCardSholdHaveBeenRepeated(sortedCards, cardsWithReverse);

            int numerOfCards = 0;
            if (sortedCards.Count > maxTrainingCardsCount)
                numerOfCards = maxTrainingCardsCount;
            else
                numerOfCards = sortedCards.Count;
            Dictionary<Card, bool> output = new Dictionary<Card, bool>();
            for (int i = 0; i < numerOfCards; i++)
                output.Add(sortedCards[i], cardsWithReverse[sortedCards[i]]);
            return output;
        }
        private void SortCardsListByDaysSinceCardSholdHaveBeenRepeated(List<Card> sortedCards, Dictionary<Card,bool> cardsWithReverse)
        {
            for (int i = 0; i < sortedCards.Count; i++)
                for (int j = 0; j < sortedCards.Count - 1; j++)
                {
                    int jDays = sortedCards[j].DaysSinceCardSholdHaveBeenRepeated(cardsWithReverse[sortedCards[j]]);
                    int jNextDays = sortedCards[j + 1].DaysSinceCardSholdHaveBeenRepeated(cardsWithReverse[sortedCards[j + 1]]);
                    if (jDays < jNextDays)
                    {
                        Card copy = sortedCards[j];
                        sortedCards[j] = sortedCards[j + 1];
                        sortedCards[j + 1] = copy;
                    }
                }
        }
        /// <summary>
        /// Update card repitition time on basis of answer, increase current card index. If it was the last card comlete the training, else - fire NextCard event
        /// </summary>
        /// <param name="answer"></param>
        public void NextCard(bool answer)
        {
            if (answer)
                Results[trainingCards[CurrentTrainingCardIndex]] = Result.Right;
            else
                Results[trainingCards[CurrentTrainingCardIndex]] = Result.Wrong;
            CurrentTrainingCardIndex++;
            if (CurrentTrainingCardIndex >= trainingCards.Count)
            {
                CompleteTraining();
                return;
            }
            OnNextCardEvent(new NextCardEventArgs(trainingCards[CurrentTrainingCardIndex], trainingCardsWithReverse[trainingCards[CurrentTrainingCardIndex]]));
        }
        public void StartTraining()
        {
            if (trainingCards == null)
            {
                CompleteTraining();
                return;
            }
            if (trainingCards.Count == 0)
            {
                CompleteTraining();
                return;
            }
            CurrentTrainingCardIndex = 0;
            OnNextCardEvent(new NextCardEventArgs(trainingCards[CurrentTrainingCardIndex], trainingCardsWithReverse[trainingCards[CurrentTrainingCardIndex]]));
        }
        /// <summary>
        /// Save all training decks, fire CompleteTraining event
        /// </summary>
        public void CompleteTraining()
        {
            TrainingIsComleted = true;
            OnCompleteTrainingEvent();
        }
        public void LeavePage()
        {
            foreach (Card card in Results.Keys)
            {
                if (Results[card] == Result.Delete)
                    card.ParentDeck.DeleteSelectedCard(new List<Card>() { card});
                bool answer = false;
                if (Results[card] == Result.Right)
                    answer = true;
                if (Results[card] == Result.Wrong)
                    answer = false;
                if (Results[card] == Result.NoAnswer)
                    continue;
                if (!trainingCardsWithReverse[card])
                    card.UpdateLastRepitionTime(answer);
                else
                    card.UpdateLastReverseRepitionTime(answer);
            }
            //trainingCardsDictionary[card].DeleteSelectedCard(new List<int>() { trainingCardsDictionary[card].Cards.IndexOf(card) });
            foreach (Deck deck in trainingDecks)
                FileManager.SaveDeckOrUpdateDeckFile(deck);
        }
        public void DeleteCurrentCard()
        {
            Results[trainingCards[CurrentTrainingCardIndex]] = Result.Delete;
            CurrentTrainingCardIndex++;
            if (CurrentTrainingCardIndex >= trainingCards.Count)
            {
                CompleteTraining();
                return;
            }
            OnNextCardEvent(new NextCardEventArgs(trainingCards[CurrentTrainingCardIndex], trainingCardsWithReverse[trainingCards[CurrentTrainingCardIndex]]));
        }
        public void UpdateResultOfCard(Card card, Result result)
        {
            //Processing of result changing
            Results[card] = result;

            foreach (Deck deck in trainingDecks)
                FileManager.SaveDeckOrUpdateDeckFile(deck);
        }
        //--------------------------------------------------------------------------------
        //------------------------------------- EVENTS ---------------------------------
        //--------------------------------------------------------------------------------
        public event EventHandler NextCardEvent;
        public event EventHandler CompleteTrainingEvent;
        //--------------------------------------------------------------------------------
        //------------------------------------- METHODS FIRING EVENTS ---------------------------------
        //--------------------------------------------------------------------------------
        private void OnNextCardEvent(NextCardEventArgs args)
        {
            EventHandler handler = NextCardEvent;
            if (handler != null)
                handler(this, args);
        }
        private void OnCompleteTrainingEvent()
        {
            EventHandler handler = CompleteTrainingEvent;
            if (handler != null)
                handler(this, new EventArgs());
        }
    }
    class NextCardEventArgs : EventArgs
    {
        public Card Card { get; private set; }
        public bool StraightOrReverse { get; private set; }
        public NextCardEventArgs(Card card, bool straightOrReverse)
        {
            Card = card;
            StraightOrReverse = straightOrReverse;
        }
    }
    public enum Result
    {
        NoAnswer,
        Right,
        Wrong,
        Delete,
    }
    public enum ReverseSettings
    {
        Straight,
        Reverse,
        Random,
        Manual,
    }
}
