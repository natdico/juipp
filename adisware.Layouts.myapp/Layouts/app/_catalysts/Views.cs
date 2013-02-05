using Org.Juipp.Core.Events.Arguments;
using Org.Juipp.Core.Events.Handlers;
using Org.Juipp.Core.Views;
using adisware.Layouts.app.ViewModels;

namespace adisware.Layouts.app.Views 
{
   
    public partial class View : ViewBase 
	{
		public DefaultViewModel DefaultViewModel
        {
            get { return base.RetrieveBindingElement<DefaultViewModel>(); }
            set { base.PersistBindingElement(value); }
        }
			
	}
    
    public static class ViewReference
    {
             public static string DefaultView = "DefaultView";
                 }

     
        public partial class View : IBehaviorEventSender<DefaultViewModel> 
        {
            private BehaviorEventHandler<DefaultViewModel> _defaultViewModelBehaviorEventFired;
            event BehaviorEventHandler<DefaultViewModel> IBehaviorEventSender<DefaultViewModel>.BehaviorEventFired
            {
                add
                {
                    _defaultViewModelBehaviorEventFired += value;
                    base.FireBehaviorEventDelegates.Add(args => base.FireBehaviorEvent(_defaultViewModelBehaviorEventFired, args as BehaviorEvent<DefaultViewModel>));
                }
                remove { if (_defaultViewModelBehaviorEventFired != null) _defaultViewModelBehaviorEventFired -= value; }
            }
        }
    } 

