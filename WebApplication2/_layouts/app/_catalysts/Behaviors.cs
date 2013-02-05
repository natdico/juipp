using Org.Juipp.Core.Events.Arguments;
using Org.Juipp.Core.Events.Handlers;
using Org.Juipp.Core.Behaviors;
using Org.Juipp.Core.Controllers;
using WebApplication2._layouts.app.ViewModels;

namespace WebApplication2._layouts.app.Behaviors 
{
    public static partial class BehaviorReference 
    {
             public const string OpenDefaultBehavior = "OpenDefaultBehavior";
             
    }
	     public partial class OpenDefaultBehavior : BehaviorBase{} 
    

  
    public partial class BehaviorBase 
	{
		public IBehaviorContext BehaviorContext { get; set; }
    }


	   public partial class BehaviorBase : IExecutableBehavior<DefaultViewModel>
   {
		public virtual void Execute(BehaviorEvent<DefaultViewModel> args) {}
   }
} 

