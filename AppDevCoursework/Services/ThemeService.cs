using Microsoft.Maui.Storage;

namespace AppDevCoursework.Services;

public class ThemeService
{
    private const string ThemeKey = "is_dark_mode";
    public event Action? OnThemeChanged;

    public bool IsDarkMode { get; private set; }

    public ThemeService()
    {
        IsDarkMode = Preferences.Get(ThemeKey, false);
    }

    public void ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
        Preferences.Set(ThemeKey, IsDarkMode);
        OnThemeChanged?.Invoke();
    }
}
