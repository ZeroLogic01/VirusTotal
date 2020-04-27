using System.Windows.Media;

namespace VirusTotalUI.Static
{
    internal static class Brushes
    {
        internal static Brush Green { get; private set; } = new SolidColorBrush(Color.FromRgb(129, 184, 86));
        internal static Brush Yellow { get; private set; } = new SolidColorBrush(Color.FromRgb(234, 201, 68));
        internal static Brush Red { get; private set; } = new SolidColorBrush(Color.FromRgb(237, 28, 36));
        internal static Brush Gray { get; private set; } = new SolidColorBrush(Color.FromRgb(164, 164, 164));
    }
}
