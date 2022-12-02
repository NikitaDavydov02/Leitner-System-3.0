using System;
using System.Collections.Generic;
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
using Leitner_System_Transfered_2.ViewModel;
using System.Windows.Forms;
using Deck = Leitner_System_Transfered_2.Model.Deck;

namespace Leitner_System_Transfered_2.View
{
    /// <summary>
    /// Interaction logic for Training.xaml
    /// </summary>
    public partial class Training : Page
    {
        TrainingViewModel viewModel;
        //StartPage startPage;
        private BitmapImage deleteImage;
        private BitmapImage deleteImageDark;
        public Training()
        {
            InitializeComponent();
        }
        public Training(TrainingViewModel vm)
        {
            InitializeComponent();
            deleteImage = new BitmapImage(new Uri("..\\Assets\\DeleteSprite.png", UriKind.Relative));
            deleteImageDark = new BitmapImage(new Uri("..\\Assets\\DeleteSpriteDark.png", UriKind.Relative));
            deleteButtonImage.Source = deleteImageDark;
            this.Focus();
            viewModel = vm;
            this.DataContext = viewModel;
            viewModel.PropertyChanged += CurrentTrainingCardChangedHandler;
            cardPresentationAndEditingElement.SetViewModel(viewModel.CurrentTrainingCard);
            //cardPresentationAndEditingElement.ChangeTextBoxesFocusable(false
            cardPresentationAndEditingElement.ChangeFocusableOfElements(false);
            cardPresentationAndEditingElement.ChangeOrientationOfTextBoxes();
            this.Focusable = true;
            this.Focus();
        }

        private void CurrentTrainingCardChangedHandler(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentTrainingCard")
            {
                cardPresentationAndEditingElement.SetViewModel(viewModel.CurrentTrainingCard);
                this.Focus();
            }

        }

        private void correctAnswer_Click(object sender, RoutedEventArgs e)
        {
            viewModel.GetAnswer(true);
            this.Focus();
        }

        private void wrongAnswer_Click(object sender, RoutedEventArgs e)
        {
            viewModel.GetAnswer(false);
            this.Focus();
        }

        private void showAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ShowAnswer();
            this.Focus();
        }

        private void toHomeButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.LeavePage();
            EventHandler handler = TrainingFinished;
            if (handler != null)
                handler(this, new EventArgs());
        }
        private void trainingPage_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!this.IsFocused)
                return;
            if (e.Key == Key.Enter)
                viewModel.ShowAnswer();
            else if (e.Key == Key.Left && !e.IsRepeat)
                viewModel.GetAnswer(true);
            else if (e.Key == Key.Right && !e.IsRepeat)
                viewModel.GetAnswer(false);
            this.Focus();
            e.Handled = true;
        }

        private void trainingPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (viewModel.TrainingIsComleted)
                return;
            e.Handled = true;
            cardPresentationAndEditingElement.ChangeTextBoxesFocusable(false);
            this.Focusable = true;
            this.Focus();
        }
        private void earlyFinish_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Earlyfinish();
        }

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (viewModel.CurrentTrainingCard != null)
            {
                viewModel.CurrentTrainingCard.SaveThisCard();
                System.Windows.MessageBox.Show("Карта успешно сохранена");
            }
        }
        public event EventHandler TrainingFinished;

        private void deleteCard_Click(object sender, RoutedEventArgs e)
        {
            viewModel.DeleteCurrentCard();
        }
        public bool CheckForUnsavedChanges()
        {
            if (!viewModel.TrainingIsComleted)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Вы не завершили тренировку. Вы действительно хотите продолжить?", "", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                    return true;
                else
                    return false;
            }
            if (viewModel.CurrentTrainingCard != null && viewModel.CurrentTrainingCard.UnsavedChanges)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("У вас есть несохранённые изменения в картах. Вы действительно хотите продолжить?", "", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                    return true;
                else
                    return false;
            }
            return false;
        }

        private void changeAnswersScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void changeAnswers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void deleteCard_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            deleteButtonImage.Source = deleteImage;
        }

        private void deleteCard_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            deleteButtonImage.Source = deleteImageDark;
        }
    }
}
