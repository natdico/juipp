using System;
using System.Collections.Generic;
using WebApplication2._layouts.app.Behaviors;
using WebApplication2._layouts.app.Views;

namespace WebApplication2._layouts.app.Controllers
{
    public partial class Container
    {
        protected override void OnBehaviorBinding()
        {
            base.BehaviorBinding = new Dictionary<string, string>()
                                       {
                                           {BehaviorReference.OpenDefaultBehavior, ViewReference.DefaultView}
                                       };
        }
    }
}