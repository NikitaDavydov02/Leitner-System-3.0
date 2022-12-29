using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leitner_System_Transfered_2.Model;
using System.IO;
using System.Windows;
using System.Runtime.Serialization;
//using Aspose.Cells;
using Excel=Microsoft.Office.Interop.Excel;



namespace Leitner_System_Transfered_2.Model
{
    static class FileManager
    {
        public static string currentFolderWithDecksFullPath { get; private set; }
        private static string defaultFolderWithDecksPath = Path.Combine(Environment.CurrentDirectory, "Decks");
        private static string defaultBackupFolder = Path.Combine(Environment.CurrentDirectory, "Backup");

        //private static string defaultFolderWithDecksPath = "G:\\My Drive\\Программирование\\C#\\Проекты\\Leitner System\\Leitner System\\Тестовые колоды";
        //private static string defaultBackupFolder = "G:\\My Drive\\Программирование\\C#\\Проекты\\Leitner System\\Leitner System\\Backups";
        public static SettingsModel settings;

        ///<summary>
        ///[Problem with optimization] 1. Return list of all decks in folder (if the folder pass is empty so return decks from default folder)
        ///2. Return null if the process was faild
        ///3. Update current folder with decks path field
        ///4. If some decks were unable to read they are ignored and aren't include in outout collection
        ///5. Set parent deck for each card in loaded decks
        ///</summary>
        ///<param name="pathOfFolderWithDecks">Absolute path of folder that contains decks (if it is empty, so reading from default folder)</param>
        //public async static Task<Deck> ReadDeckFromDeckFolderWithFullPathAsync(string absolutePathOfFolderWithDecks)
        //{
        //   return await Task.Run(() => ReadDeckFromDeckFolderWithFullPath(absolutePathOfFolderWithDecks));
        //}
        public static List<Deck> GetDecksFromFolder(string absolutePathOfFolderWithDecks = "")
        {
            if (String.IsNullOrEmpty(absolutePathOfFolderWithDecks))
                absolutePathOfFolderWithDecks = settings.AbsolutePathOfSaveDeckFolder;
            if (!Directory.Exists(settings.AbsolutePathOfSaveDeckFolder))
                Directory.CreateDirectory(settings.AbsolutePathOfSaveDeckFolder);
            List<Deck> output = new List<Deck>();
            if (!Directory.Exists(absolutePathOfFolderWithDecks))
            {
                MessageBox.Show("Folder " +absolutePathOfFolderWithDecks +" was not found.");
                return null;
            }
            currentFolderWithDecksFullPath = absolutePathOfFolderWithDecks;
            IEnumerable<string> decksDirectoriesNames = Directory.EnumerateDirectories(absolutePathOfFolderWithDecks);
            foreach (string deckDirectoryName in decksDirectoriesNames)
            {
                Deck deck = ReadDeckFromDeckFolderWithFullPath(deckDirectoryName);
                if (deck != null)
                    output.Add(deck);
                else
                    MessageBox.Show("Reading of deck from " + deckDirectoryName + "was not sucssesful.");
            }
            foreach(Deck deck in output)
                deck.SetParentDeckToCards();
            MakeBackupOfDecksFromCurrentFolder(output);
            return output;
        }
        public static void UploadSettings()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "settings.xml");
            if (!File.Exists(path))
            {
                //Default settings
                SettingsModel defaultSettings= new SettingsModel();
                settings = defaultSettings;
                settings.AbsolutePathOfBackupFolder = defaultBackupFolder;
                settings.AbsolutePathOfSaveDeckFolder = defaultFolderWithDecksPath;
                settings.CurrentTrainingTemplate = -1;
                settings.MakeBackup = false;
                settings.TrainingTemplates = new List<TrainingTemplate>();
                settings.TrainingTemplates.Add(settings.CreateDefault());
                SaveSettings();
            }
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    DataContractSerializer ds = new DataContractSerializer(typeof(SettingsModel));
                    object s = ds.ReadObject(fs);
                    if (s is SettingsModel)
                        settings = s as SettingsModel;
                    if (s == null)
                    {
                        settings = new SettingsModel();
                        settings.MakeBackup = false;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Settings was not found in " + path);
            }
        }
        public static void SaveSettings()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "settings.xml");
            if (String.IsNullOrEmpty(settings.AbsolutePathOfBackupFolder))
                settings.AbsolutePathOfBackupFolder = defaultBackupFolder;
            if (String.IsNullOrEmpty(settings.AbsolutePathOfSaveDeckFolder))
                settings.AbsolutePathOfSaveDeckFolder = defaultFolderWithDecksPath;
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    DataContractSerializer ds = new DataContractSerializer(typeof(SettingsModel));
                    ds.WriteObject(fs, settings);
                }
            }
            catch
            {
                MessageBox.Show("Settings was not saved");
            }
        }
        ///<summary>
        ///Return deck from folder by its absolute path, if the process was faild return null
        ///</summary>
        ///<param name="pathOfFolderWithDecks">Absolute path of folder that contains decks</param>
        private static Deck ReadDeckFromDeckFolderWithFullPath(string absolutePathOfFolderWithDecks)
        {
            Deck readingDeck = null;
            if (!Directory.Exists(absolutePathOfFolderWithDecks))
                return null;
            string[] deckDirectoryFileNames = Directory.GetFiles(absolutePathOfFolderWithDecks);
            string deckFileFullPath = "";
            for (int i = 0; i < deckDirectoryFileNames.Length; i++)
            {
                if (deckDirectoryFileNames[i].Contains("xml"))
                    deckFileFullPath = deckDirectoryFileNames[i];
            }
            if (String.IsNullOrEmpty(deckFileFullPath))
            {
                MessageBox.Show("There is no this deck in folder " + absolutePathOfFolderWithDecks);
                return null;
            }
            try
            {
                using (FileStream fs = new FileStream(deckFileFullPath, FileMode.Open))
                {
                    DataContractSerializer ds = new DataContractSerializer(typeof(Deck));
                    object deck = ds.ReadObject(fs) as Deck;
                    if (deck is Deck)
                        readingDeck = deck as Deck;
                    else
                    {
                        MessageBox.Show("Error while reading from: " + deckFileFullPath);
                        return null;
                    }
                }
            }
            catch(Exception e)
            {
                MessageBox.Show("Error while reading from: " + deckFileFullPath + e.Message);
                return null;
            }
            return readingDeck;
        }
        //public static async Task ExportExcelFileAsync(string absoluteFilePath, Dictionary<string, string> input)
        //{
        //    await Task.Run(() => ExportExcelFile(absoluteFilePath, input));
        //}
        public static void ExportExcelFile(string absoluteFilePath, Dictionary<string,string> input)
        {
            if (String.IsNullOrEmpty(absoluteFilePath))
                return;
            Excel.Application application = new Excel.Application();
            application.Workbooks.Add();
            Excel.Worksheet workSheet = application.ActiveSheet;
            //try
            //{

            //    workbook = application.Workbooks.Open(absoluteFilePath);

            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Не удалось импортировать файл " + absoluteFilePath + "\n" + "\n" + ex.Message + "\n" + "\n" + "Проверьте файловую систему");
            //    return null;
            //}
            //Workbook wb = new Workbook();
            //Worksheet sheet = wb.Worksheets[0];
            for (int i = 0; i < input.Count; i++)
            {
                workSheet.Cells[i + 1, "A"] = input.Keys.ElementAt(i);
                workSheet.Cells[i + 1, "B"] = input[input.Keys.ElementAt(i)];
            }
            workSheet.Columns[1].AutoFit();
            workSheet.Columns[2].AutoFit();
            try
            {
                workSheet.SaveAs(absoluteFilePath);
                application.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(application);
                //wb.Save(absoluteFilePath, SaveFormat.Xlsx);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export was not susessfull " + absoluteFilePath + "\n" + "\n" + ex.Message + "\n");

            }
        }
        //public static async Task<Dictionary<string, string>> ImportExcelFileAsync(string absoluteFilePath)
        //{
        //    return await Task.Run(() => ImportExcelFile(absoluteFilePath));
        //}
        public static Dictionary<string, string> ImportExcelFile(string absoluteFilePath)
        {
            if (String.IsNullOrEmpty(absoluteFilePath))
                return null;
            Excel.Workbook workbook = null;
            Excel.Application application = new Excel.Application();
            try
            {
                
                workbook = application.Workbooks.Open(absoluteFilePath);
                
            }
            catch(Exception ex)
            {
                MessageBox.Show("Import was not susessfull  " + absoluteFilePath + "\n" + "\n" + ex.Message + "\n");
                return null;
            }
            if (workbook == null)
                return null;
            Excel.Worksheet excelSheet = workbook.Sheets[1];
            Excel.Range range = excelSheet.UsedRange;
            int rows = range.Rows.Count;
            Dictionary<string, string> output = new Dictionary<string, string>();

            // Цикл по строкам
            for (int i = 1; i <= rows; i++)
            {
                string question;
                string answer;
                try
                {
                    question=excelSheet.Cells[i, 1].Value.ToString();
                }
                catch
                {
                    question = "";
                }
                try
                {
                    answer = excelSheet.Cells[i, 2].Value.ToString();
                }
                catch
                {
                    answer = "";
                }
                if(!output.ContainsKey(question))
                    output.Add(question, answer);
                else
                {

                }
            }
            application.Quit();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(application);
            return output;
        }

        /// <summary>
        /// Find appropriate name for new folder in current folder with decks in format "prefix X" where X is integer
        /// </summary>
        /// <param name="prefix">prefix of new name for deck</param>
        /// <returns></returns>
        public static string FindNameForNewDeckFolderInCurrentFolderWithDecks(string prefix="New deck")
        {
            if (!Directory.Exists(currentFolderWithDecksFullPath))
                return "";
            int i = 1;
            string path = Path.Combine(currentFolderWithDecksFullPath, prefix +" " + i.ToString());
            while (Directory.Exists(path))
            {
                i++;
                path = Path.Combine(currentFolderWithDecksFullPath, prefix + " " + i.ToString());
            }
            return (prefix + " " + i.ToString());
        }
        //public static async Task<bool> SaveDeckOrUpdateDeckFileAsync(Deck deck, string absolutePathOfFolderWithDecks = "")
        //{
        //    return await Task.Run(() => SaveDeckOrUpdateDeckFile(deck, absolutePathOfFolderWithDecks));
        //}
        ///<summary>
        ///If deck folder doesn't exist create new one, if it already exists presaved file, then remove old deck file from folder, after that rename new deck file in this folder by the name of old file. 
        ///Return true if the process was sucessful and false in oposite case.
        ///</summary>
        ///<param name="deck">Deck to save</param>
        ///<param name="folderWithDecksPath">Folder where save deck (if it's empty so saving in current folder with decks)</param>
        public static bool SaveDeckOrUpdateDeckFile(Deck deck, string absolutePathOfFolderWithDecks = "")
        {
            if (String.IsNullOrEmpty(absolutePathOfFolderWithDecks))
                absolutePathOfFolderWithDecks = currentFolderWithDecksFullPath;
            string fullDeckFolderPath = Path.Combine(absolutePathOfFolderWithDecks, deck.Name);
            if (!Directory.Exists(fullDeckFolderPath))
                Directory.CreateDirectory(fullDeckFolderPath);
            string[] deckDirectoryFileNames = Directory.GetFiles(fullDeckFolderPath);
            string oldDeckFileFullPath = "";
            for (int i = 0; i < deckDirectoryFileNames.Length; i++)
            {
                if (deckDirectoryFileNames[i].Contains("xml"))
                    oldDeckFileFullPath = deckDirectoryFileNames[i];
            }
            string fullDeckFilePath = Path.Combine(fullDeckFolderPath, deck.Name + "_presaved.xml");
            try
            {
                using (FileStream fs = new FileStream(fullDeckFilePath, FileMode.Create))
                {
                    DataContractSerializer ds = new DataContractSerializer(typeof(Deck));
                    ds.WriteObject(fs, deck);
                }
                if (!String.IsNullOrEmpty(oldDeckFileFullPath))
                    File.Delete(oldDeckFileFullPath);
                File.Move(fullDeckFilePath, Path.Combine(fullDeckFolderPath, deck.Name + ".xml"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Saving was not sucsessfull " + fullDeckFilePath + "\n" + "\n" + ex.Message + "\n");
                return false;
            }
            return true;
        }
        ///<summary>
        ///Save list of decks in folder for backup
        ///</summary>
        ///<param name="decksToBackup">List of decks that will be saved</param>
        private static void MakeBackupOfDecksFromCurrentFolder(List<Deck> decksToBackup)
        {
            if (!settings.MakeBackup)
                return;
            if (!Directory.Exists(settings.AbsolutePathOfBackupFolder))
                Directory.CreateDirectory(settings.AbsolutePathOfBackupFolder);
            string folderForCurrentBackupPath = "";
            if (!Directory.Exists(settings.AbsolutePathOfBackupFolder))
                return;
            int i = 1;
            string path = Path.Combine(settings.AbsolutePathOfBackupFolder, "Backup" + i.ToString());
            while (Directory.Exists(path))
            {
                i++;
                path = Path.Combine(settings.AbsolutePathOfBackupFolder, "Backup" + i.ToString());
            }
            folderForCurrentBackupPath = path;
            Directory.CreateDirectory(folderForCurrentBackupPath);
            foreach (Deck deck in decksToBackup)
            {
                SaveDeckOrUpdateDeckFile(deck, folderForCurrentBackupPath);

            }
        }
        ///<summary>
        ///Rename deck folder, file, and deck itself. If the process was faild return false and true in opposite case
        ///</summary>
        ///<param name="newName">New name of deck</param>
        ///<param name="deckNameToRename">Deck name that will be renamed</param>
        public static bool UpdateNameOfDeckDeckFileAndDeckFolder(string newName, string deckNameToRename)
        {
            if (String.IsNullOrEmpty(newName))
            {
                MessageBox.Show("Deck must have name");
                return false;
            }
            if (!Directory.Exists(currentFolderWithDecksFullPath))
            {
                MessageBox.Show("Folder is not choosen");
                return false;
            }
            if (newName.Contains(".") || newName.Contains(",") || newName.Contains(":") || newName.Contains(";") || newName.Contains("?") || newName.Contains("=") || newName.Contains("+") || newName.Contains("*") || newName.Contains("\\") || newName.Contains("/") || newName.Contains("|") || newName.Contains("<") || newName.Contains(">"))
            {
                MessageBox.Show("Forbiden file name");
                return false;
            }
            if (newName == deckNameToRename)
                return false;
            string deckOldFolderPath = Path.Combine(currentFolderWithDecksFullPath, deckNameToRename);
            string deckNewFolderPath = Path.Combine(currentFolderWithDecksFullPath, newName);
            if (Directory.Exists(deckNewFolderPath))
            {
                MessageBox.Show("Folder with this name already exists");
                return false;
            }
            if (!Directory.Exists(deckOldFolderPath))
            {
                MessageBox.Show("There is no folder to rename");
                return false;
            }

            Directory.Move(deckOldFolderPath, deckNewFolderPath);
            string[] deckDirectoryFileNames = Directory.GetFiles(deckNewFolderPath);
            string oldDeckFileFullPath = "";
            for (int i = 0; i < deckDirectoryFileNames.Length; i++)
            {
                if (deckDirectoryFileNames[i].Contains("xml"))
                    oldDeckFileFullPath = deckDirectoryFileNames[i];
            }
            if (String.IsNullOrEmpty(oldDeckFileFullPath))
                return false;
            try
            {
                File.Move(oldDeckFileFullPath, Path.Combine(deckNewFolderPath, newName + ".xml"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("File was not saved " + oldDeckFileFullPath + "\n" + "\n" + ex.Message + "\n");
                return false;
            }
            return true;
        }
        ///<summary>
        ///Remove deck from current folder with decks
        ///</summary>
        ///<param name="deckToDelete">Deck that will be delate</param>
        public static void DeleteDeckFromCurrentFolder(string deckToDeleteName)
        {
            if (!Directory.Exists(Path.Combine(currentFolderWithDecksFullPath, deckToDeleteName)))
                return;
            MessageBoxResult result = MessageBox.Show("Do you want to delete this deck?", "", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes)
                return;
            string[] files = Directory.GetFiles(Path.Combine(currentFolderWithDecksFullPath, deckToDeleteName));
            foreach (string fileName in files)
                File.Delete(fileName);
            Directory.Delete(Path.Combine(currentFolderWithDecksFullPath, deckToDeleteName));
        }
        ///<summary>
        ///Copy image files from it's locations to deck folder if they were
        ///</summary>
        ///<param name="currentCard">current card to save</param>
        public static void CopyNewImagesToDeckFolder(Card currentCard, string questionAbsoluteImagePath, string answerAbsoluteImagePath, out string relativeAnswerImagePath, out string relativeQuestionImagePath)
        {
            relativeQuestionImagePath = "";
            relativeAnswerImagePath = "";
            string oldQuestionImageAbsolitePath = Path.Combine(currentFolderWithDecksFullPath, currentCard.parentDeck.Name, currentCard.RelativeToDeckFolderQuestionImagePath);
            string oldAnswerImageAbsolitePath = Path.Combine(currentFolderWithDecksFullPath, currentCard.parentDeck.Name, currentCard.RelativeToDeckFolderAnswerImagePath);
            if (questionAbsoluteImagePath != oldQuestionImageAbsolitePath)
            {
                if (File.Exists(oldQuestionImageAbsolitePath))
                    File.Delete(oldQuestionImageAbsolitePath);
                if (!String.IsNullOrEmpty(questionAbsoluteImagePath))
                    relativeQuestionImagePath = CopyImageFileToDeckFolderAndReturnRelativeToDeckFolderPath(questionAbsoluteImagePath, true, currentCard.parentDeck.Name);
            }
            else
                relativeQuestionImagePath = currentCard.RelativeToDeckFolderQuestionImagePath;
            if (answerAbsoluteImagePath != oldAnswerImageAbsolitePath)
            {
                if (File.Exists(oldAnswerImageAbsolitePath))
                    File.Delete(oldAnswerImageAbsolitePath);
                if (!String.IsNullOrEmpty(answerAbsoluteImagePath))
                    relativeAnswerImagePath = CopyImageFileToDeckFolderAndReturnRelativeToDeckFolderPath(answerAbsoluteImagePath, false, currentCard.parentDeck.Name);
            }
            else
                relativeAnswerImagePath = currentCard.RelativeToDeckFolderAnswerImagePath;
        }
        ///<summary>
        ///Copy image file from it's location to decks folder and return relative to deck folder path of image copy
        ///</summary>
        ///<param name="absolutePathOfImageToCopy">Absolute path of image file that should be copied</param>
        ///<param name="questionOrAnswerImage">True if it is a question inage fakse if it is an answer image</param>
        ///<param name="deckName">Name of deck in which folder the image file should be copied</param>
        private static string CopyImageFileToDeckFolderAndReturnRelativeToDeckFolderPath(string absolutePathOfImageToCopy, bool questionOrAnswerImage, string deckName)
        {
            string relativeToDeckFolderFilePath = "";
            if (questionOrAnswerImage)
                relativeToDeckFolderFilePath = "question";
            else
                relativeToDeckFolderFilePath = "answer";
            int i = 0;
            while (File.Exists(Path.Combine(currentFolderWithDecksFullPath, deckName, relativeToDeckFolderFilePath + i.ToString())))
                i++;
            File.Copy(absolutePathOfImageToCopy, Path.Combine(currentFolderWithDecksFullPath, deckName, relativeToDeckFolderFilePath + i.ToString()));
            return relativeToDeckFolderFilePath + i.ToString();
        }
    }
}
