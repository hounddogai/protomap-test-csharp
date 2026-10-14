namespace Acme.Workforce.Assets.Warranty.Ledger;

using System.Globalization;

/// <summary>How screens and exports show a warranty rule.</summary>
public sealed record WarrantyRuleView(string Code, string Label, string FloorValue, string Score, string LimitText, string StatusLabel)
{
    public static WarrantyRuleView ToWarrantyRuleView(WarrantyRule item)
    {
        return new WarrantyRuleView(
            item.Code,
            item.Label,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyRuleView> ToWarrantyRuleViews(List<WarrantyRule> items)
    {
        var views = new List<WarrantyRuleView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyRuleView(item));
        }
        return views;
    }

    public static double SumWarrantyRuleView(WarrantyRuleView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedScore + (double)parsedLimit;
    }
}
