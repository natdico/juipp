using System;
using System.Security.Principal;
using Org.Juipp.Core.Controllers;
using Org.Juipp.Core.Events.Arguments;
using Org.Juipp.Core.Views;
using WebApplication2._layouts.app.Behaviors;
using WebApplication2._layouts.app.ViewModels;

namespace WebApplication2._layouts.app.Controllers
{
    public partial class Controller
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (this.Page.IsPostBack) return;

            this.FireBehaviorEvent<DefaultViewModel>(
                new BehaviorEvent<DefaultViewModel>()
                {
                    BehaviorReference = BehaviorReference.OpenDefaultBehavior
                });
        }
    }
}