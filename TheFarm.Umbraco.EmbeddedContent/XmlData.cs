using System.Xml;
using umbraco.cms.businesslogic.datatype;

namespace TheFarm.Umbraco.EmbeddedContent
{
    /// <summary>
    /// Inherits DefaultData, contains just one Method ToXML
    /// </summary>
    public class XmlData : DefaultData
    {
        /// <summary>
        /// Constructor. Only calls the base constructor.
        /// </summary>
        /// <param name="dataType">The BaseDataType used with this data</param>
        public XmlData(BaseDataType dataType)
            : base(dataType)
        {
        }

        /// <summary>
        /// Handles the conversion to Xml. Returns null if the root element is null, otherwise calls base.ToXml
        /// </summary>
        /// <param name="data">The data which needs to be converted.</param>
        /// <returns>the root node of the data</returns>
        public override XmlNode ToXMl(XmlDocument data)
        {
            if (Value != null)
            {
                var xmlDocument = new XmlDocument();
                if (string.IsNullOrEmpty(Value.ToString()))
                {
                    xmlDocument.LoadXml("<data></data>");
                }
                else
                {
                    xmlDocument.LoadXml(Value.ToString());
                }

                if (xmlDocument.DocumentElement != null)
                {
                    return data.ImportNode(xmlDocument.DocumentElement, true);
                }
                return null;
            }
            return base.ToXMl(data);
        }
    }
}