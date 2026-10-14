namespace Acme.Workforce.Compliance.Exception.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional exception preference.</summary>
public sealed record RegionalExceptionPreferenceView(string Code, string Label, string BadgeText, string LocaleText, string DurationMinutes, string ExpiresAt, string StatusLabel)
{
    public static RegionalExceptionPreferenceView ToRegionalExceptionPreferenceView(RegionalExceptionPreference item)
    {
        return new RegionalExceptionPreferenceView(
            item.Code,
            item.Label,
            item.Badge,
            Text.Truncate(item.Locale, 40),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalExceptionPreferenceView> ToRegionalExceptionPreferenceViews(List<RegionalExceptionPreference> items)
    {
        var views = new List<RegionalExceptionPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalExceptionPreferenceView(item));
        }
        return views;
    }

    public static double SumRegionalExceptionPreferenceView(RegionalExceptionPreferenceView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedExpiresAt;
    }
}
