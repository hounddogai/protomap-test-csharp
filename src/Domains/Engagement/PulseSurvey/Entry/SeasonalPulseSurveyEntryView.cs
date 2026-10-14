namespace Acme.Workforce.Engagement.PulseSurvey.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal pulse survey entry.</summary>
public sealed record SeasonalPulseSurveyEntryView(string CodeText, string Label, string CostCenterText, string VisibleText, string Category, string Priority, string TenureMonths, string Notes, string Reviewer, string ExpiresAt, string StatusLabel)
{
    public static SeasonalPulseSurveyEntryView ToSeasonalPulseSurveyEntryView(SeasonalPulseSurveyEntry item)
    {
        return new SeasonalPulseSurveyEntryView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.CostCenter,
            $"{item.Visible}",
            item.Category,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 40),
            item.Reviewer,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalPulseSurveyEntryView> ToSeasonalPulseSurveyEntryViews(List<SeasonalPulseSurveyEntry> items)
    {
        var views = new List<SeasonalPulseSurveyEntryView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalPulseSurveyEntryView(item));
        }
        return views;
    }

    public static double SumSeasonalPulseSurveyEntryView(SeasonalPulseSurveyEntryView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedTenureMonths + (double)parsedExpiresAt;
    }
}
