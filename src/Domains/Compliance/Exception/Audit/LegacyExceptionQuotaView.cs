namespace Acme.Workforce.Compliance.Exception.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy exception quota.</summary>
public sealed record LegacyExceptionQuotaView(string Code, string Title, string EmployeeId, string ExpiresAtText, string Team, string Department, string QuantityText, string StatusLabel)
{
    public static LegacyExceptionQuotaView ToLegacyExceptionQuotaView(LegacyExceptionQuota item)
    {
        return new LegacyExceptionQuotaView(
            item.Code,
            Text.Truncate(item.Title, 120),
            Text.Truncate(item.EmployeeId, 80),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Team,
            item.Department,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyExceptionQuotaView> ToLegacyExceptionQuotaViews(List<LegacyExceptionQuota> items)
    {
        var views = new List<LegacyExceptionQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyExceptionQuotaView(item));
        }
        return views;
    }

    public static double SumLegacyExceptionQuotaView(LegacyExceptionQuotaView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedQuantity;
    }
}
