namespace Acme.Workforce.Engagement.PulseSurvey.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict pulse survey tier.</summary>
public sealed record StrictPulseSurveyTierView(string Code, string Label, string LeaveDays, string RateText, string Notes, string SummaryText, string StatusLabel)
{
    public static StrictPulseSurveyTierView ToStrictPulseSurveyTierView(StrictPulseSurveyTier item)
    {
        return new StrictPulseSurveyTierView(
            item.Code,
            item.Label,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            Text.Truncate(item.Summary, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictPulseSurveyTierView> ToStrictPulseSurveyTierViews(List<StrictPulseSurveyTier> items)
    {
        var views = new List<StrictPulseSurveyTierView>();
        foreach (var item in items)
        {
            views.Add(ToStrictPulseSurveyTierView(item));
        }
        return views;
    }

    public static double SumStrictPulseSurveyTierView(StrictPulseSurveyTierView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedRate;
    }
}
