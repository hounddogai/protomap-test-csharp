namespace Acme.Workforce.Engagement.PulseSurvey.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual pulse survey audit.</summary>
public sealed record ManualPulseSurveyAuditView(string Code, string LabelText, string CostCenter, string BatchSizeText, string Checksum, string RateText, string Reviewer, string LeaveDays, string Capacity, string BadgeText, string StatusLabel)
{
    public static ManualPulseSurveyAuditView ToManualPulseSurveyAuditView(ManualPulseSurveyAudit item)
    {
        return new ManualPulseSurveyAuditView(
            item.Code,
            item.Label,
            Text.Truncate(item.CostCenter, 64),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualPulseSurveyAuditView> ToManualPulseSurveyAuditViews(List<ManualPulseSurveyAudit> items)
    {
        var views = new List<ManualPulseSurveyAuditView>();
        foreach (var item in items)
        {
            views.Add(ToManualPulseSurveyAuditView(item));
        }
        return views;
    }

    public static double SumManualPulseSurveyAuditView(ManualPulseSurveyAuditView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedRate + (double)parsedLeaveDays;
    }
}
