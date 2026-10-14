namespace Acme.Workforce.Assets.Laptop.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict laptop request.</summary>
public sealed record StrictLaptopRequestView(string Code, string Label, string Cost, string RetriesText, string Segment, string AmountText, string Fte, string StatusLabel)
{
    public static StrictLaptopRequestView ToStrictLaptopRequestView(StrictLaptopRequest item)
    {
        return new StrictLaptopRequestView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictLaptopRequestView> ToStrictLaptopRequestViews(List<StrictLaptopRequest> items)
    {
        var views = new List<StrictLaptopRequestView>();
        foreach (var item in items)
        {
            views.Add(ToStrictLaptopRequestView(item));
        }
        return views;
    }

    public static double SumStrictLaptopRequestView(StrictLaptopRequestView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedRetries + parsedAmount;
    }
}
