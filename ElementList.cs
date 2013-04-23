#region Include libraries
using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Collections.Generic;
#endregion

namespace Platformer
{
    public class ElementList
    {
        public List<ElementDefinition> Elements;

        public ElementList loadDefinitions()
        {
            var fileStream = new FileStream("Content/ElementList.xml", FileMode.Open);
            var xmlSerializer = new XmlSerializer(typeof(ElementList));
            ElementList definitions = (ElementList)xmlSerializer.Deserialize(fileStream);
            return definitions;
        }
    }// End of ElementList Class
}
