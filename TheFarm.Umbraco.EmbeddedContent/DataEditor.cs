using System;
using System.IO;
using System.Web;
using System.Xml;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.interfaces;

namespace TheFarm.Umbraco.EmbeddedContent
{
    /// <summary>
    /// The EmbeddedContent DataEditor class. It sets the Id and Name for this Umbraco data type, hooks up the PrevalueEditor and Data properties
    /// with the correct classes to be used with EmbeddedContent and hooks up the OnInit and OnSave methods.
    /// </summary>
    public class DataEditor : AbstractDataEditor
    {
        /// <summary>
        /// The Id for this data type
        /// </summary>
        public override Guid Id
        {
            get
            {
                return new Guid("454545AB-1234-4321-ABCD-1234567890AB");
            }
        }

        /// <summary>
        /// The Name for this data type
        /// </summary>
        public override string DataTypeName
        {
            get { return "Embedded Content"; }
        }

        /// <summary>
        /// The actual control as displayed to the Umbraco content editor
        /// </summary>
        private EmbeddedContent m_control;

        /// <summary>
        /// The prevalue editor used to configure the data type
        /// </summary>
        private PrevalueEditor m_prevalueEditor;

        /// <summary>
        /// The prevalue editor used to configure the data type
        /// </summary>
        public override IDataPrevalue PrevalueEditor
        {
            get
            {
                if (m_prevalueEditor == null)
                {
                    m_prevalueEditor = new PrevalueEditor(this);
                }
                return m_prevalueEditor;
            }
        }

        /// <summary>
        /// The data to be used with this data type (in this case we're creating our own instance of XmlData)
        /// </summary>
        private XmlData m_data;

        /// <summary>
        /// The data to be used with this data type
        /// </summary>
        public override IData Data
        {
            get
            {
                if (m_data == null)
                {
                    m_data = new XmlData(this);
                }
                return m_data;
            }
        }

        /// <summary>
        /// Empty constructor. Instantiates a new control (the one that is shown to the content editor) and populates the RenderControl property with it.
        /// Hooks up the OnInit and OnSave events.
        /// </summary>
        public DataEditor()
        {
            //Instantiate a new editor control for this data type
            Initialize();
        }

        private void Initialize()
        {
            //check if the custom node name service is in place
            var filePath = HttpContext.Current.Server.MapPath("~/umbraco/webservices/UmbracoNodeNameService.asmx");
            if (!File.Exists(filePath))
            {
                lock (filePath)
                {
                    if (!File.Exists(filePath))
                    {
                        using (var sw = new StreamWriter(File.Create(filePath)))
                        {
                            sw.Write(EmbeddedContentResource.UmbracoNodeNameService);
                        }
                    }
                }
            }

            m_control = new EmbeddedContent();
            RenderControl = m_control;

            //hook up OnInit and OnSave events
            m_control.Init += DataEditorControl_OnInit;
            DataEditorControl.OnSave += DataEditorControl_OnSave;

        }

        /// <summary>
        /// The OnSave method handles the saving of the data from the control to the database. Actually it just prepares the data (as xml) and
        /// populates the Data.Value property with it, Umbraco will do the rest. For that to happen the Value of the control needs to be 
        /// retrieved (itself an XmlDocument, yet not in the schema we want to store it) and then looped through node by node to create the 
        /// actual XmlDocument to save.
        /// </summary>
        /// <param name="e">event arguments</param>
        void DataEditorControl_OnSave(EventArgs e)
        {
            try
            {
                var doc = new XmlDocument();
                var req = Uri.UnescapeDataString(m_control.ValueFromRequest);
                doc.LoadXml(req);
                //doc.LoadXml(m_control.Value);
                var newDoc = new XmlDocument();
                if (doc.DocumentElement != null)
                {
                    newDoc.LoadXml("<data></data>");
                    //strip out unnecessary info
                    XmlNode root = doc.DocumentElement;
                    foreach (XmlNode itemNode in root.ChildNodes)
                    {
                        XmlNode newItemNode = newDoc.CreateElement(itemNode.Name);

                        foreach (XmlNode property in itemNode.ChildNodes)
                        {
                            XmlNode newPropertyNode = newDoc.CreateElement(property.Name);
                            //newPropertyNode.InnerText = property.InnerText;

                            if (property.Attributes != null)
                            {
                                //add the 'propertyid' attribute
                                XmlAttribute propertyId = newDoc.CreateAttribute("propertyid");
                                propertyId.Value = property.Attributes["propertyid"] != null ? property.Attributes["propertyid"].Value : "not found for id: " + property.Attributes["name"];
                                if (newPropertyNode.Attributes != null)
                                {
                                    newPropertyNode.Attributes.Append(propertyId);
                                }

                                //add the value
                                if (property.Attributes["type"] != null)
                                {
                                    if (property.Attributes["type"].Value == "Simple editor")
                                    {
                                        var value = HttpUtility.HtmlDecode(property.InnerXml);
                                        var cdataSection = newDoc.CreateCDataSection(value);
                                        newPropertyNode.AppendChild(cdataSection);
                                    }
                                    else
                                    {
                                        newPropertyNode.InnerText = property.InnerText;
                                    }
                                }
                                else
                                {
                                    newPropertyNode.InnerText = property.InnerText;
                                }
                            }

                            newItemNode.AppendChild(newPropertyNode);
                        }

                        if (itemNode.Attributes != null)
                        {
                            XmlAttribute itemId = newDoc.CreateAttribute("id");
                            itemId.Value = itemNode.Attributes["id"].Value;
                            if (newItemNode.Attributes != null)
                            {
                                newItemNode.Attributes.Append(itemId);
                            }
                        }


                        if (newDoc.DocumentElement != null)
                        {
                            newDoc.DocumentElement.AppendChild(newItemNode);
                        }
                    }
                }
                else
                {
                    newDoc.LoadXml("<data></data>");
                }

                Data.Value = newDoc.InnerXml;

                m_control.Value = newDoc.InnerXml;
                //m_control.ReloadData();
            }
            catch (XmlException xe)
            {
                Log.Add(LogTypes.Error, -1, ("Failed to save Embedded Content data: " + xe.Message + ", " + xe.StackTrace));
                var doc = new XmlDocument();
                doc.Load("<data></data>");
                Data.Value = doc.InnerXml;
            }
        }

        /// <summary>
        /// The OnInit method handles the initial setup of the controls as well as the initial data population.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DataEditorControl_OnInit(object sender, EventArgs e)
        {
            //The DTDId contains the unique Id for this data type. This is necessary in case of multiple controls of this data type on one page/node.
            m_control.DTDId = GetPropertyTypeId(m_data.PropertyId);

            //get the prevalue configuration
            var prevalue = ((PrevalueEditor)PrevalueEditor).GetPropertyConfiguration();
            m_control.Prevalue = prevalue;

            //get the actual saved data
            var xmlDocument = new XmlDocument();
            try
            {
                xmlDocument.LoadXml(Data.Value.ToString());
            }
            catch (Exception)
            {
                xmlDocument.LoadXml("<data></data>");
            }

            //Set the Value property of the control to the retrieved XML
            m_control.Value = xmlDocument.InnerXml;
        }

        /// <summary>
        /// Retrieves the ID for the specific instance of the data type on the document type. A bit more
        /// complicated than needed maybe with going back to the database, but it seems to be the 
        /// best solution at them moment.
        /// </summary>
        /// <param name="propertyId">The version id for this property</param>
        /// <returns>the id for the specific instance of the data type</returns>
        private static int GetPropertyTypeId(int propertyId)
        {
            var helper = Application.SqlHelper;
            return helper.ExecuteScalar<int>("SELECT propertytypeid FROM cmsPropertyData WHERE id = @propertyId",
                                      Application.SqlHelper.CreateParameter("@propertyId", propertyId));
        }
    }
}