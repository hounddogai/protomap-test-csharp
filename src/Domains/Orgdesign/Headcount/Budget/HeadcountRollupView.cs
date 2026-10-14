namespace Acme.Workforce.Orgdesign.Headcount.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a headcount rollup.</summary>
public sealed record HeadcountRollupView(string Code, string Label, string Pinned, string Name, string TenureMonthsText, string TitleText, string Discount, string StatusLabel)
{
    public static HeadcountRollupView ToHeadcountRollupView(HeadcountRollup item)
    {
        return new HeadcountRollupView(
            item.Code,
            Text.Truncate(item.Label, 40),
            $"{item.Pinned}",
            item.Name,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Title, 120),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HeadcountRollupView> ToHeadcountRollupViews(List<HeadcountRollup> items)
    {
        var views = new List<HeadcountRollupView>();
        foreach (var item in items)
        {
            views.Add(ToHeadcountRollupView(item));
        }
        return views;
    }

    public static double SumHeadcountRollupView(HeadcountRollupView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + parsedDiscount;
    }
}
