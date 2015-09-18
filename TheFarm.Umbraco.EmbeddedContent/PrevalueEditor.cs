using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.propertytype;
using umbraco.interfaces;
using umbraco.uicontrols;

namespace TheFarm.Umbraco.EmbeddedContent
{
    /// <summary>
    /// The PrevalueEditor for the EmbeddedContent. Contains the UI and functionality the Umbraco admin needs to configure this data type.
    /// </summary>
    public sealed class PrevalueEditor : PlaceHolder, IDataPrevalue
    {
        #region Private variables

        /// <summary>
        /// Holds the datatype reference which is given upon instantiation
        /// </summary>
        private readonly BaseDataType _datatype;

        /// <summary>
        /// The main global controls which make up the prevalue editor.
        /// </summary>
        private Literal _propertyList;
        private Literal _addBox;
        private Literal _addLink;
        private HtmlInputHidden _hiddenValue;
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        private CheckBox _showLabel;
        private TextBox _maxCount;
        // ReSharper restore FieldCanBeMadeReadOnly.Local
        private string _propertyConfiguration;
        private HtmlInputHidden _hiddenSettings;

        //The following is a list of html elements commonly used in the code below to formt he dynamic Html
        private const string HtmlDragHandle = "<div class=\"ECdraghandle\">&nbsp;</div>";
        private const string HtmlLiStart = "<li class=\"ECregular\">";
        private const string HtmlLiEnd = "</li>";
        private const string HtmlTextDivStart = "<div class=\"ECitemInner\">";
        private const string HtmlTextDivEnd = "</div>";
        private const string HtmlRemoveButton = "<a href=\"#\" onclick=\"EC.RemoveItem($(this))\" title=\"remove\" class=\"remove\"><div class=\"ECbigButton\"></div></a>";
        private const string HtmlEditButton = "<a href=\"#\" onclick=\"elem = $(this).parent(); EC.EditProperty(elem); return false;\" title=\"edit\" class=\"edit\"><div class=\"ECbigButton\"></div></a>";
        private const string HtmlSpanStart = "<span>";
        private const string HtmlSpanEnd = "</span>";

        #endregion


        #region Control events

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                DisplayProperties();
            }
        }

        #endregion


        #region Constructors

        /// <summary>
        /// Instantiates the PrevalueEditor components.
        /// </summary>
        /// <param name="datatype">The datatype to use with this editor</param>
        public PrevalueEditor(BaseDataType datatype)
        {
            //instantiate the local m_datatype field with the given datatype
            _datatype = datatype;

            //The enclosing Umbraco panel to hold all other elements for this data type
            var ppEditPanel = new PropertyPanel { Text = "Content controls" };

            //Instantiate the property list, the add box, the add link, the JS custom scripts and the hidden value
            InstantiateHiddenValue(ppEditPanel);
            InstantiateHelpText(ppEditPanel);
            InstantiatePropertyList(ppEditPanel);
            InstantiateAddBox(ppEditPanel);
            InstantiateAddLink(ppEditPanel);
            InstantiateCustomScripts();
            Controls.Add(ppEditPanel);

            var ppShowLabelPanel = new PropertyPanel { Text = "Show label" };
            _showLabel = new CheckBox
                              {
                                  ID = "showLabel",
                                  Text = "",
                                  Checked = true
                              };
            ppShowLabelPanel.Controls.Add(_showLabel);
            Controls.Add(ppShowLabelPanel);

            var ppMaxCountPanel = new PropertyPanel { Text = "Max number of items (0 = no limit)" };
            _maxCount = new TextBox
            {
                ID = "maxCount",
                Text = ""
            };
            ppMaxCountPanel.Controls.Add(_maxCount);
            Controls.Add(ppMaxCountPanel);
        }

        #region Custom controls instatiation

        private static void InstantiateHelpText(PropertyPanel ppEditPanel)
        {
            var sb = new StringBuilder();
            sb.Append("<div class=\"ECtop\">");
            sb.Append("<a href=\"#\" id=\"helpLink\" class=\"EChelplink\" title=\"show or hide help text\" onclick=\"TheFarm.Umbraco.EmbeddedContent.ToggleHelp();\"><div class=\"backgroundSpritePrevalue off\"></div></a>");
            sb.Append("<a href=\"#\" id=\"showValidation\" class=\"ECshowValidation\" title=\"hide or show validation in the list below\" onclick=\"TheFarm.Umbraco.EmbeddedContent.ToggleValidation();\"><div class=\"backgroundSpritePrevalue off\"></div></a>");
            sb.Append("<a href=\"#\" id=\"showRequired\" class=\"ECshowRequired\" title=\"hide or show required information in the list below\" onclick=\"TheFarm.Umbraco.EmbeddedContent.ToggleRequired();\"><div class=\"backgroundSpritePrevalue off\"></div></a>");
            sb.Append("<a href=\"#\" id=\"showTitle\" class=\"ECshowTitle\" title=\"hide or show 'show in title' information in the list below\" onclick=\"TheFarm.Umbraco.EmbeddedContent.ToggleTitle();\"><div class=\"backgroundSpritePrevalue off\"></div></a>");
            sb.Append("<a href=\"#\" id=\"showDescription\" class=\"ECshowDescription\" title=\"hide or show description in the list below\" onclick=\"TheFarm.Umbraco.EmbeddedContent.ToggleDescription();\"><div class=\"backgroundSpritePrevalue off\"></div></a>");
            sb.Append("<a href=\"#\" id=\"showType\" class=\"ECshowType\" title=\"hide or show type information in the list below\" onclick=\"TheFarm.Umbraco.EmbeddedContent.ToggleType();\"><div class=\"backgroundSpritePrevalue off\"></div></a>");
            sb.Append("</div>");
            sb.Append("<div id=\"helpText\"><div class=\"helpTextInner\">");
            sb.Append("This is a quick reference like overview of the control, you can find more information on how it works ");
            sb.Append("at various blog posts on <a href=\"http://farmcode.org\" target=\"_blank\">farmcode.org</a>. <br /><br />");
            sb.Append("You can think of the Embedded Content control as a complex data type. It produces pretty much the same ");
            sb.Append("XML as if you were creating a new document type (= create a new Embedded Content data type), add ");
            sb.Append("a range of properties to it of certain types (= add a new field to the Embedded Content data type list) and ");
            sb.Append("then allow the doc type to be placed under nother doc type. <br /><br />");
            sb.Append("Start by clicking on the 'add' below to open the add property box. Here is an explanation of the individual entries:");
            sb.Append("<br /><br />");
            sb.Append("<table>");
            sb.Append("<tr>");
            sb.Append("<td>Name</td><td>the name of the property, just for the editor</td>");
            sb.Append("</tr>");
            sb.Append("<tr>");
            sb.Append("<td>Alias</td><td>this is the name of the Xml node when data for this schema gets saved (see below for an example)</td>");
            sb.Append("</tr>");
            sb.Append("<tr>");
            sb.Append("<td>Type</td><td>the type of this property</td>");
            sb.Append("</tr>");
            sb.Append("<tr>");
            sb.Append("<td>Description</td><td>a short description for the purposes of the editor</td>");
            sb.Append("</tr>");
            sb.Append("<tr>");
            sb.Append("<td>Show in title?</td><td>if ticked then this property will be displayed in the content list which the content editor sees</td>");
            sb.Append("</tr>");
            sb.Append("<tr>");
            sb.Append("<td>Required?</td><td>tick if this field will be mandatory</td>");
            sb.Append("</tr>");
            sb.Append("<tr>");
            sb.Append("<td>Validation</td><td>enter a regular expression for validation on the content node</td>");
            sb.Append("</tr>");
            sb.Append("</table>");
            sb.Append("<br />");
            sb.Append("After you click on Add in the box the property will be added to your list. Once you have more than one property ");
            sb.Append("in the list you will be able to sort them by clicking on the darker drag handle to the left of each item ");
            sb.Append("and putting the item where you want it to be.");
            sb.Append("<br /><br />");
            sb.Append("EXAMPLE:");
            sb.Append("<br />");
            sb.Append("The following content definition");
            sb.Append("<br />");
            sb.Append("Property 1: Name:The first property; Alias: first; Type: Textstring; Required: true; Validation: ");
            sb.Append("<br />");
            sb.Append("Property 2: Name:The second property; Alias: second; Type: True/false; Required: false; Validation: ^[a-z]*$");
            sb.Append("<br /><br />");
            sb.Append("will be saved to the database as");
            sb.Append("<br />");
            sb.Append("&lt;data&gt;");
            sb.Append("<br />");
            sb.Append("&nbsp;&nbsp;&lt;item id=\"1\"&gt;");
            sb.Append("<br />");
            sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;&lt;first&gt;{some value}&lt;/first&gt;");
            sb.Append("<br />");
            sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;&lt;second&gt;{some value}&lt;/second&gt;");
            sb.Append("<br />");
            sb.Append("&nbsp;&nbsp;&lt;/item&gt;");
            sb.Append("<br />");
            sb.Append("&nbsp;&nbsp;&lt;item id=\"2\"&gt;");
            sb.Append("<br />");
            sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;&lt;first&gt;{some value}&lt;/first&gt;");
            sb.Append("<br />");
            sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;&lt;second&gt;{some value}&lt;/second&gt;");
            sb.Append("<br />");
            sb.Append("&nbsp;&nbsp;&lt;/item&gt;");
            sb.Append("<br />");
            sb.Append("&lt;/data&gt;");
            sb.Append("<br /><br />");
            sb.Append("</div></div>");
            var helpText = new Literal
                               {
                                   Text = sb.ToString()
                               };
            ppEditPanel.Controls.Add(helpText);
        }

        /// <summary>
        /// Instantiates the property list. It will be populated with data in the On_Load event.
        /// </summary>
        /// <param name="ppEditPanel">The Panel to add the control to</param>
        private void InstantiatePropertyList(PropertyPanel ppEditPanel)
        {
            _propertyList = new Literal
            {
                ID = string.Format("propertyList{0}", _datatype.DataTypeDefinitionId)
            };
            ppEditPanel.Controls.Add(_propertyList);
        }

        /// <summary>
        /// Instantiates the add form box.
        /// </summary>
        /// <param name="ppEditPanel">The Panel to add the control to</param>
        private void InstantiateAddBox(PropertyPanel ppEditPanel)
        {
            _addBox = new Literal
            {
                ID = "addBox"
            };
// ReSharper disable LocalizableElement
            _addBox.Text = "<div id=\"addBox\" class=\"ECaddBoxPrevalues\">" +
                           "<input type=\"hidden\" id=\"hiddenEditId\" />" +
                           "<table cellpadding=\"0\" cellspacing=\"0\">" +
                           "<tr>" +
                           "<td clase=\"vt\">" +
                           "Name:" +
                           "</td>" +
                           "<td class=\"pl10\">" +
                           "<input type=\"textbox\" id=\"name\" size=\"77\" />" +
                           "</td>" +
                           "</tr>" +
                           "<tr>" +
                           "<td class=\"vt pt10\">" +
                           "Alias:" +
                           "</td>" +
                           "<td class=\"pl10 pt10\">" +
                           "<input type=\"textbox\" id=\"alias\" size=\"77\" />" +
                           "</td>" +
                           "</tr>" +
                           "<tr>" +
                           "<td class=\"vt pt10\">" +
                           "Type:" +
                           "</td>" +
                           "<td class=\"pl10 pt10\">" +
                           "<select id=\"type\">" +
                           "<option value=\"Textstring\">Textstring</option>" +
                           "<option value=\"Textbox multiple\">Textbox multiple</option>" +
                           "<option value=\"True/false\">True/false</option>" +
                           "<option value=\"Content picker\">Content picker</option>" +
                           "<option value=\"Media picker\">Media picker</option>" +
                           "<option value=\"Simple editor\">Simple editor</option>" +
                           "<option value=\"Date picker\">Date picker</option>" +
                           "<option value=\"Richttext editor\">Richtext editor</option>";
            //foreach (var dataTypeDefinition in DataTypeDefinition.GetAll())
            //{
            //    _addBox.Text += "<option value=\"" + dataTypeDefinition.Id + "\">" + dataTypeDefinition.DataType.Id + ", " + dataTypeDefinition.Text + "</option>";
            //}
            _addBox.Text += "</select>" +
                           "</td>" +
                           "</tr>" +
                           "<tr>" +
                           "<td class=\"vt pt10\">" +
                           "Description:<br /><small>please do not use '|' or '||'</small>" +
                           "</td>" +
                           "<td class=\"pl10 pt10\">" +
                           "<textarea id=\"description\" cols=\"47\" rows=\"5\"></textarea>" +
                           "</td>" +
                           "</tr>" +
                           "<tr>" +
                           "<td class=\"vt pt10\">" +
                           "Show in title?" +
                           "</td>" +
                           "<td class=\"pl10 pt10\">" +
                           "<input type=\"checkbox\" id=\"showInTitle\" />" +
                           "</td>" +
                           "</tr>" +
                           "<tr>" +
                           "<td class=\"vt pt10\">" +
                           "Required?" +
                           "<br /><small>If selected this will be a mandatory field</small>" +
                           "</td>" +
                           "<td class=\"pl10 pt10 vt\">" +
                           "<input type=\"checkbox\" id=\"required\" />" +
                           "</td>" +
                           "</tr>" +
                           "<tr>" +
                           "<td class=\"vt pt10\">" +
                           "Validation:" +
                           "<br /><small>You can provide a regular expression that will be checked upon submission</small>" +
                           "</td>" +
                           "<td class=\"pl10 pt10 vt\">" +
                           "<input type=\"textbox\" id=\"validation\" size=\"77\" />" +
                           "</td>" +
                           "</tr>" +
                           "<tr>" +
                           "<td>" +
                           "</td>" +
                           "<td class=\"pl10 pt10\">" +
                           "<a href=\"#\" id=\"addPropertyLink\" title=\"add new property\"><div class=\"ECbigButton\"></div></a>" +
                           "</td>" +
                           "</tr>" +
                           "</table>" +
                           "<a href=\"#\" id=\"closeLink\" title=\"close\"><div class=\"ECbigButton\"></div></a>" +
                           "</div>";
            // ReSharper restore LocalizableElement
            ppEditPanel.Controls.Add(_addBox);
        }

        /// <summary>
        /// Instantiates the hidden value control. It will be populated with data in the On_Load event
        /// </summary>
        /// <param name="ppEditPanel">The Panel to add the control to</param>
        private void InstantiateHiddenValue(PropertyPanel ppEditPanel)
        {
            _hiddenValue = new HtmlInputHidden
            {
                ID = "EChiddenValue",
                EnableViewState = false
            };
            ppEditPanel.Controls.Add(_hiddenValue);
            _hiddenSettings = new HtmlInputHidden
            {
                ID = "EChiddenSettings",
                EnableViewState = false
            };
            ppEditPanel.Controls.Add(_hiddenSettings);
        }

        /// <summary>
        /// Adds all the scripts to the control.
        /// </summary>
        private void InstantiateCustomScripts()
        {
            var cs = BasePage.Current.ClientScript;
            //cs.RegisterClientScriptInclude("dojox", "http://ajax.googleapis.com/ajax/libs/dojo/1.5/dojo/dojo.xd.js");
            //TheFARM logo and link
            var url = cs.GetWebResourceUrl(typeof(PrevalueEditor), "TheFarm.Umbraco.EmbeddedContent.images.grown_on_TheFARM.jpg");
            var urlSprite = cs.GetWebResourceUrl(typeof(PrevalueEditor), "TheFarm.Umbraco.EmbeddedContent.images.Buttons.gif");
            var urlBigButtons = cs.GetWebResourceUrl(typeof(PrevalueEditor), "TheFarm.Umbraco.EmbeddedContent.images.BigButtons.png");
            
            var script =
                         "if(typeof TheFarm == 'undefined') var TheFarm = {};" + Environment.NewLine +
                         "if (!TheFarm.Umbraco) TheFarm.Umbraco = {};" + Environment.NewLine +
                         "if (!TheFarm.Umbraco.EmbeddedContent) TheFarm.Umbraco.EmbeddedContent = {};" + Environment.NewLine +
                         "TheFarm.Umbraco.EmbeddedContent.FarmLogoUrl = '" + url + "';" + Environment.NewLine +
                         "TheFarm.Umbraco.EmbeddedContent.BackgroundSpriteUrl = '" + urlSprite + "';" + Environment.NewLine +
                         "TheFarm.Umbraco.EmbeddedContent.BigButtonsUrl = '" + urlBigButtons + "';" + Environment.NewLine +
                         "TheFarm.Umbraco.EmbeddedContent.HtmlDragHandle = '" + HtmlDragHandle + "';" + Environment.NewLine +
                         "TheFarm.Umbraco.EmbeddedContent.HtmlLiStart = '" + HtmlLiStart + "';" + Environment.NewLine +
                         "TheFarm.Umbraco.EmbeddedContent.HtmlLiEnd = '" + HtmlLiEnd + "';" + Environment.NewLine +
                         "TheFarm.Umbraco.EmbeddedContent.HtmlTextDivStart = '" + HtmlTextDivStart + "';" + Environment.NewLine +
                         "TheFarm.Umbraco.EmbeddedContent.HtmlTextDivEnd = '" + HtmlTextDivEnd + "';" + Environment.NewLine +
                         "TheFarm.Umbraco.EmbeddedContent.HtmlRemoveButton = '" + HtmlRemoveButton + "';" + Environment.NewLine +
                         "TheFarm.Umbraco.EmbeddedContent.HtmlEditButton = '" + HtmlEditButton + "';" + Environment.NewLine +
                         "TheFarm.Umbraco.EmbeddedContent.HtmlSpanStart = '" + HtmlSpanStart + "';" + Environment.NewLine +
                         "TheFarm.Umbraco.EmbeddedContent.HtmlSpanEnd = '" + HtmlSpanEnd + "';" + Environment.NewLine;

            cs.RegisterClientScriptResource(typeof(PrevalueEditor), "TheFarm.Umbraco.EmbeddedContent.js.jquery.dragsort-0.4.1.min.js");
            cs.RegisterClientScriptBlock(typeof(PrevalueEditor), "ECPrevalueEditor_customScript", script, true);
            cs.RegisterClientScriptResource(typeof(PrevalueEditor), "TheFarm.Umbraco.EmbeddedContent.js.ECPrevalueEditor.js");

            var cssurl = cs.GetWebResourceUrl(typeof(EmbeddedContent),
                                            "TheFarm.Umbraco.EmbeddedContent.css.EmbeddedContent.css");
            var cssLink = new Literal
            {
// ReSharper disable LocalizableElement
                Text = "<link href=\"" + cssurl + "\" rel=\"stylesheet\" type=\"text/css\" media=\"all\"></link>"
// ReSharper restore LocalizableElement
            };
            Controls.Add(cssLink);
        }

        /// <summary>
        /// Instantiates the add link control.
        /// </summary>
        /// <param name="ppEditPanel">The Panel to add the control to</param>
        private void InstantiateAddLink(PropertyPanel ppEditPanel)
        {
            _addLink = new Literal
            {
                ID = "link",
// ReSharper disable LocalizableElement
                Text = "<div class=\"ECaddLinkContainer\"><a href=\"#\" id=\"addLink\" title=\"add a new property\"><div class=\"ECbigButton\"></div></a></div>"
// ReSharper restore LocalizableElement
            };
            ppEditPanel.Controls.Add(_addLink);
        }

        #endregion

        #endregion


        #region Display properties

        /// <summary>
        /// Will be called in the OnLoad event and when the prevalues get saved.
        /// Adds the container and the individual properties to the page. Also copies the short version to the hidden value.
        /// </summary>
        private void DisplayProperties()
        {
            _propertyConfiguration = GetPropertyConfiguration();
            var retrievedProperties = RetrievedProperties();

            //set the property list inner html and the hidden value
            _propertyList.Text =
// ReSharper disable LocalizableElement
                "<div id=\"sortListContainer\" class=\"ECsortListContainer\" style=\"clear: both; width: 500px;\">" +
                    "<ul id=\"sortList\" class=\"ECsortList\">" +
                        retrievedProperties +
                    "</ul>" +
                "</div>";
// ReSharper restore LocalizableElement

            _hiddenValue.Value = _propertyConfiguration;
        }

        /// <summary>
        /// Takes the custom separated string from the db and creates list items.
        /// </summary>
        /// <returns>string containing markup of LIs</returns>
        private string RetrievedProperties()
        {
            var sbRetrievedProperties = new StringBuilder();
            if (!string.IsNullOrEmpty(_propertyConfiguration))
            {
                SetAdditionalParameters();
                var items = _propertyConfiguration.Split(new[] { "||" }, 1000, StringSplitOptions.None);
                foreach (var item in items)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        var properties = item.Split(new[] { '|' });
                        var sbAssembledItem = new StringBuilder();
                        foreach (var property in properties)
                        {
                            if (!string.IsNullOrEmpty(property))
                            {
                                if (property.StartsWith("id"))
                                {
                                    sbAssembledItem.Append(HtmlSpanStart + property.Replace("id:", "").Trim() +
                                                           HtmlSpanEnd);
                                }
                                else if (property.StartsWith(" Validation:"))
                                {
                                    sbAssembledItem.Append(property.Replace("@@@", "|"));
                                } 
                                else
                                {
                                    sbAssembledItem.Append(property + ";");
                                }
                            }
                        }
                        sbRetrievedProperties.Append(
                            string.Format("{0}{1}{2}{3}{4}{5}{6}{7}", HtmlLiStart, HtmlDragHandle, HtmlTextDivStart, sbAssembledItem, HtmlEditButton, HtmlRemoveButton, HtmlTextDivEnd, HtmlLiEnd));
                    }
                }
            }
            return sbRetrievedProperties.ToString();
        }

        private void SetAdditionalParameters()
        {
            int lastIndex = _propertyConfiguration.LastIndexOf("||");
            if (!(lastIndex >= 0)) return;
            var additionalParameters = _propertyConfiguration.Substring(lastIndex + 2);
            Log.Add(LogTypes.Notify, -1, "additional parameters: " + additionalParameters);
            _propertyConfiguration = _propertyConfiguration.Remove(lastIndex);
            var addParams = additionalParameters.Split(new[] { ';' });
            bool foundSettings = false;
            foreach (var addParam in addParams)
            {
                if (addParam.StartsWith("showLabel:"))
                {
                    _showLabel.Checked = addParam.Substring(10) == "1";
                    //Log.Add(LogTypes.Notify, -1, "substring is: " + addParam.Substring(10));
                }else if (addParam.StartsWith("maxCount:"))
                {
                    _maxCount.Text = (string.IsNullOrEmpty(addParam.Substring(9)) ? "0" : addParam.Substring(9));
                }else if (addParam.StartsWith("showType"))
                {
                    _hiddenSettings.Value = addParam;
                    foundSettings = true;
                }
            }
            if (!foundSettings)
            {
                _hiddenSettings.Value =
                    "showType=1,showDescription=0,showTitle=0,showRequired=1,showValidation=0";
            }
        }

        #endregion


        public string GetPropertyConfiguration()
        {
            //retrieve the stored configuration from the database
            string value = "";
            var lvalue = Application.SqlHelper.ExecuteScalar<object>(
                "select value from cmsDataTypePreValues where datatypenodeid = @datatypenodeid",
                Application.SqlHelper.CreateParameter("@datatypenodeid", _datatype.DataTypeDefinitionId));
            if (lvalue != null)
            {
                value = lvalue.ToString();
            }

            return value;
        }


        #region IDataPrevalue members

        /// <summary>
        /// The save event, executed when the datatype instance is saved. Essentially reads out the value in the hidden control and saves it to the database.
        /// </summary>
        /// <remarks>
        /// It is the responsibility of the client side to keep the hidden value (which contains the actual property configuration) in a correct state and 
        /// sync'd to the visual output. The stored string gets only decoded through .Net when the control is inititialized, the whole user interaction
        /// is done via Javascript.
        /// </remarks>
        public void Save()
        {
            if (_datatype != null)
            {
                _datatype.DBType = DBTypes.Ntext;

                Application.SqlHelper.ExecuteNonQuery(
                    "delete from cmsDataTypePreValues where datatypenodeid = @dtdefid",
                    Application.SqlHelper.CreateParameter("@dtdefid", _datatype.DataTypeDefinitionId));

                var propertyListContent = Page.Request.Form[_hiddenValue.UniqueID] + 
                    "||showLabel:" + (_showLabel.Checked ? "1" : "0") + ";" + 
                    "maxCount:" + (string.IsNullOrEmpty(_maxCount.Text) ? "0" : _maxCount.Text) + ";" +
                    Page.Request[_hiddenSettings.UniqueID];

                Application.SqlHelper.ExecuteNonQuery(
                    "insert into cmsDataTypePreValues(datatypenodeid, [value], sortorder, alias) values (@dtdefid, @value, 0, 'myCustomValue')",
                    Application.SqlHelper.CreateParameter("@dtdefid", _datatype.DataTypeDefinitionId),
                    Application.SqlHelper.CreateParameter("@value", string.IsNullOrEmpty(propertyListContent) ? "|" : propertyListContent));
            }

            DisplayProperties();
        }

        /// <summary>
        /// Returns this specific PrevalueEditor instance.
        /// </summary>
        public Control Editor
        {
            get { return this; }
        }

        #endregion
    }
}