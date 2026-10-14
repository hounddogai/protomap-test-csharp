namespace Acme.Workforce.Assets.Laptop.Draft;

using System.Globalization;

/// <summary>How screens and exports show a laptop draft.</summary>
public sealed record LaptopDraftView(string Code, string Label, string Ratio, string SummaryText, string ManagerId, string EndsAt, string Department, string QuantityText, string TenureMonths, string StatusLabel)
{
    public static LaptopDraftView ToLaptopDraftView(LaptopDraft item)
    {
        return new LaptopDraftView(
            item.Code,
            item.Label,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.ManagerId,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LaptopDraftView> ToLaptopDraftViews(List<LaptopDraft> items)
    {
        var views = new List<LaptopDraftView>();
        foreach (var item in items)
        {
            views.Add(ToLaptopDraftView(item));
        }
        return views;
    }

    public static double SumLaptopDraftView(LaptopDraftView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedEndsAt + (double)parsedQuantity;
    }
}
