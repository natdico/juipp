using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace adisware.VisualWebPart
{
    [ToolboxItemAttribute(false)]
    public class VisualWebPart : WebPart
    {
        protected override void CreateChildControls()
        {
            this.Controls.Add(Page.LoadControl("~/_layouts/app/_catalysts/Container.ascx"));
            base.CreateChildControls();
        }
    }
}
