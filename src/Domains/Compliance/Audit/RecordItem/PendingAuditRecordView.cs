namespace Acme.Workforce.Compliance.Audit.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending audit record.</summary>
public sealed record PendingAuditRecordView(string Code, string Title, string Label, string Ceiling, string MinimumText, string StatusLabel)
{
    public static PendingAuditRecordView ToPendingAuditRecordView(PendingAuditRecord item)
    {
        return new PendingAuditRecordView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Label,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingAuditRecordView> ToPendingAuditRecordViews(List<PendingAuditRecord> items)
    {
        var views = new List<PendingAuditRecordView>();
        foreach (var item in items)
        {
            views.Add(ToPendingAuditRecordView(item));
        }
        return views;
    }

    public static double SumPendingAuditRecordView(PendingAuditRecordView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedMinimum;
    }
}
