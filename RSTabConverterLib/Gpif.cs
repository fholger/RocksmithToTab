using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RSTabConverterLib.Gpif
{
    [XmlRoot]
    class GPIF
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
    }
}
