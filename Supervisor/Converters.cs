using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Supervisor
{
    public class NotBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool notVal = (bool)value;
            return !notVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool notVal = (bool)value;
            return !notVal;
        }
    }

    public class ToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            short val = (short)value;
            return val == 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool val = (bool)value;
            if (val)
                return 1;
            return 0;
        }
    }

    public class StringValue : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double val = (double)value;
            return val.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string val = (string)value;
            if (double.TryParse(val, out double result))
                return result;
            return 0;
        }
    }

    public class BoolVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool vis = (bool)value;

            if (vis)
                return Visibility.Visible;
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility vis = (Visibility)value;

            if (vis == Visibility.Visible)
                return true;
            return false;
        }
    }

    public class BoolBackground : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool modified = (bool)value;

            if (modified)
                return Brushes.LightYellow;
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }

    public class ConnectToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool connected = (bool)value;

            if (connected)
                return new SolidColorBrush(Color.FromArgb(50, 0x00, 0xFF, 0x00));
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }

    public class DisconnectToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool disconnected = (bool)value;

            if (disconnected)
                return new SolidColorBrush(Color.FromArgb(50, 0xFF, 0x00, 0x00));
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }

    public class SelectedToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int selectedIndex = (int)value;

            if (selectedIndex >= 0)
                return true;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return 0;
            return -1;
        }
    }

    public class TypeToIndex : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            RegisterType type = (RegisterType)value;
            switch (type)
            {
                case RegisterType.DiscreteInput:
                    return 1;
                case RegisterType.HoldingRegister:
                    return 2;
                case RegisterType.InputRegister:
                    return 3;
                default:
                    return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int index = (int)value;

            switch (index)
            {
                case 1:
                    return RegisterType.DiscreteInput;
                case 2:
                    return RegisterType.HoldingRegister;
                case 3:
                    return RegisterType.InputRegister;
                default:
                    return RegisterType.Coil;
            }
        }
    }

    public class StringToStates : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<string> states = (List<string>)value;
            string result = string.Empty;

            for (int i = 0; i < states.Count; i++)
                result += $"{states[i]}\r\n";

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string statesList = (string)value;
            string[] separator = { "\r\n" };
            string[] states = statesList.Split(separator, StringSplitOptions.None);
            return states.ToList();
        }
    }

    public class LampValue : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            short val = (short)value;

            if (val == 0)
                return Brushes.LightGray;
            else
                return Brushes.LimeGreen;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
}
