namespace Acme.Workforce.Compliance.Audit.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a audit mapping.</summary>
public sealed record AuditMappingView(string Code, string NameText, string Timezone, string ManagerIdText, string Currency, string Sequence, string Segment, string Retries, string OwnerId, string Phone, string GraceDays, string StatusLabel)
{
    public static AuditMappingView ToAuditMappingView(AuditMapping item)
    {
        return new AuditMappingView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Timezone,
            item.ManagerId,
            item.Currency,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Phone,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AuditMappingView> ToAuditMappingViews(List<AuditMapping> items)
    {
        var views = new List<AuditMappingView>();
        foreach (var item in items)
        {
            views.Add(ToAuditMappingView(item));
        }
        return views;
    }

    public static double SumAuditMappingView(AuditMappingView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedRetries + (double)parsedGraceDays;
    }
}
