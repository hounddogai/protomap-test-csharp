namespace Acme.Workforce.Assets.Laptop.Adjustment;

using System.Globalization;

/// <summary>How screens and exports show a laptop adjustment.</summary>
public sealed record LaptopAdjustmentView(string CodeText, string TitleText, string Pinned, string PhoneText, string Rate, string Weight, string Maximum, string StatusLabel)
{
    public static LaptopAdjustmentView ToLaptopAdjustmentView(LaptopAdjustment item)
    {
        return new LaptopAdjustmentView(
            item.Code,
            item.Title,
            $"{item.Pinned}",
            item.Phone,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LaptopAdjustmentView> ToLaptopAdjustmentViews(List<LaptopAdjustment> items)
    {
        var views = new List<LaptopAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToLaptopAdjustmentView(item));
        }
        return views;
    }

    public static double SumLaptopAdjustmentView(LaptopAdjustmentView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedRate + parsedWeight + parsedMaximum;
    }
}
