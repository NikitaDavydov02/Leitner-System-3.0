using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Globalization;
using Leitner_System_Transfered_2.Model;

namespace Leitner_System_Transfered_2.ViewModel
{
    class IntegerToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value is int) && ((int)value) == 0)
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    class ResultToIntegerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value is Result) && (Result)value == Result.NoAnswer)
                return 0;
            if ((value is Result) && (Result)value == Result.Right)
                return 1;
            if ((value is Result) && (Result)value == Result.Wrong)
                return 2;
            if ((value is Result) && (Result)value == Result.Delete)
                return 3;
            else
                return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value is int) && (int)value == 0)
                return Result.NoAnswer;
            if ((value is int) && (int)value == 1)
                return Result.Right;
            if ((value is int) && (int)value == 2)
                return Result.Wrong;
            if ((value is int) && (int)value == 3)
                return Result.Delete;
            else
                return Result.NoAnswer;
        }
    }

    class ReverseSettingsToIntegerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value is ReverseSettings) && (ReverseSettings)value == ReverseSettings.Straight)
                return 0;
            if ((value is ReverseSettings) && (ReverseSettings)value == ReverseSettings.Reverse)
                return 1;
            if ((value is ReverseSettings) && (ReverseSettings)value == ReverseSettings.Random)
                return 2;
            if ((value is ReverseSettings) && (ReverseSettings)value == ReverseSettings.Manual)
                return 3;
            else
                return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value is int) && (int)value == 0)
                return ReverseSettings.Straight;
            if ((value is int) && (int)value == 1)
                return ReverseSettings.Reverse;
            if ((value is int) && (int)value == 2)
                return ReverseSettings.Random;
            if ((value is int) && (int)value == 3)
                return ReverseSettings.Manual;
            else
                return ReverseSettings.Straight;
        }
    }
}
