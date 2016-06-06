using OptimalPlayer.Model;
using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace OptimalPlayer.ViewModel
{
    public class BackgroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            ListViewItem item = (ListViewItem)values[0];
            AudioFile filePlaying = values[1] as AudioFile;

            if (item.DataContext == filePlaying)
            {
                return new SolidColorBrush(Colors.Lavender);
            }
            else
            {
                return new SolidColorBrush(Colors.Transparent);
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
