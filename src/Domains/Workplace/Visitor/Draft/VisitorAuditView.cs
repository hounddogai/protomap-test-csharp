namespace Acme.Workforce.Workplace.Visitor.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a visitor audit.</summary>
public sealed record VisitorAuditView(string Code, string Label, string Timezone, string GraceDaysText, string ManagerId, string Threshold, string Pinned, string CreatedAt, string Priority, string SummaryText, string StatusLabel)
{
    public static VisitorAuditView ToVisitorAuditView(VisitorAudit item)
    {
        return new VisitorAuditView(
            item.Code,
            item.Label,
            item.Timezone,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorAuditView> ToVisitorAuditViews(List<VisitorAudit> items)
    {
        var views = new List<VisitorAuditView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorAuditView(item));
        }
        return views;
    }

    public static double SumVisitorAuditView(VisitorAuditView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedThreshold + (double)parsedCreatedAt;
    }
}
