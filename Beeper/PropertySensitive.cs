using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Beeper
{
    public abstract class PropertySensitive : INotifyPropertyChanged
    {
        [Browsable(false)]
        public bool NotifyOnChange { get; set; } = true;
        /// <summary>
        /// <see cref="INotifyPropertyChanged.PropertyChanged"/>
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();

        public void InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        public void SetPar<T>(T value, [CallerMemberName] string propertyName = null)
        {
            fields[propertyName] = value;
            if (NotifyOnChange)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public T GetPar<T>(T defVal = default(T), [CallerMemberName] string propertyName = null)
        {
            if (!fields.ContainsKey(propertyName))
                fields[propertyName] = defVal;
            return (T)fields[propertyName];
        }

        public IEnumerable<KeyValuePair<string, object>> GetFields()
        {
            foreach (var kvp in fields)
                yield return kvp;
        }
    }
}