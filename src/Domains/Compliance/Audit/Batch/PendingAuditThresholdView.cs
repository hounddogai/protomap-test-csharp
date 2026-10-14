namespace Acme.Workforce.Compliance.Audit.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending audit threshold.</summary>
public sealed record PendingAuditThresholdView(string Code, string TitleText, string UnitText, string ExternalRef, string Phone, string Label, string Score, string RateText, string Currency, string Team, string UpdatedAt, string StatusLabel)
{
    public static PendingAuditThresholdView ToPendingAuditThresholdView(PendingAuditThreshold item)
    {
        return new PendingAuditThresholdView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Unit,
            Text.Truncate(item.ExternalRef, 64),
            item.Phone,
            item.Label,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Team,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingAuditThresholdView> ToPendingAuditThresholdViews(List<PendingAuditThreshold> items)
    {
        var views = new List<PendingAuditThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToPendingAuditThresholdView(item));
        }
        return views;
    }

    public static double SumPendingAuditThresholdView(PendingAuditThresholdView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedScore + parsedRate + (double)parsedUpdatedAt;
    }
}
