using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.IO;
using Leitner_System_Transfered_2.Model;

namespace Leitner_System_Transfered_2.Model
{
    [DataContract]
    public class SettingsModel
    {
        [DataMember]
        public bool MakeBackup { get; set; }
        [DataMember]
        public string AbsolutePathOfBackupFolder { get; set; }
        [DataMember]
        public string AbsolutePathOfSaveDeckFolder { get; set; }
        [DataMember]
        public List<TrainingTemplate> TrainingTemplates { get; set; }
        [DataMember]
        public int CurrentTrainingTemplate { get; set; }

        List<TrainingTemplate> buffer = new List<TrainingTemplate>();
        public TrainingTemplate GetDefaultTrainingTemplate()
        {
            TrainingTemplate output = new TrainingTemplate();
            output.maxCardsCount = 100;
            //output.ReverseSettingsIndex = 0;
            output.TemplateName = "Новый шаблон";
            return output;
        }

        public void CopyTemplatesInBuffer(List<int> indexesOfTemplatesToCopy)
        {
            if (buffer == null)
                buffer = new List<TrainingTemplate>();
            buffer.Clear();
            foreach (int index in indexesOfTemplatesToCopy)
                buffer.Add(TrainingTemplates[index]);
        }
        public void PasteTemplatesFromBuffer()
        {
            foreach (TrainingTemplate template in buffer)
            {
                TrainingTemplate templateCopy = new TrainingTemplate();
                templateCopy.maxCardsCount = template.maxCardsCount;
                //templateCopy.ReverseSettingsIndex = template.ReverseSettingsIndex;
                templateCopy.TemplateName = template.TemplateName;
                
                TrainingTemplates.Add(templateCopy);
            }
        }
        public void DeleteSelectedTemplates(List<TrainingTemplate> templatesToRemove)
        {
            TrainingTemplate currentTemplate = null;
            if (CurrentTrainingTemplate >= 0 && CurrentTrainingTemplate < TrainingTemplates.Count)
                currentTemplate = TrainingTemplates[CurrentTrainingTemplate];
            foreach (TrainingTemplate template in templatesToRemove)
                TrainingTemplates.Remove(template);
            if (currentTemplate != null)
                CurrentTrainingTemplate = TrainingTemplates.IndexOf(currentTemplate);
            else
                CurrentTrainingTemplate = -1;
        }
    }
}
