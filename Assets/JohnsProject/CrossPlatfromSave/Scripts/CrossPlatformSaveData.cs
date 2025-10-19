using System.Xml;
using System.Xml.Serialization;

namespace JohnsProject.CrossPlatformSave
{
    /*
    The CrossPlatformSaveData class contains the data that needs to be saved.
    While saving the CrossPlatformSaveManager will save the state of this class
    into a file. In order add your own save classes all you need to do is to
    add your class as a variable and add [XmlElement("ClassName")] above it and
    then initialize your class in the constructor.
    */
    [XmlRoot("CrossPlatformSaveData")]
    public class CrossPlatformSaveData
    {
        // Sample implementation used by the DataSaveTest class in the Sample folder
        [XmlElement("DataToSave")]
        public DataToSave dataToSave;

        public CrossPlatformSaveData()
        {
            dataToSave = new DataToSave();
        }
    }
}

