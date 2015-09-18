using System;
using System.Web.Script.Services;
using System.Web.Services;
using umbraco.cms.businesslogic.contentitem;

namespace TheFarm.Umbraco.EmbeddedContent
{
    [ScriptService]
    [WebService]
    public partial class UmbracoNodeNameService : WebService
    {
        [WebMethod(MessageName = "readumbraconodename")]
        public string ReadUmbracoNodeName(string qid)
        {
            if (string.IsNullOrEmpty(qid))
            {
                return "";
            }

            try
            {
                int id;
                if (int.TryParse(qid, out id))
                {
                    var ci = new ContentItem(id);
                    return ci.Text;
                }
            }
            catch (Exception)
            {
                return "";
            }
            return "";
        }
    }
}