namespace Acme.Workforce.Orgdesign.Headcount.Proposal;

using System.Globalization;

/// <summary>How screens and exports show a adaptive headcount audit.</summary>
public sealed record AdaptiveHeadcountAuditView(string Code, string Label, string BatchSize, string UpdatedAt, string Threshold, string ExpiresAt, string Grade, string NotesText, string StatusLabel)
{
    public static AdaptiveHeadcountAuditView ToAdaptiveHeadcountAuditView(AdaptiveHeadcountAudit item)
    {
        return new AdaptiveHeadcountAuditView(
            item.Code,
            item.Label,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Notes,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveHeadcountAuditView> ToAdaptiveHeadcountAuditViews(List<AdaptiveHeadcountAudit> items)
    {
        var views = new List<AdaptiveHeadcountAuditView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveHeadcountAuditView(item));
        }
        return views;
    }

    public static double SumAdaptiveHeadcountAuditView(AdaptiveHeadcountAuditView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedUpdatedAt + parsedThreshold;
    }
}
