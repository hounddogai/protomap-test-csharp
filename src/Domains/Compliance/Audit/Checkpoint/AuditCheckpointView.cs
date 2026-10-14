namespace Acme.Workforce.Compliance.Audit.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a audit checkpoint.</summary>
public sealed record AuditCheckpointView(string CodeText, string Name, string Country, string Region, string LeaveDays, string StatusLabel)
{
    public static AuditCheckpointView ToAuditCheckpointView(AuditCheckpoint item)
    {
        return new AuditCheckpointView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Country,
            item.Region,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AuditCheckpointView> ToAuditCheckpointViews(List<AuditCheckpoint> items)
    {
        var views = new List<AuditCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToAuditCheckpointView(item));
        }
        return views;
    }

    public static double SumAuditCheckpointView(AuditCheckpointView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays;
    }
}
