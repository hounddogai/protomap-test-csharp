namespace Acme.Workforce.Compliance.Audit.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a audit record.</summary>
public sealed record AuditRecordView(string Code, string LabelText, string Reviewer, string Rate, string Locale, string Region, string CreatedAt, string MarginText, string Limit, string Notes, string Segment, string StatusLabel)
{
    public static AuditRecordView ToAuditRecordView(AuditRecord item)
    {
        return new AuditRecordView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Reviewer,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            item.Region,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            Text.Truncate(item.Segment, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AuditRecordView> ToAuditRecordViews(List<AuditRecord> items)
    {
        var views = new List<AuditRecordView>();
        foreach (var item in items)
        {
            views.Add(ToAuditRecordView(item));
        }
        return views;
    }

    public static double SumAuditRecordView(AuditRecordView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedCreatedAt + parsedMargin;
    }
}
