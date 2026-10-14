namespace Acme.Workforce.Orgdesign.Headcount.Rollup;

using System.Globalization;

/// <summary>How screens and exports show a weighted headcount rollup.</summary>
public sealed record WeightedHeadcountRollupView(string CodeText, string LabelText, string Location, string Pinned, string ContactEmailText, string QuantityText, string StatusLabel)
{
    public static WeightedHeadcountRollupView ToWeightedHeadcountRollupView(WeightedHeadcountRollup item)
    {
        return new WeightedHeadcountRollupView(
            item.Code,
            item.Label,
            item.Location,
            $"{item.Pinned}",
            item.ContactEmail,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedHeadcountRollupView> ToWeightedHeadcountRollupViews(List<WeightedHeadcountRollup> items)
    {
        var views = new List<WeightedHeadcountRollupView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedHeadcountRollupView(item));
        }
        return views;
    }

    public static double SumWeightedHeadcountRollupView(WeightedHeadcountRollupView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
