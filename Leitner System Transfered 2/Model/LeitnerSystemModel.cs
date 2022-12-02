//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.IO;
//using System.Windows;
//using System.Runtime.Serialization;
//using System.Windows.Controls;
//using System.Windows.Navigation;

//namespace Leitner_System.Model
//{
//    public class LeitnerSystemModel
//    {
//        //--------------------------------------------------------------------------------
//        //------------------------------------- PUBLIC ---------------------------------
//        //--------------------------------------------------------------------------------
//        public List<Deck> Decks { get; private set; } = new List<Deck>();
//        public Deck CurrentDeck { get; private set; } = null;
//        public Card CurrentCard { get; private set; } = null;
//        //--------------------------------------------------------------------------------
//        //------------------------------------- PRIVATE ---------------------------------
//        //--------------------------------------------------------------------------------
//        public string currentFolderWithDecksPath { get; private set; } = "";
//        //Ноут
//        private string defaultFolderWithDecksPath = "G:\\Мой диск\\Программирование\\C#\\Проекты\\Leitner System\\Leitner System\\Decks";
//        //Десктоп
//        //private string defaultFolderWithDecksPath = "C:\\Users\\nikit\\OneDrive\\Программирование\\C#\\Проекты\\Leitner System\\Leitner System\\Decks";
//        private string defaultBackupFolder= "G:\\Мой диск\\Программирование\\C#\\Проекты\\Leitner System\\Leitner System\\Backups";
//        //--------------------------------------------------------------------------------
//        //------------------------------------- METHODS ---------------------------------
//        //--------------------------------------------------------------------------------
//        ///<summary>
//        ///Create new instance of LeitnerSystemModel and read decks from default folder
//        ///</summary>
//        public LeitnerSystemModel()
//        {
//           ReadDecksFromFolderInDecks(defaultFolderWithDecksPath);
//           //MakeBackupOfDecks();
//        }
//        ///<summary>
//        ///Read decks from folder in Decks, call DecksReload event, set default seck and card
//        ///</summary>
//        public void ReadDecksFromFolderInDecks(string folderPath)
//        {
//            Decks = GetDecksFromFolder(folderPath);
//            MakeBackupOfDecks();
//            OnDecksReload();
//            currentFolderWithDecksPath = folderPath;
//            CurrentDeck = null;
//            OnSelectedDeckChanged(new SelectedDeckChangedEventArgs(-1, defaultFolderWithDecksPath));
//            CurrentCard = null;
//            OnSelectedCardChanged(new IndexTransmissionEventArgs(-1));
//        }
//        ///<summary>
//        ///Return list of all decks in folder
//        ///</summary>
//        private List<Deck> GetDecksFromFolder(string pathOfFolderWithDecks)
//        {
//            List<Deck> output = new List<Deck>();
//            if (!Directory.Exists(pathOfFolderWithDecks))
//            {
//                MessageBox.Show("Указанная папка с колодами не найдена");
//                return output;
//            }
//            IEnumerable<string> decksDirectoriesNames = Directory.EnumerateDirectories(pathOfFolderWithDecks);
//            foreach (string deckDirectoryName in decksDirectoriesNames)
//            {
//                Deck deck = ReadDeckFromDeckFolderWithFullPath(deckDirectoryName);
//                if (deck != null)
//                    output.Add(deck);
//                else
//                    MessageBox.Show("Не удалось прочитать колоду из файла " + deckDirectoryName);
//            }
//            return output;
//        }
//        ///<summary>
//        ///Create new deck in Decks, write it down in the file, and file in new folder, call DeckAdd event
//        ///</summary>
//        public void AddDeck()
//        {
//            if (!Directory.Exists(currentFolderWithDecksPath))
//            {
//                MessageBox.Show("Папка не выбрана или не существует");
//                return;
//            }
//            Deck newDeck = new Deck(FindNameForNewDeckFolderInFolder(currentFolderWithDecksPath));
//            Decks.Add(newDeck);
//            if (!SaveDeckOrUpdateDeckFile(newDeck, currentFolderWithDecksPath))
//            {
//                Decks.Remove(newDeck);
//                MessageBox.Show("При попытке создать колоду и записать её в файл произошла ошибка");
//            }
//            OnDeckAdd(new DeckAddEventArgs(newDeck));
//        }
//        /// <summary>
//        /// Find appropriate name for new folder in folder in format "New deck X" where X is integer
//        /// </summary>
//        /// <param name="folderPath"></param>
//        /// <returns></returns>
//        private string FindNameForNewDeckFolderInFolder(string folderPath)
//        {
//            if (!Directory.Exists(folderPath))
//                return "";
//            int i = 1;
//            string path = Path.Combine(folderPath, "Новая колода " + i.ToString());
//            while (Directory.Exists(path))
//            {
//                i++;
//                path = Path.Combine(folderPath, "Новая колода " + i.ToString());
//            }
//            return ("Новая колода " + i.ToString());
//        }
//        ///<summary>
//        ///Create new folder if the deck is new, save the deck in it, old deck file is rewritten. Return true if saving is sucessful and false if it's not
//        ///</summary>
//        public bool SaveDeckOrUpdateDeckFile(Deck deck, string folderPath)
//        {
//            string fullDeckFolderPath = Path.Combine(folderPath, deck.Name);
//            if (!Directory.Exists(fullDeckFolderPath))
//                Directory.CreateDirectory(fullDeckFolderPath);

//            //Поиск и удаление старого файла колоды
//            string[] deckDirectoryFileNames = Directory.GetFiles(fullDeckFolderPath);
//            string oldDeckFileFullPath = "";
//            for (int i = 0; i < deckDirectoryFileNames.Length; i++)
//            {
//                if (deckDirectoryFileNames[i].Contains("xml"))
//                    oldDeckFileFullPath = deckDirectoryFileNames[i];
//            }
//            //if (!String.IsNullOrEmpty(oldDeckFileFullPath))
//            //    File.Delete(oldDeckFileFullPath);

//            string fullDeckFilePath = Path.Combine(fullDeckFolderPath, deck.Name + "_presaved.xml");
//            try
//            {
//                using (FileStream fs = new FileStream(fullDeckFilePath, FileMode.Create))
//                {
//                    DataContractSerializer ds = new DataContractSerializer(typeof(Deck));
//                    ds.WriteObject(fs, deck);
//                }
//                if (!String.IsNullOrEmpty(oldDeckFileFullPath))
//                    File.Delete(oldDeckFileFullPath);
//                File.Move(fullDeckFilePath, Path.Combine(fullDeckFolderPath, deck.Name + ".xml"));
//            }
//            catch(Exception ex)
//            {
//                MessageBox.Show("Не удалось записать файл " + fullDeckFilePath  + "\n" + "\n"+ex.Message+"\n"+ "\n" + "Проверьте файловую систему");
//                return false;
//            }
//            return true;
//        }
//        private void MakeBackupOfDecks()
//        {
//            string folderForCurrentBackupPath = "";
//            if (!Directory.Exists(defaultBackupFolder))
//                return;
//            int i = 1;
//            string path = Path.Combine(defaultBackupFolder, "Backup" + i.ToString());
//            while (Directory.Exists(path))
//            {
//                i++;
//                path = Path.Combine(defaultBackupFolder, "Backup" + i.ToString());
//            }
//            folderForCurrentBackupPath = path;
//            Directory.CreateDirectory(folderForCurrentBackupPath);
//            foreach (Deck deck in Decks)
//                SaveDeckOrUpdateDeckFile(deck, folderForCurrentBackupPath);
//        }
//        public void SaveAllDecks()
//        {
//            foreach (Deck deck in Decks)
//                SaveDeckOrUpdateDeckFile(deck, currentFolderWithDecksPath);
//            ReadDecksFromFolderInDecks(currentFolderWithDecksPath);
//        }
//        ///<summary>
//        ///Rename current deck
//        ///</summary>
//        public void UpdateNameOfCurrentDeckDeckFileAndDeckFolder(string newName)
//        {
//            if (CurrentDeck == null)
//            {
//                MessageBox.Show("Колода не выбрана");
//                return;
//            }
//            if (String.IsNullOrEmpty(newName))
//            {
//                MessageBox.Show("Колоду нельзя оставить без названия");
//                return;
//            }
//            if (!Directory.Exists(currentFolderWithDecksPath))
//            {
//                MessageBox.Show("Не выбрана папка с колодами");
//                return;
//            }
//            if (newName.Contains(".") || newName.Contains(",") || newName.Contains(":") || newName.Contains(";") || newName.Contains("?") || newName.Contains("=") || newName.Contains("+") || newName.Contains("*") || newName.Contains("\\") || newName.Contains("/") || newName.Contains("|") || newName.Contains("<") || newName.Contains(">"))
//            {
//                MessageBox.Show("Недопустимое имя файла");
//                return;
//            }
//            if (newName == CurrentDeck.Name)
//                return;
//            string deckOldFolderPath = Path.Combine(currentFolderWithDecksPath, CurrentDeck.Name);
//            string deckNewFolderPath = Path.Combine(currentFolderWithDecksPath, newName);
//            string oldDeckName = CurrentDeck.Name;
//            if (Directory.Exists(deckNewFolderPath))
//            {
//                MessageBox.Show("Папка с требуемым именем уже существует");
//                return;
//            }
//            CurrentDeck.ChangeName(newName);

//            Directory.Move(deckOldFolderPath, deckNewFolderPath);
//            if (!SaveDeckOrUpdateDeckFile(CurrentDeck, currentFolderWithDecksPath))
//            {
//                MessageBox.Show("При переименовании колоды"+ CurrentDeck.Name + " произошла ошибка, проверьте файловую систему");
//            }
//            OnDeckRename(new DeckRenameEventArgs(newName, Decks.IndexOf(CurrentDeck)));
//        }
//        ///<summary>
//        ///Find, read and return deck from folder
//        ///</summary>
//        private Deck ReadDeckFromDeckFolderWithFullPath(string fullDeckFolderPath)
//        {
//            Deck readingDeck = null;
//            if (!Directory.Exists(fullDeckFolderPath))
//                return null;
//            string[] deckDirectoryFileNames = Directory.GetFiles(fullDeckFolderPath);
//            string deckFileFullPath = "";
//            for (int i = 0; i < deckDirectoryFileNames.Length; i++)
//            {
//                if (deckDirectoryFileNames[i].Contains("xml"))
//                    deckFileFullPath = deckDirectoryFileNames[i];
//            }
//            if (String.IsNullOrEmpty(deckFileFullPath))
//            {
//                MessageBox.Show("В папке " + fullDeckFolderPath + "не удалось найти колоду.");
//                return null;
//            }
//            try
//            {
//                using (FileStream fs = new FileStream(deckFileFullPath, FileMode.Open))
//                {
//                    DataContractSerializer ds = new DataContractSerializer(typeof(Deck));
//                    object deck = ds.ReadObject(fs) as Deck;
//                    if (deck is Deck)
//                        readingDeck = deck as Deck;
//                    else
//                        MessageBox.Show("Не удалось прочитать колоду из файла: " + deckFileFullPath);
//                }
//            }
//            catch
//            {
//                MessageBox.Show("Не удалось прочитать колоду из файла: " + deckFileFullPath);
//                return null;
//            }
//            return readingDeck;
//        }
//        ///<summary>
//        ///Delete current deck and all its folder
//        ///</summary>
//        public void DeleteCurrentDeck()
//        {
//            if (CurrentDeck==null)
//            {
//                MessageBox.Show("Колода не выбрана");
//                return;
//            }
//            if (!Directory.Exists(Path.Combine(currentFolderWithDecksPath, CurrentDeck.Name)))
//                return;
//            MessageBoxResult result = MessageBox.Show("Вы действительно хотите удалить колоду?", "", MessageBoxButton.YesNo);
//            if (result != MessageBoxResult.Yes)
//                return;
//            string[] files = Directory.GetFiles(Path.Combine(currentFolderWithDecksPath, CurrentDeck.Name));
//            foreach (string fileName in files)
//                File.Delete(fileName);
//            Directory.Delete(Path.Combine(currentFolderWithDecksPath, CurrentDeck.Name));
//            int indexOfRemovingDeck = Decks.IndexOf(CurrentDeck);
//            Decks.Remove(CurrentDeck);
//            CurrentCard = null;
//            OnDeckRemove(new SelectedDeckChangedEventArgs(indexOfRemovingDeck, currentFolderWithDecksPath));
//        }
//        ///<summary>
//        ///Change current card oby new index, call SelectedDeckChanged event
//        ///</summary>
//        public void ChangeSelectionOfDeck(int newCurrentDeckIndex)
//        {
//            if (newCurrentDeckIndex >= 0 && newCurrentDeckIndex < Decks.Count)
//                CurrentDeck = Decks[newCurrentDeckIndex];
//            else
//                CurrentDeck = null;
//            string pathOfDeckFolder = "";
//            if (CurrentDeck != null)
//                pathOfDeckFolder = Path.Combine(currentFolderWithDecksPath,CurrentDeck.Name);
//            OnSelectedDeckChanged(new SelectedDeckChangedEventArgs(newCurrentDeckIndex, pathOfDeckFolder));
//        }
//        ///<summary>
//        ///Add card to the current deck and rewrite it
//        ///</summary>
//        public void AddCardToCurrentDeck()
//        {
//            if (CurrentDeck==null)
//            {
//                MessageBox.Show("Колода не выбрана");
//                return;
//            }
//            CurrentDeck.CreateNewCard();
//            //Надежда на то, что пользователь не переименует папку...
//            SaveDeckOrUpdateDeckFile(CurrentDeck, currentFolderWithDecksPath);
//            OnCardAdd(new CardAddEventArgs(CurrentDeck.Cards[CurrentDeck.Cards.Count-1], Path.Combine(currentFolderWithDecksPath, CurrentDeck.Name)));
//        }
//        ///<summary>
//        ///Delete card from current deck (if it's choosen), rewrite deck in file
//        ///</summary>
//        public void DeleteSelectedCardFromCurrentDeck()
//        {
//            if (CurrentCard == null || CurrentDeck == null)
//                return;
//            int removingIndex = CurrentDeck.Cards.IndexOf(CurrentCard);
//            CurrentDeck.DeleteCard(CurrentCard);
//            SaveDeckOrUpdateDeckFile(CurrentDeck, currentFolderWithDecksPath);
//            OnCardRemove(new SelectedDeckChangedEventArgs(removingIndex, Path.Combine(currentFolderWithDecksPath,CurrentDeck.Name)));
//        }
//        ///<summary>
//        ///Update current card by its new index
//        ///</summary>
//        public void UpdateSelectionCurrentCard(int newCurrentSelectedCardIndex)
//        {
//            if(CurrentDeck==null|| newCurrentSelectedCardIndex < 0)
//            {
//                CurrentCard = null;
//                newCurrentSelectedCardIndex = -1;
//            }
//            else
//            {
//                CurrentCard = CurrentDeck.Cards[newCurrentSelectedCardIndex];
//            }
//            OnSelectedCardChanged(new IndexTransmissionEventArgs(newCurrentSelectedCardIndex));
//        }
//        ///<summary>
//        ///Change four properties of card, rewrite deck file
//        ///</summary>
//        public void CurrentSelectedCardFieldsChange(string question, string answer, string questionImagePath, string answerImagePath)
//        {
//            if (CurrentDeck == null || CurrentCard == null)
//                return;
//            if (questionImagePath == CurrentCard.RelativeToDeckFolderQuestionImagePath)
//            {
//                //изображение вопроса не было изменено
//            }
//            else
//            {
//                //изображение вопроса было изменено
//                string oldImageFullPath = Path.Combine(currentFolderWithDecksPath, CurrentDeck.Name, CurrentCard.RelativeToDeckFolderQuestionImagePath);
//                if (File.Exists(oldImageFullPath))
//                    File.Delete(oldImageFullPath);
//                if (!String.IsNullOrEmpty(questionImagePath))
//                    questionImagePath = CopyImageFileToDeckFolderAndReturnTargetPath(questionImagePath, true, Path.Combine(currentFolderWithDecksPath, CurrentDeck.Name));
//            }
//            if (answerImagePath == CurrentCard.RelativeToDeckFolderAnswerImagePath)
//            {
//                //изображение ответа не было изменено
//            }
//            else
//            {
//                //изображение ответа было изменено
//                string oldImageFullPath = Path.Combine(currentFolderWithDecksPath, CurrentDeck.Name, CurrentCard.RelativeToDeckFolderAnswerImagePath);
//                if (File.Exists(oldImageFullPath))
//                    File.Delete(oldImageFullPath);
//                if (!String.IsNullOrEmpty(answerImagePath))
//                    answerImagePath = CopyImageFileToDeckFolderAndReturnTargetPath(answerImagePath, false, Path.Combine(currentFolderWithDecksPath, CurrentDeck.Name));
//            }
//            ////Надежда на то, что пользовательн не переименует папки
//            CurrentCard.UpdateCard(question, answer, questionImagePath, answerImagePath);
//            SaveDeckOrUpdateDeckFile(CurrentDeck, currentFolderWithDecksPath);
//            MessageBox.Show("Карта успешно сохранена в текущую колоду!");
//            OnCardChanged(new CurrentSelectedCardFieldsChangedEventArgs(CurrentCard));
//        }
//        ///<summary>
//        ///Copy question or answer image to the deck folder, return image path relative to the deck folder
//        ///</summary>
//        public string CopyImageFileToDeckFolderAndReturnTargetPath(string pathOfImage, bool questionOrAnswer, string fullDeckFolderPath)
//        {
//            string relativeToFolderFileTargetPath = "";
//            if (questionOrAnswer)
//                relativeToFolderFileTargetPath = "question";
//            else
//                relativeToFolderFileTargetPath = "answer";
//            int i = 0;
//            while (File.Exists(Path.Combine(fullDeckFolderPath, relativeToFolderFileTargetPath + i.ToString())))
//                i++;
//            File.Copy(pathOfImage, Path.Combine(fullDeckFolderPath, relativeToFolderFileTargetPath + i.ToString()));
//            return relativeToFolderFileTargetPath + i.ToString();
//        }
//        /// <summary>
//        /// Return new TrainigModel object with appropriate settings
//        /// </summary>
//        /// <param name="decks"></param>
//        /// <returns></returns>
//        //public TrainingModel StartNewTraining(Dictionary<Deck,bool> decksWithReverse, bool applyTimer, float timerDurationInSeconds)
//        //{
//        //    if (decksWithReverse.Count == 0)
//        //        return null;
//        //    return new TrainingModel(decksWithReverse, currentFolderWithDecksPath, this, applyTimer,timerDurationInSeconds);
//        //}

//        //--------------------------------------------------------------------------------
//        //------------------------------------- EVENTS ---------------------------------
//        //--------------------------------------------------------------------------------
//        public event EventHandler DeckAdd;
//        public event EventHandler DeckRemove;
//        public event EventHandler DeckRename;
//        public event EventHandler DecksReload;
//        public event EventHandler CardAdd;
//        public event EventHandler CardRemove;
//        public event EventHandler CurrentSelectedCardFieldsChanged;
//        public event EventHandler SelectedCardChanged;
//        public event EventHandler SelectedDeckChanged;

//        //--------------------------------------------------------------------------------
//        //------------------------------------- METHODS FIRING EVENTS ---------------------------------
//        //--------------------------------------------------------------------------------
//        public void OnDeckAdd(DeckAddEventArgs args)
//        {
//            EventHandler handler = DeckAdd;
//            if (handler != null)
//                handler(this, args);
//        }
//        public void OnDeckRemove(SelectedDeckChangedEventArgs args)
//        {
//            EventHandler handler = DeckRemove;
//            if (handler != null)
//                handler(this, args);
//        }
//        public void OnDeckRename(DeckRenameEventArgs args)
//        {
//            EventHandler handler = DeckRename;
//            if (handler != null)
//                handler(this, args);
//        }
//        public void OnDecksReload()
//        {
//            EventHandler handler = DecksReload;
//            if (handler != null)
//                handler(this, new EventArgs());
//        }
//        public void OnCardAdd(CardAddEventArgs args)
//        {
//            EventHandler handler = CardAdd;
//            if (handler != null)
//                handler(this, args);
//        }
//        public void OnCardRemove(SelectedDeckChangedEventArgs args)
//        {
//            EventHandler handler = CardRemove;
//            if (handler != null)
//                handler(this, args);
//        }
//        public void OnCardChanged(CurrentSelectedCardFieldsChangedEventArgs args)
//        {
//            EventHandler handler = CurrentSelectedCardFieldsChanged;
//            if (handler != null)
//                handler(this, args);
//        }
//        public void OnSelectedCardChanged(IndexTransmissionEventArgs args)
//        {
//            EventHandler handler = SelectedCardChanged;
//            if (handler != null)
//                handler(this, args);
//        }
//        public void OnSelectedDeckChanged(SelectedDeckChangedEventArgs args)
//        {
//            EventHandler handler = SelectedDeckChanged;
//            if (handler != null)
//                handler(this, args);
//        }
//    }
//    //public class DeckAddEventArgs : EventArgs
//    //{
//    //    public Deck Deck { get; private set; }
//    //    public DeckAddEventArgs(Deck deck)
//    //    {
//    //        Deck = deck;
//    //    }
//    //}
//    //public class DeckRenameEventArgs : EventArgs
//    //{
//    //    public string Name { get; private set; }
//    //    public int Index { get; private set; }
//    //    public DeckRenameEventArgs(string name, int index)
//    //    {
//    //        Name = name;
//    //        Index = index;
//    //    }
//    //}
//    //public class CardAddEventArgs : EventArgs
//    //{
//    //    public Card Card { get; private set; }
//    //    public string FullDeckFolderPath { get; private set; }
//    //    public CardAddEventArgs(Card card, string fullDeckFolderPath)
//    //    {
//    //        Card = card;
//    //        FullDeckFolderPath = fullDeckFolderPath;
//    //    }
//    //}
//    //public class IndexTransmissionEventArgs : EventArgs
//    //{
//    //    public int Index { get; private set; }
//    //    public IndexTransmissionEventArgs(int index)
//    //    {
//    //        Index = index;
//    //    }
//    //}
//    //public class CurrentSelectedCardFieldsChangedEventArgs : EventArgs
//    //{
//    //    public Card Card { get; private set; }
//    //    public CurrentSelectedCardFieldsChangedEventArgs(Card card)
//    //    {
//    //        Card = card;
//    //    }
//    //}
//    //public class SelectedDeckChangedEventArgs : EventArgs
//    //{
//    //    public int Index { get; private set; }
//    //    public string FolderPath { get; private set; }
//    //    public SelectedDeckChangedEventArgs(int index, string fullDeckFolderPath)
//    //    {
//    //        Index = index;
//    //        FolderPath = fullDeckFolderPath;
//    //    }
//    //}
//}
