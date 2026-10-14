namespace Acme.Workforce.Orgdesign.CostModel.ModelManifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped cost model rule.</summary>
public sealed record CappedCostModelRuleView(string Code, string NameText, string CategoryText, string Timezone, string Discount, string Visible, string Fte, string CostCenter, string Ratio, string Country, string StatusLabel)
{
    public static CappedCostModelRuleView ToCappedCostModelRuleView(CappedCostModelRule item)
    {
        return new CappedCostModelRuleView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Category,
            Text.Truncate(item.Timezone, 64),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedCostModelRuleView> ToCappedCostModelRuleViews(List<CappedCostModelRule> items)
    {
        var views = new List<CappedCostModelRuleView>();
        foreach (var item in items)
        {
            views.Add(ToCappedCostModelRuleView(item));
        }
        return views;
    }

    public static double SumCappedCostModelRuleView(CappedCostModelRuleView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedFte + parsedRatio;
    }
}
