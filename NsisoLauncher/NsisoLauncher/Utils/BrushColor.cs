using System.Windows.Media;

namespace NsisoLauncher.Utils
{
    class BrushColor
    {
        public Brush GetBursh()
        {
            try
            {
                if (App.config.MainConfig.Customize.AccentColor == null)
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AF3F5966"));
                switch (App.config.MainConfig.Customize.AccentColor)
                {
                    case "Red":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3CBE1707"));
                    case "Green":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C548E19"));
                    case "Blue":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C1585B5"));
                    case "Purple":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C574EB9"));
                    case "Orange":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3CCF5A07"));
                    case "Lime":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C8AA407"));
                    case "Emerald":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C077507"));
                    case "Teal":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C07908E"));
                    case "Cyan":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C1D88BC"));
                    case "Cobalt":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C0747C6"));
                    case "Indigo":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C5C07D3"));
                    case "Violet":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C8F07D3"));
                    case "Pink":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3CCA62AD"));
                    case "Magenta":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3CB40763"));
                    case "Crimson":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C890725"));
                    case "Amber":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3CC7890F"));
                    case "Yellow":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3CD2B90C"));
                    case "Browns":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C6F4F2A"));
                    case "Olive":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C5E7357"));
                    case "Steel":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C576573"));
                    case "mauve":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C655475"));
                    case "Taupe":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C736845"));
                    case "Sienna":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C87492B"));
                    default:
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AF3F5966"));
                }
            }
            catch
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AF3F5966"));
            }
        }
    }
}
