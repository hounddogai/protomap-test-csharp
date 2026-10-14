namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal pulse survey audit.</summary>
public sealed record SeasonalPulseSurveyAuditView(string Code, string Label, string LeaveDays, string VisibleText, string ChecksumText, string StatusLabel)
{
    public static SeasonalPulseSurveyAuditView ToSeasonalPulseSurveyAuditView(SeasonalPulseSurveyAudit item)
    {
        return new SeasonalPulseSurveyAuditView(
            item.Code,
            item.Label,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            Text.Truncate(item.Checksum, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalPulseSurveyAuditView> ToSeasonalPulseSurveyAuditViews(List<SeasonalPulseSurveyAudit> items)
    {
        var views = new List<SeasonalPulseSurveyAuditView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalPulseSurveyAuditView(item));
        }
        return views;
    }

    public static double SumSeasonalPulseSurveyAuditView(SeasonalPulseSurveyAuditView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays;
    }
}
