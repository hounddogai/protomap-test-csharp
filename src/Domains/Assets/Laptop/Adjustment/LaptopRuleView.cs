namespace Acme.Workforce.Assets.Laptop.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a laptop rule.</summary>
public sealed record LaptopRuleView(string Code, string Title, string Timezone, string Unit, string CostCenter, string Amount, string ApprovedText, string Location, string StatusLabel)
{
    public static LaptopRuleView ToLaptopRuleView(LaptopRule item)
    {
        return new LaptopRuleView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 80),
            item.Timezone,
            item.Unit,
            item.CostCenter,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            Text.Truncate(item.Location, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LaptopRuleView> ToLaptopRuleViews(List<LaptopRule> items)
    {
        var views = new List<LaptopRuleView>();
        foreach (var item in items)
        {
            views.Add(ToLaptopRuleView(item));
        }
        return views;
    }

    public static double SumLaptopRuleView(LaptopRuleView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
