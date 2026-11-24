using System.Globalization;

namespace BillarPatrik.Vista
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool ocupada = value is bool b && b;
            return ocupada ? Color.FromArgb("#3A1719") : Color.FromArgb("#17382A");
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToEstadoConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool ocupada = value is bool b && b;
            return ocupada ? "OCUPADA" : "DISPONIBLE";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}