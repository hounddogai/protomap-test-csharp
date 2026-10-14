namespace Acme.Workforce.Payrollops.Garnishment.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered garnishment quota.</summary>
public sealed record TieredGarnishmentQuotaView(string CodeText, string Title, string CreatedAt, string Maximum, string Name, string RegionText, string StatusLabel)
{
    public static TieredGarnishmentQuotaView ToTieredGarnishmentQuotaView(TieredGarnishmentQuota item)
    {
        return new TieredGarnishmentQuotaView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredGarnishmentQuotaView> ToTieredGarnishmentQuotaViews(List<TieredGarnishmentQuota> items)
    {
        var views = new List<TieredGarnishmentQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToTieredGarnishmentQuotaView(item));
        }
        return views;
    }

    public static double SumTieredGarnishmentQuotaView(TieredGarnishmentQuotaView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + parsedMaximum;
    }
}
