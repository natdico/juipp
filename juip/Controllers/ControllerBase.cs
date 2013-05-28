/*  Copyright (c) 2012 Natnael Gebremariam
    http://www.juipp.org
 
    Permission is hereby granted, free of charge, to any person obtaining
    a copy of this software and associated documentation files (the
    "Software"), to deal in the Software without restriction, including
    without limitation the rights to use, copy, modify, merge, publish,
    distribute, sublicense, and/or sell copies of the Software, and to
    permit persons to whom the Software is furnished to do so, subject to
    the following conditions:

    The above copyright notice and this permission notice shall be
    included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Org.Juipp.Core.Behaviors;
using Org.Juipp.Core.Commons;
using Org.Juipp.Core.Events.Arguments;
using Org.Juipp.Core.Events.Handlers;
using Org.Juipp.Core.ViewModels;
using Org.Juipp.Core.Views;

namespace Org.Juipp.Core.Controllers
{
    public abstract class ControllerBase :
        WebControl,
        ILoadBehaviorViewBinding,
        IDetermineModels,
        IBehaviorContext
    {
        protected IDictionary<string, ViewBase> Views;
        protected IDictionary<string, string> BehaviorBinding;
        protected IDictionary<string, string> ViewControllerBinding;
        protected IDictionary<string, IBehavior> Behaviors;
        protected ContainerBase ContainerBase;
        public ScriptManager ScriptManager { get; set; }

        protected Stack<String> ViewHistory
        {
            get
            {
                var history = this.RetrieveBindingElement("ViewHistory");
                return history == null ? new Stack<string>() : history as Stack<string>;
            }
            set { this.PersistBindingElement("ViewHistory", value);}
        }

        protected ControllerBase(ContainerBase containerBase)
        {
            ContainerBase = containerBase;
        }

        #region IBehaviorContext Members

        object IBehaviorContext.this[string key]
        {
            get { return (ContainerBase as IBehaviorContext)[key]; }
            set { (ContainerBase as IBehaviorContext)[key] = value; }
        }

        #endregion

        private string CurrentViewReference
        {
            set
            {
                var state = this.ViewState["CurrentViewReference"];
                if (state != null) this.ViewState.Remove("CurrentViewReference");
                this.ViewState.Add("CurrentViewReference", value);
            }
            get
            {
                var state = this.ViewState["CurrentViewReference"];
                if (state == null) return null;
                return (string)state;
            }
        }
        private void TransitionView<T>(ICanChangeMyVisibility sender, string viewName, BehaviorEvent<T> behaviorEvent)  where T : IViewModel, new()
        {
            this.TransitionView(sender, viewName);

            this.FireTransitionEvent(behaviorEvent, this.OnTransitionEvent, viewName);


            //if (sender is IBehaviorEventSender<T>) this.CurrentViewReference = viewName;
        }
        private ViewBase GetNextView(string viewName)
        {
            var nextView = Views[viewName];
            nextView.Reference = viewName;
            return nextView;
        }
        private void TransitionView(ICanChangeMyVisibility sender, string viewName)
        {
            if (viewName == null) return;
            var nextView = this.GetNextView(viewName);
            if (sender != null && !sender.Equals(nextView))
            {
                sender.Hide();
            }
            nextView.Show();

            if (string.IsNullOrEmpty(viewName) == false)
            {
                var viewHistory = this.ViewHistory;
                viewHistory.Push(viewName);
                this.ViewHistory = viewHistory;
            }
        }
        private void BindingViewModel(ViewBase view, IViewModel viewModel) 
        {
            try
            {
                if(viewModel != null) BindViewModel(view, viewModel);
            }
            catch (Exception ex)
            {
                this.OnErrorBindingViewModel(ex);
            }
        }
        private static void BindViewModel(ViewBase view, IViewModel viewModel)
        {
            if (viewModel == null) return;

            var viewType = view.GetType();
            var modelType = viewModel.GetType();

            try
            {
                BindViewModelByConvention(view, viewModel);
            }
            catch(Exception ex)
            {
                var e = ex.Message;
            }

            var bind = viewType.GetMethods().FirstOrDefault(
                m =>
                m.Name == "Bind"
                && m.GetParameters().FirstOrDefault() != null
                && m.GetParameters()[0].ParameterType == modelType);

            if (bind != null) bind.Invoke(view, new object[] {viewModel});

            var model = viewType.GetProperties().FirstOrDefault(
                m =>
                m.Name == modelType.Name
                && m.GetAccessors().FirstOrDefault() != null
                && m.GetSetMethod().GetParameters()[0].ParameterType == modelType);

            if (model != null) model.GetSetMethod().Invoke(view, new object[] {viewModel});


            foreach (var control in view.Controls.OfType<ViewBase>())
            {
                BindViewModel(control, viewModel);
            }
        }
        private static void BindViewModelByConvention(ViewBase view, IViewModel viewModel)
        {
            var viewType = view.GetType();
            var modelType = viewModel.GetType();

            if(modelType.BaseType != null && modelType.BaseType.IsGenericType)
            {
                BindViewModelCollection(view, viewModel);
                return;
            }

            foreach (var property in modelType.GetProperties())
            {
                if (property.PropertyType == typeof(bool))
                {
                    if(BindViewModelVisibility(view, viewModel, property)) continue;
                }
                var controlID = string.Format("_{0}_{1}", modelType.Name, property.Name);

                foreach (var field in GetFields(viewType, controlID))
                {
                    var control = GetControl(view, viewType, field);

                    var text = GetProperty(field, "Text");
                    if (text != null)
                    {
                        var v = property.GetValue(viewModel, null) ?? string.Empty;

                        text.GetSetMethod().Invoke(control, new[] { v.ToString() });
                        continue;
                    }

                    var value = GetProperty(field, "Value");
                    if (value != null)
                    {
                        var v = property.GetValue(viewModel, null) ?? string.Empty;
                        value.GetSetMethod().Invoke(control, new[] { v.ToString() });
                        continue;
                    }
                }
            }
        }
        private static bool BindViewModelVisibility(ViewBase view, IViewModel viewModel, PropertyInfo property)
        {
            var viewType = view.GetType();
            var modelType = viewModel.GetType();

            var controlID = string.Format("_{0}_Visible_{1}", modelType.Name, property.Name);

            foreach (var field in GetFields(viewType, controlID))
            {
                var control = GetControl(view, viewType, field);

                var visible = GetProperty(field, "Visible");

                if (visible == null) continue;

                var v = property.GetValue(viewModel, null);
                visible.GetSetMethod().Invoke(control, new[] {v});

                return true;
            }
            return false;
        }
        private static void BindViewModelCollection(ViewBase view, IViewModel viewModel)
        {
            var viewType = view.GetType();
            var modelType = viewModel.GetType();

            var controlID = string.Format("_{0}", modelType.Name);

            foreach (var field in GetFields(viewType, controlID))
            {
                var control = GetControl(view, viewType, field);

                var dataSouce = GetProperty(field, "DataSource");
                if (dataSouce == null) continue;

                var v = viewModel;

                dataSouce.GetSetMethod().Invoke(control, new object[] {v});

                var selectedValue = GetProperty(field, "SelectedValue");
                selectedValue.GetSetMethod().Invoke(control, new object[] { string.Empty });

                var dataBind = field.FieldType.GetMethods().FirstOrDefault(
                    m =>
                    m.Name == "DataBind"
                    && m.GetParameters().Length == 0);

                if (dataBind != null) dataBind.Invoke(control, new object[] {});
            }
        }

        private static PropertyInfo GetProperty(FieldInfo field, string name)
        {
            return field.FieldType.GetProperties().FirstOrDefault(m => m.Name == name);
        }

        private static object GetControl(ViewBase view, Type viewType, FieldInfo field)
        {
            return viewType.InvokeMember(
                field.Name,
                BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance,
                Type.DefaultBinder,
                view,
                new object[] {});
        }

        private static IEnumerable<FieldInfo> GetFields(Type viewType, string controlID)
        {
            return viewType
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.Name.EndsWith(controlID));
        }

        private void FireTransitionEvent<T>(BehaviorEvent<T> args, TransitionEventDelegate<T> transitionEventDelegate, string viewName)  where T : IViewModel, new()
        {
            var transitionEvent = new TransitionEvent<T>(args)
            {
                ViewModel = args.ViewModel,
                ViewReference = viewName,
                PreviousViewReference = this.CurrentViewReference
            };
            transitionEventDelegate(transitionEvent);

            if (viewName != null) this.CurrentViewReference = viewName;


            this.ScriptManager = ScriptManager.GetCurrent(this.Page);
            if (this.ScriptManager != null
                && this.ScriptManager.IsInAsyncPostBack
                && this.ScriptManager.EnableHistory
                && string.IsNullOrEmpty(viewName) == false)
            {
                this.ScriptManager.AddHistoryPoint(new NameValueCollection
                                                       {
                                                           {"pv", transitionEvent.PreviousViewReference}
                                                       }, transitionEvent.PreviousViewReference);
            }

        }
        private bool OnBehaviorEventFired<T>(IBehaviorEventSender<T> sender, BehaviorEvent<T> behaviorEvent) where T : IViewModel, new()
        {
            this.OnBeforeBehaviorLookup(behaviorEvent);
            if (this.Behaviors.ContainsKey(behaviorEvent.BehaviorReference) == false) return false;

            var behavior = this.Behaviors[behaviorEvent.BehaviorReference] as IExecutableBehavior<T>;
            if (behavior == null) return false;

            behavior.BehaviorContext = this;

            this.OnBeforeBehaviorEvent(sender, behaviorEvent);
            try
            {
                this.OnBehaviorEvent(behavior, behaviorEvent);
            }
            catch (Exception ex)
            {
                this.OnErrorBehaviorEvent(ex);
                return false;
            }
            this.OnAfterBehaviorEvent(sender, behaviorEvent);

            string viewName = null;

            if (this.BehaviorBinding.ContainsKey(behaviorEvent.BehaviorReference))
            {
                viewName = this.BehaviorBinding[behaviorEvent.BehaviorReference];
            }

            this.OnBeforeTransitionEvent(viewName, behavior);
            var view = sender as ViewBase;
            if (view != null) view.BehaviorContext = this;

            this.TransitionView(view, viewName, behaviorEvent);

            if (viewName == null && sender is ViewBase)
            {
                this.BindingViewModel(sender as ViewBase, behaviorEvent.ViewModel);
            }
            else if (viewName != null)
            {
                var next = this.GetNextView(viewName);
                if (next != null)
                {
                    this.BindingViewModel(next, behaviorEvent.ViewModel);

                    next.OnAfterTransition(behaviorEvent);
                    foreach (var sub in next.Controls.OfType<ViewBase>())
                    {
                        sub.BehaviorContext = next.BehaviorContext;
                        sub.OnAfterTransition(behaviorEvent);
                    }
                }
            }

            this.OnAfterTransitionEvent(viewName, behavior);

            return true;
        }

        protected T RetrieveBindingElement<T>()
        {
            var name = typeof(T).FullName;
            //if (name != null)
            //{
                var bindingItem = this.ViewState[name];
                if (bindingItem == null) return default(T);
                return (T)bindingItem;
            //}
            //return default(T);
        }
        protected void PersistBindingElement<T>(T element)
        {
            var name = typeof(T).FullName;
            //if (name != null)
            //{
                var bindingItem = this.ViewState[name];
                if (bindingItem != null) this.ViewState.Remove(name);
                this.ViewState.Add(name, element);
            //}
        }
        protected object RetrieveBindingElement(string key)
        {
            var bindingItem = this.ViewState[key];
            return bindingItem;
        }
        protected void PersistBindingElement(string key, object element)
        {
            var bindingItem = this.ViewState[key];
            if (bindingItem != null) this.ViewState.Remove(key);
            this.ViewState.Add(key, element);
        }
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.ScriptManager = ScriptManager.GetCurrent(this.Page);
            if (this.ScriptManager != null)
            {
                this.ScriptManager.Navigate += this.ScriptManagerNavigate;

            }
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (string.IsNullOrEmpty(this.Page.ClientQueryString)) return;

            try
            {
                this.ProcessJapi();
            }
            catch (Exception ex)
            {
                this.EndResponseWithStatusCode(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        private void ProcessJapi()
        {

            var attributes = Attribute.GetCustomAttributes(this.GetType());

            var attr = attributes.FirstOrDefault(a => a.GetType() == typeof(JapiAttribute) );

            if (attr == null) return;

            var japi = this.Page.Request.QueryString["_japi"];

            if (string.IsNullOrEmpty(japi)) return;

            var frags = japi.Split(new[] {'/'}, StringSplitOptions.None);

            if (frags.Length < 2) return;

            if (frags[0] == "behaviors" && frags[2] == "execute")
            {
                this.ProcessJapiForBehaviors(frags[1], frags[3]);
                return;
            }

            this.EndResponseWithStatusCode("Invalid or undefined JAPI URL", HttpStatusCode.BadRequest);
        }

        private void EndResponseWithStatusCode(string message, HttpStatusCode code)
        {
            this.Page.Response.Clear();
            this.Page.Response.Write(message);
            this.Page.Response.StatusCode = (int)code;
            this.Page.Response.Flush();
            this.Page.Response.Close();
        }
        private void ProcessJapiForBehaviors(string partialBehaviorName, string partialViewModel)
        {
            var fireBehaviorEventMethodInfo = this.GetType().GetMethod("FireBehaviorEvent",
                                                                    BindingFlags.Instance | BindingFlags.NonPublic |
                                                                    BindingFlags.Public | BindingFlags.FlattenHierarchy);


            var viewModelReference = GetViewModelReference(partialViewModel);

            var partialNamespace = GetNamespace(fireBehaviorEventMethodInfo);

            var viewModel = GetViewModel(partialNamespace, viewModelReference);

            var requestContent = GetRequestContent(this.Page.Request);

            var deserializeMethodInfo = typeof(ViewModel).GetMethod("Deserialize");

            var genericMethod = deserializeMethodInfo.MakeGenericMethod(new[] { viewModel.GetType() });
             

            viewModel = genericMethod.Invoke(viewModel, new object[] { requestContent });

            var behaviorReference = GetBehaviorReference(partialBehaviorName);

            if (this.IsExposedForJapi(behaviorReference, partialNamespace) == false)
                throw new ApplicationException(string.Format("{0} is not exposed to JAPI.", behaviorReference));

            this.InvokeFireBehaviorEventMethod(viewModel, behaviorReference, fireBehaviorEventMethodInfo);
        }
        private bool IsExposedForJapi(string behaviorReference, string nspace)
        {
            var behaviorTypeName = string.Format("{0}.Behaviors.{1}", nspace, behaviorReference);

            var assemblyName = this.GetType().Assembly.FullName;

            var behaviorType =  Activator.CreateInstance(assemblyName, behaviorTypeName).Unwrap().GetType();

            var attributes = Attribute.GetCustomAttributes(behaviorType);

            var attr = attributes.FirstOrDefault(a => a.GetType() == typeof(JapiAttribute) );

            return attr != null;
        }
        private static string GetRequestContent(System.Web.HttpRequest request)
        {
            string documentContents;
            using (var receiveStream = request.InputStream)
            {
                using (var readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    documentContents = readStream.ReadToEnd();
                }
            }
            return documentContents;
        }
        private static string GetNamespace(MethodInfo fireBehaviorEventMethodInfo)
        {
            var nspace = fireBehaviorEventMethodInfo.ReflectedType.Namespace;
            if (nspace != null) nspace = nspace.Substring(0, nspace.LastIndexOf('.'));
            return nspace;
        }

        protected abstract Type GetViewModelReferenceType();
        protected abstract Type GetBehaviorReferenceType();

        private string GetViewModelReference(string partialViewModelName)
        {
            var viewModelName = string.Format("{0}ViewModel", partialViewModelName);
            var viewModelReference = string.Empty;
        
            foreach (var member in GetViewModelReferenceType()
                .GetMembers()
                .Where(member => member.Name.ToLower() == viewModelName.ToLower()))
            {
                viewModelReference = member.Name;
                break;
            }
            return viewModelReference;
        }
        private  object GetViewModel(string nspace, string viewModelReference)
        {
            var viewModelTypeName = string.Format("{0}.ViewModels.{1}", nspace, viewModelReference);

            var appDomain = AppDomain.CurrentDomain;
            var assemblyName = this.GetType().Assembly.FullName;

            var viewModel = Activator.CreateInstance(appDomain, assemblyName, viewModelTypeName).Unwrap();
            return viewModel;
        }
        private string GetBehaviorReference(string partialBehaviorReferenceName)
        {
            var behaviorName = string.Format("{0}Behavior", partialBehaviorReferenceName);
            var behaviorReference = string.Empty;
            foreach (var member in this.GetBehaviorReferenceType()
                .GetMembers()
                .Where(member => member.Name.ToLower() == behaviorName.ToLower()))
            {
                behaviorReference = member.Name;
                break;
            }
            return behaviorReference;
        }

        private void InvokeFireBehaviorEventMethod(object viewModel, string behaviorReference,
                                                   MethodInfo fireBehaviorEventMethodInfo)
        {
            var genericListType = typeof (BehaviorEvent<>).MakeGenericType(new[] {viewModel.GetType()});
            var behaviorEvent = Activator.CreateInstance(genericListType);

            var setViewModel = behaviorEvent.GetType().GetProperty("ViewModel").GetSetMethod();
            setViewModel.Invoke(behaviorEvent, new[] {viewModel});

            var setBehaviorReference = behaviorEvent.GetType().GetProperty("BehaviorReference").GetSetMethod();
            setBehaviorReference.Invoke(behaviorEvent, new object[] {behaviorReference});

            var method = fireBehaviorEventMethodInfo.MakeGenericMethod(new[] {viewModel.GetType()});

            method.Invoke(this, new[] {behaviorEvent});

            var getViewModel = behaviorEvent.GetType().GetProperty("ViewModel").GetGetMethod();

            var postViewModel = getViewModel.Invoke(behaviorEvent, null);

            var deserializeMethodInfo = typeof (ViewModel).GetMethod("Serialize", new Type[] {});


            var json = deserializeMethodInfo.Invoke(postViewModel, null) as String;

            this.Page.Response.Clear();
            if (json != null)
            {
                this.Page.Response.Write(json);
            }
            this.Page.Response.ContentType = "application/json";
            this.Page.Response.StatusCode = (int) HttpStatusCode.OK;
            this.Page.Response.Flush();
            this.Page.Response.Close();
        }

        protected virtual void ScriptManagerNavigate(object sender, HistoryEventArgs e)
        {
            
            if (e.State.AllKeys.Contains("pv") == false) return;

            this.TransitionView(this.Views[this.CurrentViewReference], e.State["pv"]);
            this.CurrentViewReference = e.State["pv"];
        }
        protected void OnInitialBehaviorEventFired<T>(string behaviorName) where T : IViewModel, new()
        {
            this.OnBehaviorEventFired(null,
                new BehaviorEvent<T>
                    {
                        BehaviorReference = behaviorName
                    });
        }
        protected void OnLoadBehaviorViewBinding()
        {
            foreach (var model in Models)
            {
                var type = this.GetType();
                var wireOnBehaviorEventFiredMethod = type.GetMethod("WireOnBehaviorEventFired", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

                var method = wireOnBehaviorEventFiredMethod.MakeGenericMethod(new[] { model.GetType() });

                method.Invoke(this, new object[] { });
            }

        }
        protected void WireViewEvents()
        {
            foreach (var view in Views.Where(view => this.ViewControllerBinding[view.Key] == this.ID))
            {
                view.Value.BackTriggered += this.ValueBackTriggered;
            }
        }
        protected void FireTransitionEvent<T>(TransitionEvent<T> transitionEvent, TransitionEventHandler<T> transitionEventHandler) where T : IViewModel, new()
        {
            if (transitionEvent == null || transitionEventHandler == null) return;

            transitionEventHandler((ITransitionEventSender<T>)this, transitionEvent);
        }
        protected void WireOnBehaviorEventFired<T>() where T : IViewModel, new()
        {
            foreach (var view in Views.Where(view => this.ViewControllerBinding[view.Key] == this.ID))
            {
                view.Value.BehaviorContext = this;

                foreach (var sub in view.Value.Controls.OfType<ViewBase>())
                {
                    sub.BehaviorContext = this;
                }

                if (view.Value is IBehaviorEventSender<T>)
                {
                    var actionPerformer = ((IBehaviorEventSender<T>)view.Value);

                    actionPerformer.BehaviorEventFired -= this.OnBehaviorEventFired;
                    actionPerformer.BehaviorEventFired += this.OnBehaviorEventFired;

                    if (view.Value is IBehaviorEventSenderCollection<T>)
                    {
                        var parent = (IBehaviorEventSenderCollection<T>)view.Value;
                        foreach (var c in parent.BehaviorTriggers)
                        {
                            c.BehaviorEventFired -= this.OnBehaviorEventFired;
                            c.BehaviorEventFired += this.OnBehaviorEventFired;
                        }
                    }
                }

                var listenerView = view.Value as ICanCatchTransition;
                ((ITransitionEventSender<T>)this).TransitionEventFired += listenerView.OnCatchTransition;
            }
        }
        protected void ValueBackTriggered(object sender, EventArgs e)
        {
            var viewHistory = this.ViewHistory;
            viewHistory.Pop(); //pop the current
            this.ViewHistory = viewHistory;
            this.TransitionView(sender as ICanChangeMyVisibility, ViewHistory.Pop()); //pop the previous one
        }

        protected virtual void InitBehaviorContext() { }
        protected virtual void OnErrorBindingViewModel(Exception ex) {}
        protected virtual void OnErrorBehaviorEvent(Exception ex) {}
        protected virtual void OnBeforeBehaviorEvent<T>(IBehaviorEventSender<T> sender, BehaviorEvent<T> behaviorEvent) where T : IViewModel, new()
        {
        }
        protected virtual void OnBeforeBehaviorLookup<T>(BehaviorEvent<T> behaviorEvent) where T : IViewModel, new()
        {
        }
        protected virtual void OnAfterBehaviorEvent<T>(IBehaviorEventSender<T> sender, BehaviorEvent<T> behaviorEvent) where T : IViewModel, new()
        {
        }
        protected virtual void OnBehaviorEvent<T>(IExecutableBehavior<T> executableBehavior, BehaviorEvent<T> behaviorEvent) where T : IViewModel, new()
        {
            executableBehavior.Execute(behaviorEvent);
        }
        protected virtual void OnBeforeTransitionEvent<T>(string viewReference, IExecutableBehavior<T> behavior) where T : IViewModel, new()
        {
        }
        protected virtual void OnAfterTransitionEvent<T>(string viewReference, IExecutableBehavior<T> behavior) where T : IViewModel, new()
        {
        }

        protected abstract void OnTransitionEvent<T>(TransitionEvent<T> transitionEvent) where T : IViewModel, new();
        public abstract IList<IViewModel> Models { get; }

        public bool FireBehaviorEvent<T>(BehaviorEvent<T> behaviorEvent) where T : IViewModel, new()
        {
            return this.OnBehaviorEventFired(null, behaviorEvent);
        }
        public void LoadBehaviorViewBinding(
            IDictionary<string, ViewBase> views,
            IDictionary<string, IBehavior> behaviors,
            IDictionary<string, string> behaviorBinding,
            IDictionary<string, string> viewControllerBinding)
        {
            this.Views = views;
            this.Behaviors = behaviors;
            this.BehaviorBinding = behaviorBinding;
            this.ViewControllerBinding = viewControllerBinding;
            this.WireViewEvents();
            this.OnLoadBehaviorViewBinding();
            this.InitBehaviorContext();
        }
    }
}
