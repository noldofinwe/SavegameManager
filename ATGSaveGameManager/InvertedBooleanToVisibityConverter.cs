﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace ATGSaveGameManager
{
        class InvertedBooleanToVisibityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value is Boolean && (bool)value)
                {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value is Visibility && (Visibility)value == Visibility.Collapsed)
                {
                    return true;
                }
                return false;
            }
        
    }
}
