namespace Acme.Workforce.Engagement.PulseSurvey.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy pulse survey checkpoint.</summary>
public sealed record LegacyPulseSurveyCheckpointView(string Code, string Label, string Version, string Minimum, string Category, string ManagerIdText, string ExpiresAtText, string Quantity, string Ceiling, string Visible, string StatusLabel)
{
    public static LegacyPulseSurveyCheckpointView ToLegacyPulseSurveyCheckpointView(LegacyPulseSurveyCheckpoint item)
    {
        return new LegacyPulseSurveyCheckpointView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            Text.Truncate(item.ManagerId, 80),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyPulseSurveyCheckpointView> ToLegacyPulseSurveyCheckpointViews(List<LegacyPulseSurveyCheckpoint> items)
    {
        var views = new List<LegacyPulseSurveyCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyPulseSurveyCheckpointView(item));
        }
        return views;
    }

    public static double SumLegacyPulseSurveyCheckpointView(LegacyPulseSurveyCheckpointView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedMinimum + (double)parsedExpiresAt;
    }
}
