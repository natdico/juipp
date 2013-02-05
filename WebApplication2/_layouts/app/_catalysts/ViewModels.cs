using System;
using Org.Juipp.Core.ViewModels;

namespace WebApplication2._layouts.app.ViewModels 
{
    public static class ViewModelReference
    {
             public const string DefaultViewModel = "DefaultViewModel";
                 }

     
    
    [Serializable]
    public partial class DefaultViewModel : IViewModel {}
 } 

