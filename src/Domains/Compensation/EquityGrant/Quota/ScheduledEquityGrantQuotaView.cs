namespace Acme.Workforce.Compensation.EquityGrant.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled equity grant quota.</summary>
public sealed record ScheduledEquityGrantQuotaView(string Code, string TitleText, string CategoryText, string Quantity, string CurrencyText, string Version, string StatusLabel)
{
    public static ScheduledEquityGrantQuotaView ToScheduledEquityGrantQuotaView(ScheduledEquityGrantQuota item)
    {
        return new ScheduledEquityGrantQuotaView(
            item.Code,
            item.Title,
            Text.Truncate(item.Category, 120),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledEquityGrantQuotaView> ToScheduledEquityGrantQuotaViews(List<ScheduledEquityGrantQuota> items)
    {
        var views = new List<ScheduledEquityGrantQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledEquityGrantQuotaView(item));
        }
        return views;
    }

    public static double SumScheduledEquityGrantQuotaView(ScheduledEquityGrantQuotaView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedVersion;
    }
}
