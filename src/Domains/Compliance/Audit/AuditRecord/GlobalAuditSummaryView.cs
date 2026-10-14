namespace Acme.Workforce.Compliance.Audit.AuditRecord;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global audit summary.</summary>
public sealed record GlobalAuditSummaryView(string Code, string LabelText, string JobCode, string LeaveDays, string Minimum, string Limit, string Threshold, string Score, string StatusLabel)
{
    public static GlobalAuditSummaryView ToGlobalAuditSummaryView(GlobalAuditSummary item)
    {
        return new GlobalAuditSummaryView(
            item.Code,
            Text.Truncate(item.Label, 80),
            Text.Truncate(item.JobCode, 64),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalAuditSummaryView> ToGlobalAuditSummaryViews(List<GlobalAuditSummary> items)
    {
        var views = new List<GlobalAuditSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalAuditSummaryView(item));
        }
        return views;
    }

    public static double SumGlobalAuditSummaryView(GlobalAuditSummaryView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedMinimum + (double)parsedLimit;
    }
}
