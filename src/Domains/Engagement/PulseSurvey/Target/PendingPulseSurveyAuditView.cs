namespace Acme.Workforce.Engagement.PulseSurvey.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending pulse survey audit.</summary>
public sealed record PendingPulseSurveyAuditView(string CodeText, string TitleText, string LocaleText, string LeaveDays, string RegionText, string StatusLabel)
{
    public static PendingPulseSurveyAuditView ToPendingPulseSurveyAuditView(PendingPulseSurveyAudit item)
    {
        return new PendingPulseSurveyAuditView(
            item.Code,
            Text.Truncate(item.Title, 40),
            Text.Truncate(item.Locale, 64),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingPulseSurveyAuditView> ToPendingPulseSurveyAuditViews(List<PendingPulseSurveyAudit> items)
    {
        var views = new List<PendingPulseSurveyAuditView>();
        foreach (var item in items)
        {
            views.Add(ToPendingPulseSurveyAuditView(item));
        }
        return views;
    }

    public static double SumPendingPulseSurveyAuditView(PendingPulseSurveyAuditView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays;
    }
}
