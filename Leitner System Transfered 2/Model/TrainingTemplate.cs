using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Leitner_System_Transfered_2.Model
{
    [DataContract]
    public class TrainingTemplate
    {
        //[DataMember]
        //public int ReverseSettingsIndex { get; set; }
        [DataMember]
        public int maxCardsCount { get; set; }
        [DataMember]
        public string TemplateName { get; set; }
        //public bool TemplateIsSelectedForTraining { get; set; }

    }
}
