﻿using System;
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
using System.Windows.Media.Imaging;
using System.Drawing;



namespace Leitner_System_Transfered_2.Model
{
    static class FileManager
    {
        public static string currentFolderWithDecksFullPath { get; private set; }
        private static string defaultFolderWithDecksPath = Path.Combine(Environment.CurrentDirectory, "Decks");
        private static string defaultBackupFolder = Path.Combine(Environment.CurrentDirectory, "Backup");
        private static string defaultSettingPath= Path.Combine(Environment.CurrentDirectory, "settings.xml");
        private static bool readingSettings = false;
        //private static string defaultFolderWithDecksPath = "G:\\My Drive\\Планирование и отслеживание продуктивности\\Decks - Copy";
        //private static string defaultBackupFolder = "G:\\My Drive\\Программирование\\C#\\Проекты\\Leitner System\\Leitner System\\Backups";
        public static SettingsModel settings;

        ///<summary>
        ///[Problem with optimization] 1. Return list of all decks in folder (if the folder pass is empty so return decks from default folder)
        ///2. Return empty List if the process was faild
        ///3. Update current folder with decks path field
        ///4. If some decks were unable to read they are ignored and aren't include in outout collection
        ///5. Set parent deck for each card in loaded decks
        ///</summary>
        ///<param name="pathOfFolderWithDecks">Absolute path of folder that contains decks (if it is empty, so reading from default folder)</param>
        public static List<Deck> GetDecksFromFolder(string absolutePathOfFolderWithDecks = "")
        {
            if (String.IsNullOrEmpty(absolutePathOfFolderWithDecks))
            {
                if (settings == null)
                    CreateDefaultSettings();
                absolutePathOfFolderWithDecks = settings.AbsolutePathOfSaveDeckFolder;

            }
            if (!Directory.Exists(settings.AbsolutePathOfSaveDeckFolder))
                Directory.CreateDirectory(settings.AbsolutePathOfSaveDeckFolder);
            List<Deck> output = new List<Deck>();
            if (!Directory.Exists(absolutePathOfFolderWithDecks))
            {
                MessageBox.Show("Folder " + absolutePathOfFolderWithDecks + " was not found.");
                return output;
            }
            currentFolderWithDecksFullPath = absolutePathOfFolderWithDecks;
            string[] decksFilesNames = Directory.GetFiles(absolutePathOfFolderWithDecks);
            foreach (string deckFileName in decksFilesNames)
            {
                if (!deckFileName.Contains(".xml"))
                    continue;
                Deck deck = ReadDeckByFullPath(deckFileName);
                if (deck != null)
                    output.Add(deck);
                else
                    MessageBox.Show("Reading of deck from " + deckFileName + "was not sucssesful.");
            }
            MakeBackupOfDecksFromCurrentFolder(output);
            if (output.Count == 0)
                output = ReadAndConvertOldDecksToNewFormat(absolutePathOfFolderWithDecks);
            return output;
        }
        /// <summary>
        /// Read decks of old format from folder with decks and returns list of converted Deck
        /// </summary>
        /// <param name="absolutePathOfFolderWithDecks"></param>
        /// <returns></returns>
        public static List<Deck> ReadAndConvertOldDecksToNewFormat(string absolutePathOfFolderWithDecks = "")
        {
            if (String.IsNullOrEmpty(absolutePathOfFolderWithDecks))
            {
                if (settings == null)
                    CreateDefaultSettings();
                absolutePathOfFolderWithDecks = settings.AbsolutePathOfSaveDeckFolder;
                if (!Directory.Exists(settings.AbsolutePathOfSaveDeckFolder))
                    Directory.CreateDirectory(settings.AbsolutePathOfSaveDeckFolder);
            }
            List<Deck> output = new List<Deck>();
            if (!Directory.Exists(absolutePathOfFolderWithDecks))
            {
                MessageBox.Show("Folder " + absolutePathOfFolderWithDecks + " was not found.");
                return output;
            }
            currentFolderWithDecksFullPath = absolutePathOfFolderWithDecks;
            IEnumerable<string> decksDirectoriesNames = Directory.EnumerateDirectories(absolutePathOfFolderWithDecks);
            foreach (string deckDirectoryName in decksDirectoriesNames)
            {
                Deck deck = ReadOldDeckFromDeckFolderWithFullPath(deckDirectoryName);
                if (deck != null)
                    output.Add(deck);
                else
                    MessageBox.Show("Reading of deck from " + deckDirectoryName + "was not sucssesful.");
            }
            //Converting to new format
            foreach (Deck deck in output)
            {
                foreach (Card card in deck.Cards)
                {
                    string relativeQuestionImagePath = "";
                    string relativeAnswerImagePath = "";
                    if (!String.IsNullOrEmpty(card.RelativeToDeckFolderQuestionImagePath))
                        relativeQuestionImagePath = CopyImageFileToFolderWithDecksAndReturnRelativeToDeckFolderPath(Path.Combine(currentFolderWithDecksFullPath, deck.Name, card.RelativeToDeckFolderQuestionImagePath), true);
                    if (!String.IsNullOrEmpty(card.RelativeToDeckFolderAnswerImagePath))
                        relativeAnswerImagePath = CopyImageFileToFolderWithDecksAndReturnRelativeToDeckFolderPath(Path.Combine(currentFolderWithDecksFullPath, deck.Name, card.RelativeToDeckFolderAnswerImagePath),false);
                    card.RelativeToDeckFolderAnswerImagePath = relativeAnswerImagePath;
                    card.RelativeToDeckFolderQuestionImagePath = relativeQuestionImagePath;
                }
                SaveDeckOrUpdateDeckFile(deck);
            }
            return output;
        }
        /// <summary>
        /// Read deck specified by full pass of folder with deck. If reading was nor sucessfull returns null 
        /// </summary>
        /// <param name="absolutePathOfFolderWithDecks"></param>
        /// <returns></returns>
        private static Deck ReadOldDeckFromDeckFolderWithFullPath(string absolutePathOfFolderWithDecks)
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
            catch (Exception e)
            {
                MessageBox.Show("Error while reading from: " + deckFileFullPath + e.Message);
                return null;
            }
            return readingDeck;
        }
        /// <summary>
        /// Returns true if file is xml format
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static bool CheckIfTheFileIsXml(string filePath)
        {
            if (filePath.Contains("xml"))
                return true;
            return false;
        }
        /// <summary>
        /// Create defaul settings and save them at default location
        /// </summary>
        private static void CreateDefaultSettings()
        {
            SettingsModel defaultSettings = new SettingsModel();
            settings = defaultSettings;
            settings.AbsolutePathOfBackupFolder = defaultBackupFolder;
            settings.AbsolutePathOfSaveDeckFolder = defaultFolderWithDecksPath;
            settings.CurrentTrainingTemplate = -1;
            settings.MakeBackup = false;
            settings.TrainingTemplates = new List<TrainingTemplate>();
            settings.TrainingTemplates.Add(settings.GetDefaultTrainingTemplate());
            SaveSettings();
        }
        /// <summary>
        /// Read settings from specified path
        /// </summary>
        /// <param name="path"></param>
        private static void ReadExistingSettings(string path)
        {
            readingSettings = true;
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
            readingSettings = false;
        }
        /// <summary>
        /// Read existing settings or create new default one
        /// </summary>
        public static void UploadSettings()
        {
            string path = defaultSettingPath;
            if (!File.Exists(path))
                CreateDefaultSettings();
            else
                ReadExistingSettings(path);
        }
        /// <summary>
        /// Save settings in default location
        /// </summary>
        public static void SaveSettings()
        {
            if (settings == null||readingSettings)
                return;
            string path = defaultSettingPath;
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
            catch(Exception e)
            {
                MessageBox.Show("Settings was not saved"+e.Message);
            }
        }
        ///<summary>
        ///Return deck by its absolute path, if the process was faild return null
        ///</summary>
        ///<param name="pathOfFolderWithDecks">Absolute path of folder that contains decks</param>
        private static Deck ReadDeckByFullPath(string absolutePathOfDeckFile)
        {
            Deck readingDeck = null;
            if (!File.Exists(absolutePathOfDeckFile))
                return null;
            try
            {
                using (FileStream fs = new FileStream(absolutePathOfDeckFile, FileMode.Open))
                {
                    DataContractSerializer ds = new DataContractSerializer(typeof(Deck));
                    object deck = ds.ReadObject(fs) as Deck;
                    if (deck is Deck)
                        readingDeck = deck as Deck;
                    else
                    {
                        MessageBox.Show("Error while reading from: " + absolutePathOfDeckFile);
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error while reading from: " + absolutePathOfDeckFile + e.Message);
                return null;
            }
            return readingDeck;
        }
        /// <summary>
        /// Export Dictionary of queston-answer paors as excel file 
        /// </summary>
        /// <param name="absoluteFilePath"></param>
        /// <param name="input"></param>
        public static void ExportExcelFile(string absoluteFilePath, Dictionary<string,string> input)
        {
            if (String.IsNullOrEmpty(absoluteFilePath))
                return;
            Excel.Application application = new Excel.Application();
            application.Workbooks.Add();
            Excel.Worksheet workSheet = application.ActiveSheet;
            
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export was not susessfull " + absoluteFilePath + "\n" + "\n" + ex.Message + "\n");

            }
        }
        /// <summary>
        /// Import Dictionary of queston-answer pairs from excel file 
        /// </summary>
        /// <param name="absoluteFilePath"></param>
        /// <returns></returns>
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
            }
            //
            application.Quit();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(application);
            return output;
        }

        /// <summary>
        /// Find appropriate name for new folder in current folder with decks in format "prefix X" where X is integer
        /// </summary>
        /// <param name="prefix">prefix of new name for deck</param>
        /// <returns></returns>
        public static string FindNameForNewDeckInCurrentFolderWithDecks(string prefix="New deck")
        {
            if (!Directory.Exists(currentFolderWithDecksFullPath))
                return "";
            int i = 1;
            string path = Path.Combine(currentFolderWithDecksFullPath, prefix +" " + i.ToString()+".xml");
            while (File.Exists(path))
            {
                i++;
                path = Path.Combine(currentFolderWithDecksFullPath, prefix + " " + i.ToString()+".xml");
            }
            return (prefix + " " + i.ToString());
        }
        
        ///<summary>
        ///If deck folder doesn't exist create new one, if it already exists presaved file, then remove old deck file from folder, after that rename new deck file in this folder by the name of old file. 
        ///Return true if the process was sucessful and false in oposite case.
        ///</summary>
        ///<param name="deck">Deck to save</param>
        ///<param name="folderWithDecksPath">Folder where save deck (if it's empty so saving in current folder with decks)</param>
        public static bool SaveDeckOrUpdateDeckFile(Deck deck, string fullDeckFilePath = "")
        {
            string[] decksDirectoryFileNames;
            string oldDeckFileFullPath = "";
            string absolutePathOfFolderWithDecks = currentFolderWithDecksFullPath;
            bool automaticalSavingToCurrentDeckFolder = false;
            
            if (String.IsNullOrEmpty(fullDeckFilePath))
            {
                automaticalSavingToCurrentDeckFolder = true;
                if (!Directory.Exists(absolutePathOfFolderWithDecks))
                    Directory.CreateDirectory(absolutePathOfFolderWithDecks);
                decksDirectoryFileNames = Directory.GetFiles(absolutePathOfFolderWithDecks);
                oldDeckFileFullPath = "";
                for (int i = 0; i < decksDirectoryFileNames.Length; i++)
                {
                    if (decksDirectoryFileNames[i].Contains(deck.Name) && decksDirectoryFileNames[i].Contains(".xml"))
                        oldDeckFileFullPath = decksDirectoryFileNames[i];
                }
                fullDeckFilePath= Path.Combine(absolutePathOfFolderWithDecks, deck.Name + "_presaved.xml");
            }
            else
            {
                if (File.Exists(fullDeckFilePath))
                {
                    MessageBox.Show("The same deck " + fullDeckFilePath + " has already exists try to rename this deck and repeat saving again");
                    return false;
                }
            }
            //string fullDeckFilePath = Path.Combine(absolutePathOfFolderWithDecks, deck.Name + "_presaved.xml");

            try
            {
                using (FileStream fs = new FileStream(fullDeckFilePath, FileMode.Create))
                {
                    DataContractSerializer ds = new DataContractSerializer(typeof(Deck));
                    ds.WriteObject(fs, deck);
                }
                if (!String.IsNullOrEmpty(oldDeckFileFullPath))
                    File.Delete(oldDeckFileFullPath);
                if(automaticalSavingToCurrentDeckFolder)
                    File.Move(fullDeckFilePath, Path.Combine(absolutePathOfFolderWithDecks, deck.Name + ".xml"));
            }
            catch (Exception ex)
            {
                //
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
            foreach (Deck deck in decksToBackup)
            {
                string pathToSave = Path.Combine(path, deck.Name + ".xml");
                SaveDeckOrUpdateDeckFile(deck, pathToSave);
            }
        }
        ///<summary>
        ///Rename deck folder, file, and deck itself. If the process was faild return false and true in opposite case
        ///</summary>
        ///<param name="newName">New name of deck</param>
        ///<param name="deckNameToRename">Deck name that will be renamed</param>
        public static bool UpdateNameOfDeckDeckFileAndDeckFolder(Deck deck,string newName)
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
            if (newName == deck.Name)
                return false;
            string deckOldFilePath = Path.Combine(currentFolderWithDecksFullPath, deck.Name+".xml");
            string deckNewFilePath = Path.Combine(currentFolderWithDecksFullPath, newName + ".xml");
            if (File.Exists(deckNewFilePath))
            {
                MessageBox.Show("File with this name already exists");
                return false;
            }
            if (!File.Exists(deckOldFilePath))
            {
                MessageBox.Show("There is no folder to rename");
                return false;
            }
            try
            {
                File.Move(deckOldFilePath, deckNewFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("File was not saved " + deckOldFilePath + "\n" + "\n" + ex.Message + "\n");
                return false;
            }
            return true;
        }
        ///<summary>
        ///Remove deck from current folder with decks and all files which are related to this deck
        ///</summary>
        ///<param name="deckToDelete">Deck that will be delate</param>
        public static void DeleteDeckFromCurrentFolder(Deck deckToDelete)
        {
            string path = Path.Combine(currentFolderWithDecksFullPath, deckToDelete.Name + ".xml");
            if (!File.Exists(path))
                return;
            foreach(Card card in deckToDelete.Cards)
            {
                string questionImagePath = Path.Combine(currentFolderWithDecksFullPath, card.RelativeToDeckFolderQuestionImagePath);
                string answerImagePath = Path.Combine(currentFolderWithDecksFullPath, card.RelativeToDeckFolderAnswerImagePath);
                if (File.Exists(questionImagePath))
                    File.Delete(questionImagePath);
                if (File.Exists(answerImagePath))
                    File.Delete(answerImagePath);
            }
            File.Delete(path);
        }
        ///<summary>
        ///Copy image files from it's locations to deck folder if they are differnet from images that are related to card it replace them and return new names of images and convert them to byte array which is assignedto appropriatefield inside Card class
        ///</summary>
        ///<param name="currentCard">current card to save</param>
        public static void CopyNewImagesToDeckFolder(Card currentCard, string questionAbsoluteImagePath, string answerAbsoluteImagePath, out string relativeAnswerImagePath, out string relativeQuestionImagePath)
        {
            relativeQuestionImagePath = "";
            relativeAnswerImagePath = "";
            string oldQuestionImageAbsolitePath = Path.Combine(currentFolderWithDecksFullPath, currentCard.RelativeToDeckFolderQuestionImagePath);
            string oldAnswerImageAbsolitePath = Path.Combine(currentFolderWithDecksFullPath, currentCard.RelativeToDeckFolderAnswerImagePath);
            if (questionAbsoluteImagePath != oldQuestionImageAbsolitePath)
            {
                if (File.Exists(oldQuestionImageAbsolitePath))
                    File.Delete(oldQuestionImageAbsolitePath);
                if (!String.IsNullOrEmpty(questionAbsoluteImagePath))
                {
                    relativeQuestionImagePath = CopyImageFileToFolderWithDecksAndReturnRelativeToDeckFolderPath(questionAbsoluteImagePath, true);
                    currentCard.QuestionImageByte = ByteFromImageFile(questionAbsoluteImagePath);
                }
                    
            }
            else
                relativeQuestionImagePath = currentCard.RelativeToDeckFolderQuestionImagePath;
            if (answerAbsoluteImagePath != oldAnswerImageAbsolitePath)
            {
                if (File.Exists(oldAnswerImageAbsolitePath))
                    File.Delete(oldAnswerImageAbsolitePath);
                if (!String.IsNullOrEmpty(answerAbsoluteImagePath))
                {
                    relativeAnswerImagePath = CopyImageFileToFolderWithDecksAndReturnRelativeToDeckFolderPath(answerAbsoluteImagePath, false);
                    currentCard.AnswerImageByte = ByteFromImageFile(answerAbsoluteImagePath);
                }
                    
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
        private static string CopyImageFileToFolderWithDecksAndReturnRelativeToDeckFolderPath(string absolutePathOfImageToCopy, bool questionOrAnswerImage)
        {
            string relativeToDeckFolderFilePath = "";
            if (questionOrAnswerImage)
                relativeToDeckFolderFilePath = "question";
            else
                relativeToDeckFolderFilePath = "answer";
            int i = 0;
            while (File.Exists(Path.Combine(currentFolderWithDecksFullPath, relativeToDeckFolderFilePath + i.ToString())))
                i++;
            File.Copy(absolutePathOfImageToCopy, Path.Combine(currentFolderWithDecksFullPath, relativeToDeckFolderFilePath + i.ToString()));
            return relativeToDeckFolderFilePath + i.ToString();
        }
       /// <summary>
       /// Create BitmapImage fromimage file specified by pass. If process was failed it creates default image
       /// </summary>
       /// <param name="path"></param>
       /// <returns></returns>
        public static BitmapImage CreateImageWithFullPath(string path)
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
                return CreateDefaultImage();
            }
        }
        /// <summary>
        /// Creates default BitmapImage
        /// </summary>
        /// <returns></returns>
        private static BitmapImage CreateDefaultImage()
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
        /// <summary>
        /// Create image from byte array. If process was not sucessful it returns default image
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static BitmapImage ImageFromByteArray(Byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return null;
            MemoryStream stream = new MemoryStream(bytes);
            BitmapImage image = new BitmapImage();
            try
            {
                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
            }
            catch
            {
                return CreateDefaultImage();
            }
            return image;
        }
        /// <summary>
        /// Converts BitmapImage to Bitmap
        /// </summary>
        /// <param name="bitmapImage"></param>
        /// <returns></returns>
        public static Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);
                return new Bitmap(bitmap);
            }
        }
        /// <summary>
        /// Return byte array from image file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static byte[] ByteFromImageFile(string filePath)
        {
            if (String.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;
            BitmapImage image = FileManager.CreateImageWithFullPath(filePath);
            Bitmap bitmap = BitmapImageToBitmap(image);
            ImageConverter converter = new ImageConverter();
            try
            {
                byte[] output = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));
                return output;
            }
            catch
            {
                return new byte[0];
            }
        }
        /// <summary>
        /// Returns absolutepath of image from relative
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public static string PathOfImage(string relativePath)
        {
            return Path.Combine(currentFolderWithDecksFullPath, relativePath);
        }
        /// <summary>
        /// Read deck and decompress it (create images in folder with decks from bite arrays) if it is compressed
        /// </summary>
        /// <param name="absoluteFilePath"></param>
        /// <returns></returns>
        public static Deck DecompressDeck(string absoluteFilePath)
        {
            
            Deck decompressingDeck = ReadDeckByFullPath(absoluteFilePath);
            if(File.Exists(Path.Combine(currentFolderWithDecksFullPath, decompressingDeck.Name + ".xml")))
            {
                MessageBox.Show("Deck with thesame name as decompressing deck has already exists. Try to rename deck with name: " + decompressingDeck.Name + " and repeat again");
                return null;
            }
            if (!decompressingDeck.Compressed)
                return decompressingDeck;
            foreach (Card card in decompressingDeck.Cards)
            {
                BitmapImage questionImage;
                if (card.QuestionImageByte != null)
                {
                    questionImage = ImageFromByteArray(card.QuestionImageByte);
                    Bitmap bitmapQueston = BitmapImageToBitmap(questionImage);
                    int i = 0;
                    while (File.Exists(Path.Combine(currentFolderWithDecksFullPath, "question" + i.ToString())))
                        i++;
                    bitmapQueston.Save(Path.Combine(currentFolderWithDecksFullPath, "question" + i.ToString()));
                    card.RelativeToDeckFolderQuestionImagePath = "question" + i.ToString();
                }
                BitmapImage answerImage;
                if (card.AnswerImageByte != null)
                {
                    answerImage = ImageFromByteArray(card.AnswerImageByte);
                    Bitmap bitmapAnswer = BitmapImageToBitmap(answerImage);
                    int i = 0;
                    while (File.Exists(Path.Combine(currentFolderWithDecksFullPath, "answer" + i.ToString())))
                        i++;
                    bitmapAnswer.Save(Path.Combine(currentFolderWithDecksFullPath, "answer" + i.ToString()));
                    card.RelativeToDeckFolderAnswerImagePath = "answer" + i.ToString();
                }
            }
            decompressingDeck.Decompress();
            return decompressingDeck;
        }
    }
}
