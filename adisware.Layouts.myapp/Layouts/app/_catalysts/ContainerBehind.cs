using System;
using System.Collections.Generic;
using Org.Juipp.Core.Behaviors;
using adisware.Layouts.app.Views;
using adisware.Layouts.app.Behaviors;
using adisware.Layouts.app.Controllers;
using Org.Juipp.Core.Views;
using Org.Juipp.Core.Controllers;

namespace adisware.Layouts.app.Controllers 
{ 
    public static class ControllerReference
    {
             public const string Controller = "Controller";
                 }

    public partial class Container : ContainerBase
    {
	           				protected ControllerBase Controller;
				
            
             protected global::adisware.Layouts.app.Views.DefaultView DefaultView;
             
        protected override void OnInit(EventArgs e) 
        { 
		      
			             base.Controllers.Add(new KeyValuePair<string, ControllerBase>(
                                      ControllerReference.Controller,
                                      new Controller(this) { ID = "Controller" }));
             
		    
             base.OnInit(e);

             base.Behaviors = new Dictionary<String, IBehavior>();

              
                 base.Behaviors.Add(new KeyValuePair<string, IBehavior>(
                                        BehaviorReference.OpenDefaultBehavior, 
                                        new OpenDefaultBehavior() { BehaviorContext = this } ));
              
                 base.Behaviors.Add(new KeyValuePair<string, IBehavior>(
                                        BehaviorReference.ProfileBehavior, 
                                        new ProfileBehavior() { BehaviorContext = this } ));
             

            base.Views = new Dictionary<string, ViewBase>();

              
                 base.Views.Add(new KeyValuePair<string, ViewBase>(
                                        ViewReference.DefaultView,  
                                        this.DefaultView ));
                     }
    }
} 

