namespace Acme.Workforce.Compensation.EquityGrant.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a equity grant quota.</summary>
public sealed record EquityGrantQuotaView(string Code, string Name, string Unit, string UpdatedAtText, string Grade, string Attempts, string Phone, string JobCode, string Quantity, string Timezone, string CurrencyText, string StatusLabel)
{
    public static EquityGrantQuotaView ToEquityGrantQuotaView(EquityGrantQuota item)
    {
        return new EquityGrantQuotaView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 120),
            Text.Truncate(item.Unit, 120),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 80),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            item.JobCode,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            Text.Truncate(item.Currency, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantQuotaView> ToEquityGrantQuotaViews(List<EquityGrantQuota> items)
    {
        var views = new List<EquityGrantQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantQuotaView(item));
        }
        return views;
    }

    public static double SumEquityGrantQuotaView(EquityGrantQuotaView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedAttempts + (double)parsedQuantity;
    }
}
