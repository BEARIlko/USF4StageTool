using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "library_physics_materials", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Library_Physics_Materials
    {
        [XmlAttribute("id")]
        public string ID;

        [XmlAttribute("name")]
        public string Name;

        [XmlElement(ElementName = "physics_material")]
        public Grendgine_Collada_Physics_Material[] Physics_Material;

        [XmlElement(ElementName = "asset")]
        public Grendgine_Collada_Asset Asset;

        [XmlElement(ElementName = "extra")]
        public Grendgine_Collada_Extra[] Extra;

    }
}

