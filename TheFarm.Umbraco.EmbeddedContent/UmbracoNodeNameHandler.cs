using System;
using System.Web;
using umbraco.cms.businesslogic.contentitem;

namespace TheFarm.Umbraco.EmbeddedContent
{
    /// <summary>
    /// This is a simple Umbraco node name lookup, wrapped as a handler. It will be called from JS to retrieve a node name when
    /// initializing the content and media picker.
    /// </summary>
    public class UmbracoNodeNameHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            ReadUmbracoNodeName(context);
        }

        /// <summary>
        /// Takes the id from the querystring and returns the nodename of the node with this id.
        /// </summary>
        /// <param name="context">the current HttpContext</param>
        private static void ReadUmbracoNodeName(HttpContext context)
        {
            context.Response.ContentType = "text/xml";

            var qid = context.Request.QueryString["id"];

            if (string.IsNullOrEmpty(qid))
            {
                context.Response.Write("<data>no node found</data>");
            }

            try
            {
                int id;
                if (int.TryParse(qid, out id))
                {
                    var ci = new ContentItem(id);
                    context.Response.Write("<data>" + ci.Text + "</data>");
                }
            }
            catch (Exception)
            {
                context.Response.Write("<data>no node found</data>");
            }
        }
    }
}