namespace Acme.Workforce.Assets.Warranty.Quota;

using System.Globalization;

/// <summary>How screens and exports show a warranty quota.</summary>
public sealed record WarrantyQuotaView(string CodeText, string NameText, string UnitText, string CreatedAtText, string StatusLabel)
{
    public static WarrantyQuotaView ToWarrantyQuotaView(WarrantyQuota item)
    {
        return new WarrantyQuotaView(
            item.Code,
            item.Name,
            item.Unit,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyQuotaView> ToWarrantyQuotaViews(List<WarrantyQuota> items)
    {
        var views = new List<WarrantyQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyQuotaView(item));
        }
        return views;
    }

    public static double SumWarrantyQuotaView(WarrantyQuotaView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt;
    }
}
