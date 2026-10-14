namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched audit template.</summary>
public sealed record BatchedAuditTemplateView(string Code, string TitleText, string Segment, string Ceiling, string Label, string BudgetText, string Timezone, string Grade, string EmployeeId, string Score, string StatusLabel)
{
    public static BatchedAuditTemplateView ToBatchedAuditTemplateView(BatchedAuditTemplate item)
    {
        return new BatchedAuditTemplateView(
            item.Code,
            item.Title,
            item.Segment,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Label, 40),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            item.Grade,
            item.EmployeeId,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedAuditTemplateView> ToBatchedAuditTemplateViews(List<BatchedAuditTemplate> items)
    {
        var views = new List<BatchedAuditTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedAuditTemplateView(item));
        }
        return views;
    }

    public static double SumBatchedAuditTemplateView(BatchedAuditTemplateView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedBudget + parsedScore;
    }
}
