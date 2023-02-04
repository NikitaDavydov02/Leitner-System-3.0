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
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.IO;
using System.Collections.ObjectModel;
using Leitner_System_Transfered_2.ViewModel;


namespace Leitner_System_Transfered_2.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private StartPage startPage;
        private Training trainingPage;
        public MainWindow()
        {
            InitializeComponent();
            //frame.Navigate(new StartPage());
            ReloadStartPage();
        }

        private void StartPage_GoToSettings(object sender, EventArgs e)
        {
            Settings settingsPage = new Settings();
            settingsPage.GoToHomePage += SettingsPage_GoToHomePage;
            this.Content = settingsPage;
        }
        private void ReloadStartPage()
        {
            startPage = new StartPage();
            this.Content = startPage;
            startPage.TrainingStart += StartPage_TrainingStart;
            startPage.GoToSettings += StartPage_GoToSettings;
        }
        private void SettingsPage_GoToHomePage(object sender, EventArgs e)
        {
            ReloadStartPage();
        }

        private void StartPage_TrainingStart(object sender, EventArgs e)
        {
            TrainingStartEventArgs args = e as TrainingStartEventArgs;
            trainingPage = new Training(args.trainingViewModel);
            trainingPage.TrainingFinished += TrainingPage_TrainingFinished;
            this.Content = trainingPage;
        }

        private void TrainingPage_TrainingFinished(object sender, EventArgs e)
        {
            this.Content = startPage;
            startPage.TrainigIsFinished();
            //Reload decks
        }

        

        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            double controlsize = ((SystemParameters.PrimaryScreenWidth / 12) / 3 * 2) / 5 * 0.7;
            System.Windows.Application.Current.Resources.Remove("ControlFontSize");
            System.Windows.Application.Current.Resources.Add("ControlFontSize", controlsize);
        }

        private void mainWindow_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.Content == startPage)
            {
                if (startPage.CheckForUnsavedChanges())
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show("У вас есть несохранённые изменения в картах. Вы действительно хотите продолжить?", "", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.No)
                        e.Cancel = true;
                }
            }
            else if (this.Content == trainingPage)
            {
                if (trainingPage.CheckForUnsavedChanges())
                    e.Cancel = true;
            }
        }
    }
}
