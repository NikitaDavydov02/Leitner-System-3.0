using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.IO;
using System.Collections.ObjectModel;
using Leitner_System_Transfered_2.ViewModel;

namespace Leitner_System_Transfered_2.View
{
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : Page
    {
        private LeitnerSystemViewModel viewModel;
        private bool cardsInDeckIsActive = false;
        private bool deckListIsActive = false;
        //Light Images
        private BitmapImage deleteImage;
        private BitmapImage dontSaveImage;
        private BitmapImage folderImage;
        private BitmapImage plusImage;
        private BitmapImage settingsImage;
        private BitmapImage saveImage;
        private BitmapImage homeImage;
        private BitmapImage exportImage;
        private BitmapImage importImage;
        private BitmapImage findImage;
        private BitmapImage questionImage;
        //Light Images
        private BitmapImage deleteImageDark;
        private BitmapImage dontSaveImageDark;
        private BitmapImage folderImageDark;
        private BitmapImage plusImageDark;
        private BitmapImage settingsImageDark;
        private BitmapImage saveImageDark;
        private BitmapImage homeImageDark;
        private BitmapImage exportImageDark;
        private BitmapImage importImageDark;
        private BitmapImage findImageDark;
        private BitmapImage questionImageDark;
        public StartPage()
        {
            InitializeComponent();
            viewModel = new LeitnerSystemViewModel();
            DataContext = viewModel;
            viewModel.PropertyChanged += CurrentCardChanged;
            cardScrollViewer.HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Disabled;
            //Images inicialization
            deleteImage = new BitmapImage(new Uri("..\\Assets\\DeleteSprite.png", UriKind.Relative));
            dontSaveImage = new BitmapImage(new Uri("..\\Assets\\DontSaveSprite.png", UriKind.Relative));
            folderImage = new BitmapImage(new Uri("..\\Assets\\FolderSprite.png", UriKind.Relative));
            plusImage = new BitmapImage(new Uri("..\\Assets\\PlusSprite.png", UriKind.Relative));
            settingsImage = new BitmapImage(new Uri("..\\Assets\\SettingsSprite.png", UriKind.Relative));
            saveImage = new BitmapImage(new Uri("..\\Assets\\SaveSprite.png", UriKind.Relative));
            homeImage = new BitmapImage(new Uri("..\\Assets\\HomeSprite.png", UriKind.Relative));
            exportImage = new BitmapImage(new Uri("..\\Assets\\ExportSprite.png", UriKind.Relative));
            importImage = new BitmapImage(new Uri("..\\Assets\\ImportSprite.png", UriKind.Relative));
            findImage = new BitmapImage(new Uri("..\\Assets\\FindSprite.png", UriKind.Relative));
            questionImage = new BitmapImage(new Uri("..\\Assets\\Question.png", UriKind.Relative));
            ////Dark images inicialization
            deleteImageDark = new BitmapImage(new Uri("..\\Assets\\DeleteSpriteDark.png", UriKind.Relative));
            dontSaveImageDark = new BitmapImage(new Uri("..\\Assets\\DontSaveSpriteDark.png", UriKind.Relative));
            folderImageDark = new BitmapImage(new Uri("..\\Assets\\FolderSpriteDark.png", UriKind.Relative));
            plusImageDark = new BitmapImage(new Uri("..\\Assets\\PlusSpriteDark.png", UriKind.Relative));
            settingsImageDark = new BitmapImage(new Uri("..\\Assets\\SettinsSpriteDark.png", UriKind.Relative));
            saveImageDark = new BitmapImage(new Uri("..\\Assets\\SaveSpriteDark.png", UriKind.Relative));
            homeImageDark = new BitmapImage(new Uri("..\\Assets\\HomeSpriteDark.png", UriKind.Relative));
            exportImageDark = new BitmapImage(new Uri("..\\Assets\\ExportSpriteDark.png", UriKind.Relative));
            importImageDark = new BitmapImage(new Uri("..\\Assets\\ImportSpriteDark.png", UriKind.Relative));
            findImageDark = new BitmapImage(new Uri("..\\Assets\\FindSpriteDark.png", UriKind.Relative));
            questionImageDark = new BitmapImage(new Uri("..\\Assets\\QuestionDark.png", UriKind.Relative));
            //ButtonsINicialization
            deleteDeckButtonImage.Source = deleteImage;
            chooseFolderButtonImage.Source = folderImage;
            newDeckButtonImage.Source = plusImage;
            settingsButtonImage.Source = settingsImage;
            newCardButtonImage.Source = plusImageDark;
            deleteCardButtonImage.Source = deleteImageDark;
            importButtonImage.Source = importImageDark;
            exportButtonImage.Source = exportImageDark;
            findButtonImage.Source = findImageDark;
            helpButtonImage.Source = questionImage;
        }
        private void CurrentCardChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentCard")
                cardPresentationAndEditingElement.SetViewModel(viewModel.CurrentCard);
        }
        private void startTraining_Click(object sender, RoutedEventArgs e)
        {
            TrainingViewModel trainingViewModel = viewModel.StartNewTraining();
            if (trainingViewModel == null)
                return;
            OnTrainingStart(new TrainingStartEventArgs(trainingViewModel));
        }
        private void cardsInDeck_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.UpdateCurrentSelectedCard(cardsInDeck.SelectedIndex);
        }
        private int initialIndexInDeckList = -1;
        private void decksList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(!viewModel.UpdateCurrentDeckIndex(decksList.SelectedIndex))
                decksList.SelectedIndex = initialIndexInDeckList;
            else
                initialIndexInDeckList = decksList.SelectedIndex;
        }
        private void deckSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.CurrentDeck == null)
                return;
            viewModel.CurrentDeck.SaveRenameThisDeck();
        }
        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SelectAllDecks();
        }
        private void ReverseAll_Click(object sender, RoutedEventArgs e)
        {
            //viewModel.ReverseAllDecks();
        }
        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (cardsInDeckIsActive)
            {
                List<int> indexesOfSelectedItems = new List<int>();
                int i = 0;
                foreach (CardViewModel item in cardsInDeck.Items)
                {
                    if (cardsInDeck.SelectedItems.Contains(item))
                        indexesOfSelectedItems.Add(i);
                    i++;
                }
                viewModel.CopyCardsInBuffer(indexesOfSelectedItems);
            }
            else if (deckListIsActive)
            {
                List<int> indexesOfSelectedDecks = new List<int>();
                int i = 0;
                foreach (DeckViewModel deck in decksList.Items)
                {
                    if (decksList.SelectedItems.Contains(deck))
                        indexesOfSelectedDecks.Add(i);
                    i++;
                }
                viewModel.CopyDecksInBuffer(indexesOfSelectedDecks);
            }
        }
        private void cardsInDeck_GotFocus(object sender, RoutedEventArgs e)
        {
            cardsInDeckIsActive = true;
            e.Handled = true;
        }
        private void cardsInDeck_LostFocus(object sender, RoutedEventArgs e)
        {
            cardsInDeckIsActive = false;
            e.Handled = true;
        }
        private void decksList_GotFocus(object sender, RoutedEventArgs e)
        {
            deckListIsActive = true;
            e.Handled = true;
        }
        private void decksList_LostFocus(object sender, RoutedEventArgs e)
        {
            deckListIsActive = false;
            e.Handled = true;
        }
        private void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void Paste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (cardsInDeckIsActive)
            {
                viewModel.PasteCardsFromBuffer();
            }
            else if (deckListIsActive)
            {
                viewModel.PasteDecksFromBuffer();
            }
        }
        private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void DeleteSelectedCards()
        {
            List<int> indexesOfSelectedItems = new List<int>();
            int i = 0;
            foreach (CardViewModel item in cardsInDeck.Items)
            {
                if (cardsInDeck.SelectedItems.Contains(item))
                    indexesOfSelectedItems.Add(i);
                i++;
            }
            viewModel.DeleteSelectedCards(indexesOfSelectedItems);
        }
        private void DeleteSelectedDecks()
        {
            List<int> indexesOfSelectedDecks = new List<int>();
            int i = 0;
            foreach (DeckViewModel deck in decksList.Items)
            {
                if (decksList.SelectedItems.Contains(deck))
                    indexesOfSelectedDecks.Add(i);
                i++;
            }
            viewModel.DeleteSelectedDecks(indexesOfSelectedDecks);
        }
        private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (cardsInDeckIsActive)
                DeleteSelectedCards();
            else if (deckListIsActive)
                DeleteSelectedDecks();
        }
        private void ScrollViewer_GotFocus(object sender, RoutedEventArgs e)
        {
            cardsInDeckIsActive = true;
            e.Handled = true;
        }
        private void ScrollViewer_LostFocus(object sender, RoutedEventArgs e)
        {
            cardsInDeckIsActive = false;
            e.Handled = true;
        }
        private void deckScrollViewer_LostFocus(object sender, RoutedEventArgs e)
        {
            deckListIsActive = false;
            e.Handled = true;
        }
        private void deckScrollViewer_GotFocus(object sender, RoutedEventArgs e)
        {
            deckListIsActive = true;
            e.Handled = true;
        }
        private void New_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (cardsInDeckIsActive)
                viewModel.AddCardToCurrentDeck();
            else if (deckListIsActive)
                viewModel.AddDeck();
        }
        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            viewModel.ChooseFolder();
        }
        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (cardsInDeckIsActive)
                SaveSelectedCards();
            else if (deckListIsActive || sender == deckSaveButton || sender == deckName)
            {
                viewModel.CurrentDeck.SaveRenameThisDeck();
            }
        }
        private void SaveSelectedCards()
        {
            List<int> indexesOfSelectedItems = new List<int>();
            int i = 0;
            foreach (CardViewModel item in cardsInDeck.Items)
            {
                if (cardsInDeck.SelectedItems.Contains(item))
                    indexesOfSelectedItems.Add(i);
                i++;
            }
            viewModel.SaveSelectedCards(indexesOfSelectedItems);
        }
        private void deleteDeckButton_Click_1(object sender, RoutedEventArgs e)
        {
            DeleteSelectedDecks();
        }
        private void deleteCardButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedCards();
        }

        private void newDeckButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.AddDeck();
        }

        private void deleteDeckButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedDecks();
        }

        private void newCardButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.AddCardToCurrentDeck();
        }

        private void deleteCardButton_Click_1(object sender, RoutedEventArgs e)
        {
            DeleteSelectedCards();
        }

        private void deckScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
        public void TrainigIsFinished()
        {
            viewModel.TrainingIsFinished();
        }
        public event EventHandler TrainingStart;
        public event EventHandler GoToSettings;
        public void OnTrainingStart(TrainingStartEventArgs args)
        {
            EventHandler handler = TrainingStart;
            if (handler != null)
                handler(this, args);
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            EventHandler handler = GoToSettings;
            if (handler != null)
                handler(this, new EventArgs());
        }

        private void ReverseSetting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (viewModel != null)
                viewModel.GeneralReverseSettingsChanged(ReverseSetting.SelectedIndex);
        }
        public bool CheckForUnsavedChanges()
        {
            return !viewModel.CheckForUnsavedCardsDoWeContinue();
        }

        private void findCard_TextChanged(object sender, TextChangedEventArgs e)
        {
            //viewModel.UpdateCardListByCardFindRequest(findCard.Text);
        }

        private void startPage_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void StackPanel_GotFocus(object sender, RoutedEventArgs e)
        {

        }
        private void ChangePageFocusable(bool value)
        {

        }
        private void startPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            cardPresentationAndEditingElement.ChangeFocusableOfElements(false);
            this.Focus();
            cardPresentationAndEditingElement.ChangeFocusableOfElements(true);
        }

        private void importExcel_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel == null)
                return;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.ShowDialog();
            viewModel.ImportExcelFileToCurrentDeck(openFileDialog.FileName);
        }

        private void exportEacel_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel == null)
                return;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "|*.xlsx";
            saveFileDialog.DefaultExt = "xlsx";
            saveFileDialog.ShowDialog();
            viewModel.ExportCurrentDeckToExcelFile(saveFileDialog.FileName);
        }

        private void chooseFolderButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ChooseFolder();
        }

        private void settingsButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            settingsButtonImage.Source = settingsImageDark;
        }

        private void settingsButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            settingsButtonImage.Source = settingsImage;
        }

        private void chooseFolderButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            chooseFolderButtonImage.Source = folderImageDark;
        }

        private void chooseFolderButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            chooseFolderButtonImage.Source = folderImage;
        }

        private void newDeckButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            newDeckButtonImage.Source = plusImageDark;
        }

        private void deleteDeckButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            deleteDeckButtonImage.Source = deleteImageDark;
        }
        private void deleteDeckButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            deleteDeckButtonImage.Source = deleteImage;
        }

        private void newDeckButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            newDeckButtonImage.Source = plusImage;
        }

        private void newCardButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            newCardButtonImage.Source = plusImage;
        }

        private void newCardButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            newCardButtonImage.Source = plusImageDark;
        }

        private void deleteCardButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            deleteCardButtonImage.Source = deleteImage;
        }

        private void deleteCardButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            deleteCardButtonImage.Source = deleteImageDark;
        }

        private void toggleButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void importExcel_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            importButtonImage.Source = importImage;
        }

        private void importExcel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            importButtonImage.Source = importImageDark;
        }

        private void exportEacel_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            exportButtonImage.Source = exportImage;
        }

        private void exportEacel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            exportButtonImage.Source = exportImageDark;
        }

        private void findButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            findButtonImage.Source = findImage;
        }

        private void findButton_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void findButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            findButtonImage.Source = findImageDark;
        }

        private void helpButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("www.yandex.ru");
        }

        private void helpButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            helpButtonImage.Source = questionImage;
        }

        private void helpButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            helpButtonImage.Source = questionImageDark;
        }
    }
    public class TrainingStartEventArgs : EventArgs
    {
        public TrainingViewModel trainingViewModel { get; private set; }
        public TrainingStartEventArgs(TrainingViewModel trainingViewModel)
        {
            this.trainingViewModel = trainingViewModel;
        }
    }
}
