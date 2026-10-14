namespace Acme.Workforce.Assets.Laptop.Baseline;

using System.Globalization;

/// <summary>How screens and exports show a laptop slot.</summary>
public sealed record LaptopSlotView(string CodeText, string Title, string RatioText, string CostText, string Quantity, string CostCenter, string Category, string Unit, string Country, string Minimum, string Source, string StatusLabel)
{
    public static LaptopSlotView ToLaptopSlotView(LaptopSlot item)
    {
        return new LaptopSlotView(
            item.Code,
            item.Title,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Category,
            item.Unit,
            item.Country,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LaptopSlotView> ToLaptopSlotViews(List<LaptopSlot> items)
    {
        var views = new List<LaptopSlotView>();
        foreach (var item in items)
        {
            views.Add(ToLaptopSlotView(item));
        }
        return views;
    }

    public static double SumLaptopSlotView(LaptopSlotView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return parsedRatio + parsedCost + (double)parsedQuantity;
    }
}
