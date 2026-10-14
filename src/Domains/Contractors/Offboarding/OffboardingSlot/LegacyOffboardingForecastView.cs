namespace Acme.Workforce.Contractors.Offboarding.OffboardingSlot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy offboarding forecast.</summary>
public sealed record LegacyOffboardingForecastView(string CodeText, string Label, string WindowDays, string Channel, string DescriptionText, string JobCode, string SalaryBand, string PinnedText, string LocaleText, string DurationMinutesText, string Minimum, string StatusLabel)
{
    public static LegacyOffboardingForecastView ToLegacyOffboardingForecastView(LegacyOffboardingForecast item)
    {
        return new LegacyOffboardingForecastView(
            item.Code,
            item.Label,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 40),
            item.Description,
            Text.Truncate(item.JobCode, 40),
            item.SalaryBand,
            $"{item.Pinned}",
            item.Locale,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyOffboardingForecastView> ToLegacyOffboardingForecastViews(List<LegacyOffboardingForecast> items)
    {
        var views = new List<LegacyOffboardingForecastView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyOffboardingForecastView(item));
        }
        return views;
    }

    public static double SumLegacyOffboardingForecastView(LegacyOffboardingForecastView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedDurationMinutes + parsedMinimum;
    }
}
