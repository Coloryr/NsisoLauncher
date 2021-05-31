using System.Windows.Media;

namespace NsisoLauncher.Utils
{
    class BrushColor
    {
        public static Brush GetBursh()
        {
            try
            {
                if (App.Config.MainConfig.Customize.AppThme == null)
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AF3F5966"));
                string color = App.Config.MainConfig.Customize.AppThme.Replace("Light.", "").Replace("Dark.", "");
                switch (color)
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
        public static Brush GetLDBursh()
        {
            try
            {
                if (App.Config.MainConfig.Customize.AppThme == null)
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8F8FF"));
                string color = App.Config.MainConfig.Customize.AppThme.Replace("Light.", "").Replace("Dark.", "");
                if (App.Config.MainConfig.Customize.AppThme.StartsWith("Light"))
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8F8FF"));
                }
                else
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#696969"));
                }
            }
            catch
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8F8FF"));
            }
        }
        public static Brush GetNBursh()
        {
            try
            {
                if (App.Config.MainConfig.Customize.AppThme == null)
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3F5966"));
                string color = App.Config.MainConfig.Customize.AppThme.Replace("Light.", "").Replace("Dark.", "");
                switch (color)
                {
                    case "Red":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BE1707"));
                    case "Green":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#548E19"));
                    case "Blue":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1585B5"));
                    case "Purple":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#574EB9"));
                    case "Orange":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CF5A07"));
                    case "Lime":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8AA407"));
                    case "Emerald":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#077507"));
                    case "Teal":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#07908E"));
                    case "Cyan":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1D88BC"));
                    case "Cobalt":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0747C6"));
                    case "Indigo":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5C07D3"));
                    case "Violet":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8F07D3"));
                    case "Pink":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CA62AD"));
                    case "Magenta":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B40763"));
                    case "Crimson":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#890725"));
                    case "Amber":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C7890F"));
                    case "Yellow":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D2B90C"));
                    case "Browns":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6F4F2A"));
                    case "Olive":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5E7357"));
                    case "Steel":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#576573"));
                    case "mauve":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#655475"));
                    case "Taupe":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#736845"));
                    case "Sienna":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#87492B"));
                    default:
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3F5966"));
                }
            }
            catch
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3F5966"));
            }
        }
    }
}
