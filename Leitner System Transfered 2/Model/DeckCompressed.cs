using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.IO;
namespace Leitner_System_Transfered_2.Model
{
    [DataContract]
    class DeckCompressed
    {
        //--------------------------------------------------------------------------------
        //------------------------------------- DATA MEMBERS ---------------------------------
        //--------------------------------------------------------------------------------
        [DataMember]
        public List<Card> Cards { get; set; }
        [DataMember]
        public string Name { get; set; }
    }
}
