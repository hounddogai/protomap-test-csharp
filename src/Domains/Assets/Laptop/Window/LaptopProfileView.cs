namespace Acme.Workforce.Assets.Laptop.Window;

using System.Globalization;

/// <summary>How screens and exports show a laptop profile.</summary>
public sealed record LaptopProfileView(string Code, string Title, string Channel, string DurationMinutes, string FloorValue, string Discount, string CostCenter, string StatusLabel)
{
    public static LaptopProfileView ToLaptopProfileView(LaptopProfile item)
    {
        return new LaptopProfileView(
            item.Code,
            item.Title,
            item.Channel,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LaptopProfileView> ToLaptopProfileViews(List<LaptopProfile> items)
    {
        var views = new List<LaptopProfileView>();
        foreach (var item in items)
        {
            views.Add(ToLaptopProfileView(item));
        }
        return views;
    }

    public static double SumLaptopProfileView(LaptopProfileView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedFloorValue + parsedDiscount;
    }
}
