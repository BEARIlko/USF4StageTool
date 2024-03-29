using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class Grendgine_Collada_Instance_Formula
    {
        [XmlAttribute("sid")]
        public string sID;

        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("url")]
        public string URL;


        [XmlElement(ElementName = "setparam")]
        public Grendgine_Collada_Set_Param[] Set_Param;
    }
}

