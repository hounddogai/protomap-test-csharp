namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a audit forecast.</summary>
public sealed record AuditForecastView(string CodeText, string Name, string Weight, string Cost, string UnitText, string ExpiresAt, string OwnerIdText, string Approved, string StatusLabel)
{
    public static AuditForecastView ToAuditForecastView(AuditForecast item)
    {
        return new AuditForecastView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 64),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AuditForecastView> ToAuditForecastViews(List<AuditForecast> items)
    {
        var views = new List<AuditForecastView>();
        foreach (var item in items)
        {
            views.Add(ToAuditForecastView(item));
        }
        return views;
    }

    public static double SumAuditForecastView(AuditForecastView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return parsedWeight + parsedCost + (double)parsedExpiresAt;
    }
}
