namespace Acme.Workforce.Assets.Warranty.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered warranty profile.</summary>
public sealed record TieredWarrantyProfileView(string CodeText, string Name, string FteText, string Score, string RateText, string Segment, string StatusLabel)
{
    public static TieredWarrantyProfileView ToTieredWarrantyProfileView(TieredWarrantyProfile item)
    {
        return new TieredWarrantyProfileView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredWarrantyProfileView> ToTieredWarrantyProfileViews(List<TieredWarrantyProfile> items)
    {
        var views = new List<TieredWarrantyProfileView>();
        foreach (var item in items)
        {
            views.Add(ToTieredWarrantyProfileView(item));
        }
        return views;
    }

    public static double SumTieredWarrantyProfileView(TieredWarrantyProfileView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return parsedFte + parsedScore + parsedRate;
    }
}
