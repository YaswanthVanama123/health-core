using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace GeniView.Data.Hardware.Abstract
{
    public abstract class Data : INotifyDataErrorInfo
    {
        // For convenience, instead of setting each property errors, we set global errors for all properties.
        private const string ERRORS_FOR_ALL_PROPERTIES = "ERRORS_FOR_ALL_PROPERTIES";

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public Data()
        {
            Errors = new Dictionary<string, List<string>>();
        }

        [NotMapped]   // EF Core cannot map Dictionary<string, List<string>> — validation-only field
        [XmlIgnore]   // XmlSerializer: exclude from XML export
        public Dictionary<string, List<string>> Errors { get; set; }

        public bool HasErrors
        {
            get
            {
                // Indicate whether the entire Product object is error-free.
                return (Errors.Count > 0);
            }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                // Provide all the error collections.
                return (Errors.Values);
            }
            else
            {
                // Provice the error collection for the requested property
                // (if it has errors).
                if (Errors.ContainsKey(propertyName))
                {
                    return (Errors[propertyName]);
                }

                if (Errors.ContainsKey(ERRORS_FOR_ALL_PROPERTIES))
                {
                    return Errors[ERRORS_FOR_ALL_PROPERTIES];
                }

                return null;
            }
        }

        public void SetErrors(string propertyName, List<string> propertyErrors)
        {
            // Clear any errors that already exist for this property.
            Errors.Remove(propertyName);
            // Add the list collection for the specified property.
            Errors.Add(propertyName, propertyErrors);
            // Raise the error-notification event.
            if (ErrorsChanged != null)
                ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public void ClearErrors(string propertyName)
        {
            // Remove the error list for this property.
            Errors.Remove(propertyName);
            // Raise the error-notification event.
            if (ErrorsChanged != null)
                ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public void SetErrorsForAllProperties(List<string> propertyErrors)
        {
            // Clear any errors that already exist for this property.
            Errors.Remove(ERRORS_FOR_ALL_PROPERTIES);
            // Add the list collection for the specified property.
            Errors.Add(ERRORS_FOR_ALL_PROPERTIES, propertyErrors);
        }

        public void ClearErrorsForAllProperties()
        {
            // Clear the entire error list.
            Errors.Clear();
        }

        public override string ToString()
        {
            PropertyInfo[] pi = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            StringBuilder sb = new StringBuilder();
            sb.Append(this.GetType().Name);

            foreach (var property in pi)
            {
                sb.Append(" ");
                sb.Append(property.Name);
                sb.Append(": ");
                sb.Append(property.GetValue(this));
                sb.Append(",");
            }

            return sb.ToString().TrimEnd(',');
        }
    }
}
