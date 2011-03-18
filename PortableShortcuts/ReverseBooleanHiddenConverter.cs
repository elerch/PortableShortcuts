﻿using System;
using System.Windows;
using System.Windows.Data;

namespace PortableShortcuts
{
    class ReverseBooleanHiddenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return value == (object)Visibility.Hidden;
        }
    }
}
