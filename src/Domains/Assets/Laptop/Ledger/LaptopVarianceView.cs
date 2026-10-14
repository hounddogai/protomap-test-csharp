namespace Acme.Workforce.Assets.Laptop.Ledger;

using System.Globalization;

/// <summary>How screens and exports show a laptop variance.</summary>
public sealed record LaptopVarianceView(string CodeText, string Title, string CostCenterText, string Phone, string ExpiresAtText, string StatusLabel)
{
    public static LaptopVarianceView ToLaptopVarianceView(LaptopVariance item)
    {
        return new LaptopVarianceView(
            item.Code,
            item.Title,
            item.CostCenter,
            item.Phone,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LaptopVarianceView> ToLaptopVarianceViews(List<LaptopVariance> items)
    {
        var views = new List<LaptopVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToLaptopVarianceView(item));
        }
        return views;
    }

    public static double SumLaptopVarianceView(LaptopVarianceView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt;
    }
}
