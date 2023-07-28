using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;

namespace Supervisor
{
    /// <summary>
    /// Logica di interazione per Register.xaml
    /// </summary>
    public partial class Register : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string Description { get; set; }
        public string UM { get; set; }
        public List<string> States { get; set; }
        public bool ReadWrite { get; set; }
        public int Address { get; set; }
        public RegisterType Type { get; set; }
        public double Gain { get; set; }
        public bool Freeze { get; set; }
        public bool Modified
        {
            get { return _modified; }
            set { _modified = value; OnPropertyChanged(); }
        }
        public bool IsLabel
        {
            get
            {
                if (Type == RegisterType.HoldingRegister | Type == RegisterType.InputRegister)
                {
                    if (States.Count == 0)
                        if (!ReadWrite)
                            return true;
                }
                return false;
            }
        }
        public bool IsStateLabel
        {
            get
            {
                if (States.Count > 0)
                {
                    if (!ReadWrite)
                        return true;
                }
                return false;
            }
        }
        public bool IsTextBox
        {
            get
            {
                if (Type == RegisterType.HoldingRegister | Type == RegisterType.InputRegister)
                {
                    if (States.Count == 0)
                        if (ReadWrite)
                            return true;
                }
                return false;
            }
        }
        public bool IsComboBox
        {
            get
            {
                if (States.Count > 0)
                {
                    if (ReadWrite)
                        return true;
                }
                return false;
            }
        }
        public bool IsCheckBox
        {
            get
            {
                if (Type == RegisterType.Coil | Type == RegisterType.DiscreteInput)
                {
                    if (States.Count == 0)
                    {
                        if (ReadWrite)
                            return true;
                    }
                }
                return false;
            }
        }
        public bool IsLamp
        {
            get
            {
                if (Type == RegisterType.Coil | Type == RegisterType.DiscreteInput)
                {
                    if (States.Count == 0)
                    {
                        if (!ReadWrite)
                            return true;
                    }
                }
                return false;
            }
        }
        public short Value
        {
            get { return _value; }
            set { _value = value; Modified = true; OnPropertyChanged(); OnPropertyChanged("GainedValue"); }
        }
        public double GainedValue
        {
            get { return Value * Gain; }
            set { Value = Convert.ToInt16(value / Gain); Modified = true; OnPropertyChanged(); }
        }

        private short _value;
        private bool _modified;

        public Register()
        {
            InitializeComponent();
            States = new List<string>();
            Gain = 1.0;
            DataContext = this;
        }

        public void SetValue(short newValue)
        {
            Value = newValue;
            Modified = false;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tb = (TextBox)sender;

            if (e.Key == Key.Enter)
                tb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
        }

        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.SelectAll();
            Freeze = true;
        }

        private void TextBox_GotMouseCapture(object sender, MouseEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.SelectAll();
            Freeze = true;
        }

        private void TextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!Modified)
                Freeze = false;
        }

        private void ComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;

            if (e.Key == Key.Enter)
                cb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
        }

        private void ComboBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Freeze = true;
        }

        private void ComboBox_GotMouseCapture(object sender, MouseEventArgs e)
        {
            Freeze = true;
        }

        private void Combo_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!Modified)
                Freeze = false;
        }
    }
}
