using System;
using System.Collections.Generic;
using Org.Juipp.Core.Behaviors;
using WebApplication2._layouts.app.Views;
using WebApplication2._layouts.app.Behaviors;
using WebApplication2._layouts.app.Controllers;
using Org.Juipp.Core.Views;
using Org.Juipp.Core.Controllers;

namespace WebApplication2._layouts.app.Controllers 
{ 
    public static class ControllerReference
    {
             public const string Controller = "Controller";
                 }

    public partial class Container : ContainerBase
    {
	           				protected ControllerBase Controller;
				
            
             protected global::WebApplication2._layouts.app.Views.DefaultView DefaultView;
             
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
             

            base.Views = new Dictionary<string, ViewBase>();

              
                 base.Views.Add(new KeyValuePair<string, ViewBase>(
                                        ViewReference.DefaultView,  
                                        this.DefaultView ));
                     }
    }
} 

