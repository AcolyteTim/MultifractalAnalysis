using System.IO;
using System.Xml.Serialization;

namespace MultifractalAnalysis.Model.Functionality
{
    public class AppSettings
    {
        public string StandartExcelSavePathSetting { get; set; }
        public string ThresholdValueSetting { get; set; }
        public string ImageInversionSetting { get; set; }
        public string MinSignificantAreaSetting { get; set; }
        public string RectsSizesSetting { get; set; }
        public string VariableParameterSetting { get; set; }

        public void Save(string filename)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(AppSettings));
                xmls.Serialize(sw, this);
            }
        }
        public AppSettings Read(string filename)
        {
            using (StreamReader sw = new StreamReader(filename))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(AppSettings));
                return xmls.Deserialize(sw) as AppSettings;
            }
        }

    }
}
