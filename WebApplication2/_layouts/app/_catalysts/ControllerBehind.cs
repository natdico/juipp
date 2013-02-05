using System.Collections.Generic;

using Org.Juipp.Core.Controllers;
using Org.Juipp.Core.Events.Arguments;
using Org.Juipp.Core.Events.Handlers;
using Org.Juipp.Core.ViewModels;

using WebApplication2._layouts.app.ViewModels;

namespace WebApplication2._layouts.app.Controllers 
{ 

 
	public partial class Controller : ParentController
    {
        public Controller(ContainerBase containerBase) : base(containerBase) {}
    }
 


    public partial class ParentController : ControllerBase
    {
	    public ParentController(ContainerBase containerBase) : base(containerBase) {}
        protected override void OnTransitionEvent<T>(TransitionEvent<T> transitionEvent)  
        {  
              
            base.FireTransitionEvent(transitionEvent as TransitionEvent<DefaultViewModel>, _defaultViewModelViewSwitched);
             
        }

        public override IList<IViewModel> Models
        {
            get
            {
                var list = new List<IViewModel>();
                                 list.Add(new DefaultViewModel());
                                 return list; 
            }
        }
    }


     public partial class ParentController : ITransitionEventSender<DefaultViewModel>
    {
        private TransitionEventHandler<DefaultViewModel> _defaultViewModelViewSwitched;
        event TransitionEventHandler<DefaultViewModel> ITransitionEventSender<DefaultViewModel>.TransitionEventFired
        {
            add { _defaultViewModelViewSwitched += value; }
            remove { if (_defaultViewModelViewSwitched != null) _defaultViewModelViewSwitched -= value; }
        }
    }
 
} 

