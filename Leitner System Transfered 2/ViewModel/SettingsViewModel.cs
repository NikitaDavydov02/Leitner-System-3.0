using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leitner_System_Transfered_2.Model;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Leitner_System_Transfered_2.ViewModel
{
    class SettingsViewModel:INotifyPropertyChanged
    {
        public SettingsModel SettingsModel { get; private set; }
        public ObservableCollection<TrainingTemplateViewModel> TrainingTemplates { get; private set; }
        private List<TrainingTemplateViewModel> buffer;

        public SettingsViewModel()
        {
            SettingsModel = FileManager.settings;
            TrainingTemplates = new ObservableCollection<TrainingTemplateViewModel>();
            ReloadTemplateList();
            buffer = new List<TrainingTemplateViewModel>();
        }
        private TrainingTemplateViewModel CreaterainingTemplateViewModel(TrainingTemplate template)
        {
            TrainingTemplateViewModel templateViewModel = new TrainingTemplateViewModel(template);
            templateViewModel.PropertyChanged += TemplateSelectionChanged;
            return templateViewModel;
        }

        private void TemplateSelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            TrainingTemplateViewModel templateSender = sender as TrainingTemplateViewModel;
            if (templateSender == null)
                return;
            if (!templateSender.TemplateIsSelectedForTraining)
            {
                if(SettingsModel.CurrentTrainingTemplate == TrainingTemplates.IndexOf(templateSender))
                    SettingsModel.CurrentTrainingTemplate = -1;
                return;
            }
            SettingsModel.CurrentTrainingTemplate = TrainingTemplates.IndexOf(templateSender);
            foreach (TrainingTemplateViewModel templateViewModel in TrainingTemplates)
            {
                if (templateViewModel != sender)
                    templateViewModel.TemplateIsSelectedForTraining = false;
            }
        }

        private void ReloadTemplateList()
        {
            TrainingTemplates.Clear();
            if (SettingsModel.TrainingTemplates == null)
                return;
            for(int i = 0; i < SettingsModel.TrainingTemplates.Count; i++)
            {
                TrainingTemplates.Add(CreaterainingTemplateViewModel(SettingsModel.TrainingTemplates[i]));
                if (SettingsModel.CurrentTrainingTemplate == i)
                    TrainingTemplates[TrainingTemplates.Count - 1].TemplateIsSelectedForTraining = true;
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        public void SaveSettings()
        {
            FileManager.SaveSettings();
        }
        public void AddTrainingTemplate()
        {
            TrainingTemplate t = SettingsModel.CreateDefault();
 
            SettingsModel.TrainingTemplates.Add(t);
            TrainingTemplates.Add(CreaterainingTemplateViewModel(t));
        }
        public void CopyTemplatesInBuffer(List<int> indexesOfTemplatesToCopy)
        {
            SettingsModel.CopyTemplatesInBuffer(indexesOfTemplatesToCopy);
        }
        public void PasteTemplatesFromBuffer()
        {
            SettingsModel.PasteTemplatesFromBuffer();
            ReloadTemplateList();
        }
        public void DeleteSelectedTemplates(List<int> indexesOfTemplatesToDelete)
        {
            SettingsModel.DeleteSelectedTemplates(indexesOfTemplatesToDelete);
            ReloadTemplateList();
        }
    }
}
