namespace Acme.Workforce.Workplace.Visitor.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a visitor quota.</summary>
public sealed record VisitorQuotaView(string CodeText, string Label, string Unit, string CeilingText, string ExpiresAtText, string Source, string StatusLabel)
{
    public static VisitorQuotaView ToVisitorQuotaView(VisitorQuota item)
    {
        return new VisitorQuotaView(
            item.Code,
            Text.Truncate(item.Label, 40),
            Text.Truncate(item.Unit, 80),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorQuotaView> ToVisitorQuotaViews(List<VisitorQuota> items)
    {
        var views = new List<VisitorQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorQuotaView(item));
        }
        return views;
    }

    public static double SumVisitorQuotaView(VisitorQuotaView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedExpiresAt;
    }
}
