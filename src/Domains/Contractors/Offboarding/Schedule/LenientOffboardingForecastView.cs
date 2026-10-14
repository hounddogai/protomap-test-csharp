namespace Acme.Workforce.Contractors.Offboarding.Schedule;

using System.Globalization;

/// <summary>How screens and exports show a lenient offboarding forecast.</summary>
public sealed record LenientOffboardingForecastView(string Code, string Name, string Category, string Threshold, string LocaleText, string ManagerId, string StatusLabel)
{
    public static LenientOffboardingForecastView ToLenientOffboardingForecastView(LenientOffboardingForecast item)
    {
        return new LenientOffboardingForecastView(
            item.Code,
            item.Name,
            item.Category,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientOffboardingForecastView> ToLenientOffboardingForecastViews(List<LenientOffboardingForecast> items)
    {
        var views = new List<LenientOffboardingForecastView>();
        foreach (var item in items)
        {
            views.Add(ToLenientOffboardingForecastView(item));
        }
        return views;
    }

    public static double SumLenientOffboardingForecastView(LenientOffboardingForecastView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedThreshold;
    }
}
