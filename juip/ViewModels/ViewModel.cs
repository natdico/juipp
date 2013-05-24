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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Script.Serialization;
using Org.Juipp.Core.Views;

namespace Org.Juipp.Core.ViewModels
{
    [Serializable]
    public class ViewModel : IViewModel
    {
        private static object GetControl(ViewBase view, FieldInfo field)
        {
            return view.GetType().InvokeMember(
                field.Name,
                BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance,
                Type.DefaultBinder,
                view,
                new object[] { });
        }
        private static PropertyInfo GetProperty(FieldInfo field, string name)
        {
            return field.FieldType.GetProperties().FirstOrDefault(m => m.Name == name);
        }
        private void SetProperty(FieldInfo field, object control, Type modelType, PropertyInfo property, string propertyName)
        {
            var propertyInfo = GetProperty(field, propertyName);
            if (propertyInfo != null)
            {
                var value = propertyInfo.GetValue(control, null);
                modelType.InvokeMember(
                    property.Name,
                    BindingFlags.SetProperty,
                    Type.DefaultBinder,
                    this, new[] { value });
            }
        }

        public ViewModel Bind(ViewBase view)
        {
            var modelType = this.GetType();
            var viewType = view.GetType();

            foreach (var property in modelType.GetProperties())
            {
                var controlID = string.Format("_{0}_{1}", modelType.Name, property.Name);

                var field = viewType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                                    .FirstOrDefault(m => m.Name.EndsWith(controlID));

                if (field == null) continue;

                var control = GetControl(view, field);

                SetProperty(field, control, modelType, property, "SelectedValue");
                SetProperty(field, control, modelType, property, "Text");
                SetProperty(field, control, modelType, property, "Value");

            }
            return this;
        }
        public string Serialize()
        {
            return Serialize(this);
        }
        public static string Serialize<T>(T obj)
        {
            var serializer = new DataContractJsonSerializer(obj.GetType());
            var ms = new MemoryStream();
            serializer.WriteObject(ms, obj);
            var retVal = Encoding.UTF8.GetString(ms.ToArray());
            return retVal;
        }
        public static T Deserialize<T>(string json)
        {
            var obj = Activator.CreateInstance<T>();
            var ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
            var serializer = new DataContractJsonSerializer(obj.GetType());
            obj = (T)serializer.ReadObject(ms);
            ms.Close();
            return obj;
        }
        public ViewModel Clone()
        {
            using (var stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, this);
                stream.Seek(0, SeekOrigin.Begin);
                return (ViewModel)formatter.Deserialize(stream);
            }
        }
    }
}
