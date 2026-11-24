using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace BillarPatrik.Vista
{
    public static class MauiImageSourceHelper
    {
        public static ImageSource Resolve(string? source, string fallback)
        {
            if (!string.IsNullOrWhiteSpace(source)
                && (Path.IsPathRooted(source) || source.StartsWith("file:", StringComparison.OrdinalIgnoreCase)))
            {
                string filePath = source.StartsWith("file:", StringComparison.OrdinalIgnoreCase)
                    ? new Uri(source).LocalPath
                    : source;

                if (File.Exists(filePath))
                    return ImageSource.FromFile(filePath);
            }

            if (!string.IsNullOrWhiteSpace(source)
                && source.IndexOfAny(new[] { '/', '\\' }) >= 0)
            {
                string fileName = Path.GetFileName(source);
                if (!string.IsNullOrWhiteSpace(fileName))
                    return ImageSource.FromFile(fileName);
            }

            if (!string.IsNullOrWhiteSpace(source))
                return ImageSource.FromFile(source);

            return ImageSource.FromFile(fallback);
        }
    }

    public class BoolToEstadoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool ocupada)
                return "LIBRE";

            return ocupada ? "OCUPADA" : "LIBRE";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}