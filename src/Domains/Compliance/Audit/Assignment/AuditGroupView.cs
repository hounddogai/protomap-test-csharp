namespace Acme.Workforce.Compliance.Audit.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a audit group.</summary>
public sealed record AuditGroupView(string Code, string LabelText, string Timezone, string Limit, string Unit, string FloorValueText, string Rate, string Capacity, string StatusLabel)
{
    public static AuditGroupView ToAuditGroupView(AuditGroup item)
    {
        return new AuditGroupView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Label, 80),
            item.Timezone,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 40),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AuditGroupView> ToAuditGroupViews(List<AuditGroup> items)
    {
        var views = new List<AuditGroupView>();
        foreach (var item in items)
        {
            views.Add(ToAuditGroupView(item));
        }
        return views;
    }

    public static double SumAuditGroupView(AuditGroupView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedFloorValue + parsedRate;
    }
}
