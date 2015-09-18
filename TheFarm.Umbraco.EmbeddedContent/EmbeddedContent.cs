using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using umbraco.BasePages;
using umbraco.cms.businesslogic.datatype;
using umbraco.editorControls;
using editorctrls = umbraco.editorControls;
using editorctrls_datepicker = umbraco.editorControls.datepicker;
using editorctrls_simpleEditor = umbraco.editorControls.simpleEditor;
using editorctrls_textfield = umbraco.editorControls.textfield;
using editorctrls_textfieldmultiple = umbraco.editorControls.textfieldmultiple;
using editorctrls_tinyMCE3 = umbraco.editorControls.tinyMCE3;
using editorctrls_yesno = umbraco.editorControls.yesno;
using umbraco.interfaces;
using umbraco.uicontrols.TreePicker;
using DefaultData = umbraco.cms.businesslogic.datatype.DefaultData;

namespace TheFarm.Umbraco.EmbeddedContent
{
    public class EmbeddedContent : Panel
    {
        #region private fields

        //The following is a list of html elements commonly used below
        private const string HtmlDragHandle = "<div class=\"ECdraghandle\">&nbsp;</div>";
        private const string HtmlLiStart = "<li class=\"ECregular\">";
        private const string HtmlLiEnd = "</li>";
        private const string HtmlTextDivStart = "<div class=\"ECitemInner\">";
        private const string HtmlTextDivEnd = "</div>";
        private const string HtmlSpanStart = "<span>";
        private const string HtmlSpanEnd = "</span>";

        private Literal _list;
        private Literal _addBox1;
        private Literal _addBox2;

        /// <summary>
        /// Contains a comple xml representation of the data in the list.
        /// </summary>
        private HtmlInputHidden _hiddenXmlValue;

        /// <summary>
        /// Contains an Xml representation of the schema for an entity in the list. This is used 
        /// to a) traverse the m_hiddenXmlValue to make sense of the data and b) to add new entities.
        /// </summary>
        private HtmlInputHidden _hiddenXmlSchema;

        private bool _showLabel;
        private string _maxCount;

        #endregion


        #region properties

        /// <summary>
        /// The value for the content editor. Identical with the value of the m_hiddenXmlValue field
        /// or - if that is not set e.g. upon initialization - the m_value field which contains the
        /// data as retrieved from the database.
        /// </summary>
        private string _value;

        /// <summary>
        /// Property encapsulating the m_value field.
        /// </summary>
        public string Value
        {
            get
            {
                if (_hiddenXmlValue != null)
                {
                    return _hiddenXmlValue.Value;
                }
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        public string ValueFromRequest
        {
            get
            {
                string value = "";
                foreach (var key in Page.Request.Form.Keys)
                {
                    if (key.ToString().Contains("EChiddenXmlValue" + DTDId))
                    {
                        value = Page.Request.Form[key.ToString()];
                    }
                }
                return value;
            }
        }

        /// <summary>
        /// The m_prevalue field contains the property definition as saved by the prevalue editor.
        /// </summary>
        private string _prevalue;

        /// <summary>
        /// Prevalue property, has only a setter method.
        /// </summary>
        public string Prevalue
        {
            set { _prevalue = value; }
        }

        public int DTDId { get; set; }

        #endregion

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //if the prevalue settings are empty there is no need to go any further as no entities have been defined.
            if (string.IsNullOrEmpty(_prevalue)) return;

            SetAdditionalParameters();

            var divWidth = _showLabel ? "500px" : "700px";

            //add the list
            InstantiateList(divWidth);

            //add the add link
            var addLink = new Literal
            {
                ID = string.Format("addLink{0}", DTDId),
// ReSharper disable LocalizableElement
                Text = "<div class=\"ECaddLinkContainer\"><a href=\"#\" id=\"addLink" + DTDId + "\" title=\"add new item\" class=\"ECaddLink\"><div class=\"ECbigButton\"></div></a><div id=\"converter" + DTDId + "\"></div></div>"
// ReSharper restore LocalizableElement
            };
            Controls.Add(addLink);


            //add the add box
            _addBox1 = new Literal();
            _addBox1.Text += string.Format("<div class=\"ECaddBox\" id=\"addBox{0}\" style=\"width: " + divWidth + ";\">" +
                            "<input type=\"hidden\" id=\"hiddenEditId{0}\" />", DTDId);
            Controls.Add(_addBox1);

            Controls.Add(GenerateAddBox());

            _addBox2 = new Literal();
// ReSharper disable LocalizableElement
            _addBox2.Text += "<a href=\"#\" id=\"closeLink" + DTDId + "\" class=\"ECcloseLink\" title=\"close add box and discard values\"><div class=\"ECbigButton\"></div></a>";
            _addBox2.Text += "</div>";
// ReSharper restore LocalizableElement
            Controls.Add(_addBox2);

            Page.Init += PageInit;

        }

        static void PageInit(object sender, EventArgs e)
        {
            //Disable request validation
            BasePage.Current.EnableEventValidation = false;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                //this will only fail if the control itself has saved, in which case the data loading is taking care of elsewhere
                LoadControlsAndData();
            } finally
            {
            }

        }

        private void LoadControlsAndData()
        {
            //store the data xml in the hidden value
            MergeValueWithXmlSchema();

            _hiddenXmlValue = new HtmlInputHidden
            {
                ID = string.Format("EChiddenXmlValue{0}", DTDId),
                Value = Value,
                EnableViewState = false
            };
            Controls.Add(_hiddenXmlValue);
            Controls.Add(_hiddenXmlSchema);


            var cs = BasePage.Current.ClientScript;
            cs.RegisterClientScriptBlock(typeof(EmbeddedContent), "dojo", "<script src=\"http://ajax.googleapis.com/ajax/libs/dojo/1.5/dojo/dojo.xd.js\" type=\"text/javascript\" djConfig=\"parseOnLoad: true\"></script>", false);
            cs.RegisterClientScriptResource(typeof(EmbeddedContent), "TheFarm.Umbraco.EmbeddedContent.js.jquery.dragsort-0.4.1.min.js");
            cs.RegisterClientScriptResource(typeof(EmbeddedContent), "TheFarm.Umbraco.EmbeddedContent.js.dojox.xml.parser.js");

            var url = cs.GetWebResourceUrl(typeof(EmbeddedContent),
                                            "TheFarm.Umbraco.EmbeddedContent.css.EmbeddedContent.css");
            var cssLink = new Literal
                              {
// ReSharper disable LocalizableElement
                                  Text = "<link href=\"" + url + "\" rel=\"stylesheet\" type=\"text/css\" media=\"all\"></link>"
// ReSharper restore LocalizableElement
                              };
            Controls.Add(cssLink);

            //add the javascript
            var script =
                    "if(typeof TheFarm == 'undefined') var TheFarm = {};" + Environment.NewLine +
                    "if (!TheFarm.Umbraco) TheFarm.Umbraco = {};" + Environment.NewLine +
                    "if (!TheFarm.Umbraco.EmbeddedContent) TheFarm.Umbraco.EmbeddedContent = {};" + Environment.NewLine +
                    "TheFarm.Umbraco.EmbeddedContent.htmlDragHandle = '" + HtmlDragHandle + "';" + Environment.NewLine +
                    "TheFarm.Umbraco.EmbeddedContent.htmlLiStart = '" + HtmlLiStart + "';" + Environment.NewLine +
                    "TheFarm.Umbraco.EmbeddedContent.htmlLiEnd = '" + HtmlLiEnd + "';" + Environment.NewLine +
                    "TheFarm.Umbraco.EmbeddedContent.htmlTextDivStart = '" + HtmlTextDivStart + "';" + Environment.NewLine +
                    "TheFarm.Umbraco.EmbeddedContent.htmlTextDivEnd = '" + HtmlTextDivEnd + "';" + Environment.NewLine +
                    "TheFarm.Umbraco.EmbeddedContent.htmlSpanStart = '" + HtmlSpanStart + "';" + Environment.NewLine +
                    "TheFarm.Umbraco.EmbeddedContent.htmlSpanEnd = '" + HtmlSpanEnd + "';" + Environment.NewLine +
                    "TheFarm.Umbraco.EmbeddedContent.htmlRemoveButton = '<a href=\"#\" onclick=\"TheFarm.Umbraco.EmbeddedContent.RemoveItem($(this))\" class=\"remove\" title=\"remove\"><div class=\"ECbigButton\"></div></a>';" + Environment.NewLine +
                    "TheFarm.Umbraco.EmbeddedContent.htmlEditButton = '<a href=\"#\" onclick=\"TheFarm.Umbraco.EmbeddedContent.EditItem($(this))\" class=\"edit\" title=\"edit\"><div class=\"ECbigButton\"></div></a>';" +
                    "TheFarm.Umbraco.EmbeddedContent.maxCount = " + _maxCount + ";" + Environment.NewLine;

            script += "$(document).ready(function(){" + Environment.NewLine;

            //view function: opens the add form box
            script += "$(\"a#addLink" + DTDId + "\").click(function(){" + Environment.NewLine +
                        "TheFarm.Umbraco.EmbeddedContent.AddLinkClicked(" + DTDId + ", $(this));" + Environment.NewLine +
                      "});" + Environment.NewLine + Environment.NewLine;

            //view function: closes the add form box
            script += "$(\"a#closeLink" + DTDId + "\").click(function(){" +
                        "TheFarm.Umbraco.EmbeddedContent.CloseLinkClicked(" + DTDId + ");" + Environment.NewLine +
                      "});" + Environment.NewLine + Environment.NewLine;

            //handles both add and update functionality
            script += "$(\"a#addPropertyLink" + DTDId + "\").click(function(){" + Environment.NewLine +
                        "TheFarm.Umbraco.EmbeddedContent.AddPropertyClicked(" + DTDId + ");" + Environment.NewLine +
                      "});" + Environment.NewLine + Environment.NewLine;

            //loads the list of items on page load
            script += "TheFarm.Umbraco.EmbeddedContent.PopulateListFromHiddenValue(" + DTDId + ");" + Environment.NewLine +
                      "TheFarm.Umbraco.EmbeddedContent.InitializeDragSort(" + DTDId + ");" + Environment.NewLine +
                      "TheFarm.Umbraco.EmbeddedContent.SetBigButtonBackground(false);" + Environment.NewLine +
                      "TheFarm.Umbraco.EmbeddedContent.AddBlurToFields(" + DTDId + ");" + Environment.NewLine;

            if (!_showLabel)
            {
                script += "$('#sortListContainer" + DTDId + "').parent().parent().parent().prev().hide();" +
                               Environment.NewLine;
            }

            script += "});" + Environment.NewLine + Environment.NewLine;

            cs.RegisterClientScriptBlock(typeof(EmbeddedContent), "ECCombeddedContent_customScript" + DTDId, script, true);
            cs.RegisterClientScriptResource(typeof(EmbeddedContent), "TheFarm.Umbraco.EmbeddedContent.js.ECContentEditor.js");
        }

        private void SetAdditionalParameters()
        {
            int lastIndex = _prevalue.LastIndexOf("||");
            if (!(lastIndex >= 0)) return;
            var additionalParameters = _prevalue.Substring(lastIndex + 2);
            _prevalue = _prevalue.Remove(lastIndex);
            var addParams = additionalParameters.Split(new[] { ';' });
            _showLabel = true;
            foreach (var addParam in addParams)
            {
                if (addParam.StartsWith("showLabel:"))
                {
                    _showLabel = addParam.Substring(10) == "1";
                }else if (addParam.StartsWith("maxCount:"))
                {
                    _maxCount = addParam.Substring(9);
                }
            }
        }

        /// <summary>
        /// Builds up the proper value xml for the hidden field, merged with the schema.
        /// </summary>
        private void MergeValueWithXmlSchema()
        {
            var sbValue = new StringBuilder();
            sbValue.Append("<data>");

            var schema = new XmlDocument();
            schema.LoadXml(Uri.UnescapeDataString(_hiddenXmlSchema.Value));

            var plainValue = new XmlDocument();
            plainValue.LoadXml(Value);

            if (plainValue.DocumentElement != null && schema.DocumentElement != null)
            {
                //get the schema in a more usable condition
                var attributes = new Dictionary<string, string>();
                foreach (XmlNode property in schema.DocumentElement.ChildNodes)
                {
                    var sbAttribute = new StringBuilder();
                    if (property.Attributes != null && property.Attributes.Count > 0)
                    {
                        var id = "";
                        foreach (XmlAttribute xmlAttribute in property.Attributes)
                        {
                            if (xmlAttribute.Name == "propertyid")
                            {
                                id = xmlAttribute.Value;
                            }
                            else if (xmlAttribute.Name != "description" && 
                                xmlAttribute.Name != "name" && 
                                xmlAttribute.Name != "required" &&
                                xmlAttribute.Name != "validation")
                            {
                                if (!string.IsNullOrEmpty(xmlAttribute.Value))
                                {
                                    sbAttribute.Append(" " + xmlAttribute.Name + "=\"" + xmlAttribute.Value + "\"");
                                }
                            }
                        }
                        attributes.Add(id, sbAttribute.ToString());
                    }
                }


                foreach (XmlNode itemNode in plainValue.DocumentElement.ChildNodes)
                {
                    if (itemNode.Attributes != null)
                    {
                        sbValue.Append(string.Format("<item id=\"{0}\">", itemNode.Attributes["id"].Value));

                        foreach (XmlNode propertyNode in itemNode.ChildNodes)
                        {
                            if (propertyNode.Attributes != null && propertyNode.Attributes["propertyid"] != null)
                            {
                                var id = propertyNode.Attributes["propertyid"].Value;
                                sbValue.Append(string.Format("<{0} propertyid=\"{1}\"", propertyNode.Name, id));
                                if (attributes.ContainsKey(id))
                                {
                                    sbValue.Append(attributes[id]);
                                }
                                sbValue.Append(">");
                                sbValue.Append(System.Web.HttpUtility.HtmlEncode(propertyNode.InnerText));
                                sbValue.Append(string.Format("</{0}>", propertyNode.Name));
                            }
                        }

                        sbValue.Append("</item>");
                    }
                }
            }

            sbValue.Append("</data>");
            Value = Uri.EscapeDataString(sbValue.ToString());
        }

        /// <summary>
        /// Adds the item list div with an empty ul to the page controls. The ul will get populated upon page load using jQuery.
        /// </summary>
        private void InstantiateList(string divWidth)
        {
            _list = new Literal
            {
                ID = string.Format("list{0}", DTDId),
                Text =
// ReSharper disable LocalizableElement
                    "<div class=\"ECsortListContainer\" id=\"sortListContainer" + DTDId +
                    "\" style=\"width: " + divWidth + ";\">" +
                    "<ul class=\"ECsortList\" id=\"sortList" + DTDId + "\">" +
                    "</ul>" +
                    "</div>"
// ReSharper restore LocalizableElement
            };
            Controls.Add(_list);
        }

        /// <summary>
        /// Generates the add box to add new items to the list
        /// </summary>
        /// <returns>Html representation of the fields in the add box.</returns>
        /// <remarks>
        /// There are actually a few things happening here:
        /// 1. The prevalue properties are examined one after the other, 
        ///    for each item the appropriate control is added to the box (e.g. textstring, true/false).
        ///    The control gets the id field{DTDId}_{id of the prevalue property}
        /// 2. While looping through the prevalues the xml schema is build up alongside and stored in m_hiddenXmlSchema.
        ///    This schema is used to build the xml representation of the value for this property as well as to build the relationship
        ///    in JS between the data and the dynamically added fields of each item.
        /// </remarks>
        private Table GenerateAddBox()
        {
            var table = new Table();
            table.Attributes.Add("class", "ECaddBoxTable");
            TableRow row;
            TableCell cell;

            var xmlSchema = new StringBuilder();

            xmlSchema.Append("<item id=\"\">");

            //decode the list of controls in the prevalue field and add them to the controls
            var prevalueSplit = _prevalue.Split(new[] { "||" }, 1000, StringSplitOptions.None);

            foreach (var pv in prevalueSplit)
            {
                if (string.IsNullOrEmpty(pv.Trim())) continue;

                var id = string.Empty;
                var name = string.Empty;
                var alias = string.Empty;
                var description = string.Empty;
                var showInTitle = false;
                var type = "Textstring";
                var required = false;
                var validation = string.Empty;

                var attributes = pv.Split('|');
                foreach (var attribute in attributes)
                {
                    if (!string.IsNullOrEmpty(attribute.Trim()))
                    {
                        var attr = attribute.Trim();
                        if (attr.StartsWith("id:"))
                        {
                            id = attr.Substring(3).Trim();
                        }
                        if (attr.StartsWith("Name:"))
                        {
                            name = attr.Substring(5).Trim();
                        }
                        if (attr.StartsWith("Alias:"))
                        {
                            alias = attr.Substring(6).Trim();
                        }
                        if (attr.StartsWith("Description:"))
                        {
                            description = System.Web.HttpUtility.HtmlEncode(attr.Substring(12).Trim().Replace("&lt;", "<").Replace("&gt;", ">"));
                        }
                        if (attr.StartsWith("Show in title?"))
                        {
                            bool.TryParse(attr.Substring(14).Trim(), out showInTitle);
                        }
                        if (attr.StartsWith("Type:"))
                        {
                            type = attr.Substring(5).Trim();
                        }
                        if (attr.StartsWith("Required?"))
                        {
                            bool.TryParse(attr.Substring(9).Trim(), out required);
                        }
                        if (attr.StartsWith("Validation:"))
                        {
                            validation = attr.Substring(11).Trim().Replace("@@@", "|");
                        }
                        

                    }
                }
                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(name) &&
                    !string.IsNullOrEmpty(alias) && !string.IsNullOrEmpty(type))
                {
                    row = new TableRow();
                    cell = new TableCell
                               {
// ReSharper disable LocalizableElement
                                   Text = name + "<br /><small>" + description + "</small>"
// ReSharper restore LocalizableElement
                               };
                    cell.Attributes.Add("class", "ECfirst");
                    row.Cells.Add(cell);

                    cell = new TableCell();
                    if (type == "-87")
                    {
                        var dataTypeDefinition = new DataTypeDefinition(-87);
                        //var dt = dataTypeDefinition.DataType;
                        var dt = new editorctrls_tinyMCE3.tinyMCE3dataType();
                        dt.DataTypeDefinitionId = -87;
                        //cell.Controls.Add(new TextBox { Text = dataTypeDefinition.DataType.DataTypeName });
                        //addControlNew(dataTypeDefinition.DataType, cell, alias, int.Parse(id));
                        dt.DataEditor.Editor.ID = string.Format("prop_{0}", alias);
                        dt.Data.PropertyId = int.Parse(id);
                        //IDataEditor de = new dt.DataEditor.Editor();
                        cell.Controls.Add(dt.DataEditor.Editor);
                    }
                    else
                    {
                        switch (type)
                        {
                            case "Textstring":
                                var tfe =
                                    new editorctrls_textfield.TextFieldEditor(
                                        new DefaultData(new editorctrls_textfield.TextFieldDataType()))
                                        {ID = string.Format("field{0}_{1}", DTDId, id)};
                                cell.Controls.Add(tfe);
                                break;
                            case "Textbox multiple":
                                var tfm =
                                    new editorctrls_textfield.TextFieldEditor(
                                        new DefaultData(
                                            new editorctrls_textfieldmultiple.textfieldMultipleDataType()))
                                        {
                                            ID = string.Format("field{0}_{1}", DTDId, id),
                                            TextMode = TextBoxMode.MultiLine,
                                            Rows = 5
                                        };
                                cell.Controls.Add(tfm);
                                break;
                            case "True/false":
                                var yesno =
                                    new yesNo(
                                        new DefaultData(
                                            new editorctrls_yesno.YesNoDataType())) { ID = string.Format("field{0}_{1}", DTDId, id) };
                                yesno.LabelAttributes.Add("style", "display: none;");
                                cell.Controls.Add(yesno);
                                break;
                            case "Content picker":
                                cell.Controls.Add(new SimpleContentPicker
                                                      {
                                                          ID = string.Format("field{0}_{1}", DTDId, id)
                                                      });
                                break;
                            case "Media picker":
                                cell.Controls.Add(new SimpleMediaPicker
                                                      {
                                                          ID = string.Format("field{0}_{1}", DTDId, id)
                                                      });
                                break;
                            case "Simple editor":
                                var simpleEditor =
                                    new editorctrls_simpleEditor.SimpleEditor(
                                        new DefaultData(
                                            new editorctrls_simpleEditor.simpleEditorDataType())) { ID = string.Format("field{0}_{1}", DTDId, id) };
                                cell.Controls.Add(simpleEditor);
                                break;
                            case "Date picker":
                                var datePicker = new dateField(
                                    new DefaultData(
                                        new editorctrls_datepicker.DateDataType()));
                                cell.Controls.Add(datePicker);
                                cell.ID = string.Format("field{0}_{1}", DTDId, id);
                                break;
                            case "Richtext editor":
                                //ataType
                                //var d = new Document(123);
                                //Property p = d.getProperty("asdf");
                                //PropertyType.GetAll()
                                //var richtextBox = new TinyMCE(
                                //    new DefaultData(
                                //        new tinyMCE3dataType()), ((editorCtrls.tinymce.tinyMCEPreValueConfigurator)PrevalueEditor).Configuration);
                                break;
                            default:
                                var tfeDefault =
                                    new editorctrls_textfield.TextFieldEditor(
                                        new DefaultData(new editorctrls_textfield.TextFieldDataType()))
                                        {ID = string.Format("field{0}_{1}", DTDId, id)};
                                cell.Controls.Add(tfeDefault);
                                break;
                        }
                    }
                
                cell.Attributes.Add("class", "ECsecond");
                    row.Cells.Add(cell);

                    table.Rows.Add(row);

                    xmlSchema.Append(string.Format("<{0} propertyid=\"{1}\" name=\"{2}\" description=\"{3}\" type=\"{4}\" showintitle=\"{5}\" required=\"{6}\" validation=\"{7}\">|value{1}|</{0}>", alias, id, name, description, type, showInTitle, required, validation));
                }
            }

            xmlSchema.Append("</item>");

            _hiddenXmlSchema = new HtmlInputHidden
            {
                ID = string.Format("EChiddenXmlSchema{0}", DTDId),
                Value = Uri.EscapeDataString(xmlSchema.ToString()),
                EnableViewState = true
            };

            row = new TableRow();
            row.Cells.Add(new TableCell());
            cell = new TableCell();
// ReSharper disable LocalizableElement
            cell.Controls.Add(new Literal { Text = "<a href=\"#\" id=\"addPropertyLink" + DTDId + "\" title=\"create a new item with the above values\" class=\"ECaddPropertyLink\"><div class=\"ECbigButton\"></div></a>" });
// ReSharper restore LocalizableElement
            cell.Attributes.Add("class", "ECsecond");
            row.Cells.Add(cell);
            table.Rows.Add(row);

            return table;
        }

        private void addControlNew(IDataType dt, TableCell tc, string propertyTypeAlias, int propertyId)
        {
            //IDataType dt = p.PropertyType.DataTypeDefinition.DataType;
            dt.DataEditor.Editor.ID = string.Format("prop_{0}", propertyTypeAlias);
            dt.Data.PropertyId = propertyId;

            // fieldData.Alias = p.PropertyType.Alias;
            // ((Control) fieldData).ID = p.PropertyType.Alias;
            // fieldData.Text = p.Value.ToString();

            //_dataFields.Add(dt.DataEditor.Editor);


            //Pane pp = new Pane();
            //Control holder = new Control();
            //holder.Controls.Add(dt.DataEditor.Editor);
            tc.Controls.Add(dt.DataEditor.Editor);
            
            //// This is once again a nasty nasty hack to fix gui when rendering wysiwygeditor
            //if (dt.DataEditor.TreatAsRichTextEditor)
            //{
            //    tc.Controls.Add(dt.DataEditor.Editor);
            //}
            //else
            //{
            //    Panel ph = new Panel();
            //    ph.Attributes.Add("style", "padding: 0px 0px 0px 0px");
            //    ph.Controls.Add(pp);

            //    tp.Controls.Add(ph);
            //}
        }


        public void ReloadData()
        {
            LoadControlsAndData();
        }
    }
}