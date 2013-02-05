﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplication2._layouts.app.Pages
{
    public partial class Default : System.Web.UI.Page
    {
        protected override void CreateChildControls()
        {
            this.PanelContainer.Controls.Add(Page.LoadControl("../_catalysts/Container.ascx"));
            base.CreateChildControls();
        }
    }
}