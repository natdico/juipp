using adisware.juipp.Events.Arguments;
using adisware.juipp.Events.Handlers;
using adisware.juipp.Views;
using TargetName@juipp.ViewModels;
 
namespace TargetName@juipp.Views 
{
   
    public partial class View : ViewBase {}
    
    public static class ViewReference
    {
             public static string MyView = "TargetName@juipp.Views.MyView";
                 }

     
        public partial class View : IBehaviorEventSender<MyViewModel> 
        {
            private BehaviorEventHandler<MyViewModel> _myViewModelBehaviorEventFired;
            event BehaviorEventHandler<MyViewModel> IBehaviorEventSender<MyViewModel>.BehaviorEventFired
            {
                add
                {
                    _myViewModelBehaviorEventFired += value;
                    base.FireBehaviorEventDelegates.Add(args => base.FireBehaviorEvent(_myViewModelBehaviorEventFired, args as BehaviorEvent<MyViewModel>));
                }
                remove { if (_myViewModelBehaviorEventFired != null) _myViewModelBehaviorEventFired -= value; }
            }
        }
    } 

