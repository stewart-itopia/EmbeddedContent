using System;
using System.Collections.Generic;
using System.Web.UI;
using umbraco.BusinessLogic;

namespace TheFarm.Umbraco.EmbeddedContent.umbraco.EmbeddedContent
{
    public partial class UpgradeSchema : UserControl
    {
        private Dictionary<int, string> prevalues;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                FillPrevalues();
                if (prevalues != null && prevalues.Count > 0)
                {
                    var oldValueFound = false;
                    foreach (var prevalue in prevalues)
                    {
                        if (PrevalueIsOld(prevalue.Value))
                        {
                            oldValueFound = true;
                            break;
                        }
                    }
                    if (oldValueFound)
                        start.Visible = true;
                }
            }
        }

        private bool PrevalueIsOld(string prevalue)
        {
            if (string.IsNullOrEmpty(prevalue) || prevalue.Contains("||"))
            {
                return false;
            }
            return true;
        }


// ReSharper disable InconsistentNaming
        protected void go_Click(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            int count = 0;
            try
            {
                FillPrevalues();

                if (prevalues != null && prevalues.Count > 0)
                {
                    foreach (var prevalue in prevalues)
                    {
                        if (PrevalueIsOld(prevalue.Value))
                        {
                            //start upgrading:
                            var newValue = prevalue.Value.Replace("|", "||")
                                .Replace(";Name:", "|Name:")
                                .Replace("; Alias:", "| Alias:")
                                .Replace("; Type:", "| Type:")
                                .Replace("; Description:", "| Description:");
                            if (newValue.Contains("Required?"))
                            {
                                newValue = newValue.Replace("; Show in title?", "| Show in title?")
                                    .Replace("; Required?", "| Required?")
                                    .Replace("; Validation:", "| Validation:");
                            }
                            else
                            {
                                newValue = newValue.Replace("; Show in title? true",
                                                            "| Show in title? true| Required? false| Validation: ")
                                    .Replace("; Show in title? false",
                                             "| Show in title? false| Required? false| Validation: ");
                            }
                            newValue = newValue.Replace(";", "");
                            var index = newValue.IndexOf("||showLabel:");
                            if (index >= 0)
                            {
                                var newValueFirstPart = newValue.Substring(0, index);

                                //get the last string
                                var settings = newValue.Substring(index + 2);
                                var showLabel = 1;
                                var maxCount = 0;
                                var showType = 1;
                                var showDescription = 0;
                                var showTitle = 0;
                                var showRequired = 1;
                                var showValidation = 0;

                                var index2 = settings.IndexOf("showLabel:");
                                if (index2 > 0)
                                {
                                    int.TryParse(settings.Substring(10, 1), out showLabel);
                                }
                                index2 = settings.IndexOf("maxCount:");
                                if (index > 0)
                                {
                                    int.TryParse(settings.Substring(index2 + 9, 1), out maxCount);
                                }
                                index2 = settings.IndexOf("showType=");
                                if (index2 > 0)
                                {
                                    int.TryParse(settings.Substring(index2 + 9, 1), out showType);
                                }
                                index2 = settings.IndexOf("showDescription=");
                                if (index2 > 0)
                                {
                                    int.TryParse(settings.Substring(index2 + 16, 1), out showDescription);
                                }
                                index2 = settings.IndexOf("showTitle=");
                                if (index2 > 0)
                                {
                                    int.TryParse(settings.Substring(index2 + 10, 1), out showTitle);
                                }
                                index2 = settings.IndexOf("showRequired=");
                                if (index2 > 0)
                                {
                                    int.TryParse(settings.Substring(index2 + 13, 1), out showRequired);
                                }
                                index2 = settings.IndexOf("showValidation=");
                                if (index2 > 0)
                                {
                                    int.TryParse(settings.Substring(index2 + 15, 1), out showValidation);
                                }

                                newValue =
                                    string.Format(
                                        "{0}||showLabel:{1};maxCount:{2};showType={3},showDescription={4},showTitle={5},showRequired={6},showValidation={7}",
                                        newValueFirstPart, showLabel, maxCount, showType, showDescription, showTitle,
                                        showRequired, showValidation);
                            }
                            else
                            {
                                newValue +=
                                    "||showLabel:1;maxCount:0;showType=1,showDescription=0,showTitle=0,showRequired=1,showValidation=0";
                            }
                            var helper = global::umbraco.BusinessLogic.Application.SqlHelper;
                            //write to database
                            helper.ExecuteNonQuery(
                                "UPDATE cmsDataTypePreValues SET value = @value WHERE id = @id",
                                global::umbraco.BusinessLogic.Application.SqlHelper.CreateParameter("@id", prevalue.Key),
                                global::umbraco.BusinessLogic.Application.SqlHelper.CreateParameter("@value", newValue));

                            count++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Notify, -1, "error in upgrade schema: " + ex.Message + ex.StackTrace);
            }

            start.Visible = false;
            end.Visible = true;
            number.Text = count.ToString();
        }

        private void FillPrevalues()
        {
            try
            {
                var helper = global::umbraco.BusinessLogic.Application.SqlHelper;
                var reader = helper.ExecuteReader("SELECT dtpv.id, " +
                                                       "dtpv.value " +
                                                       "FROM cmsDataType dt " +
                                                       "LEFT JOIN cmsDataTypePreValues dtpv ON dt.nodeId = dtpv.datatypeNodeId " +
                                                       "WHERE dt.controlId = '454545AB-1234-4321-ABCD-1234567890AB'");
                prevalues = new Dictionary<int, string>();
                if (reader.HasRecords)
                {
                    while (reader.Read())
                    {
                        prevalues.Add(reader.GetInt("id"), reader.GetString("value"));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Notify, -1, "error in upgrade schema: " + ex.Message + ex.StackTrace);
            }
        }

    }
}