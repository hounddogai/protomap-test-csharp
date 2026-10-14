namespace Acme.Workforce.Talent.Pool.Audit;

using System.Globalization;

/// <summary>How screens and exports show a pool rule.</summary>
public sealed record PoolRuleView(string Code, string Label, string MarginText, string Active, string TitleText, string StatusLabel)
{
    public static PoolRuleView ToPoolRuleView(PoolRule item)
    {
        return new PoolRuleView(
            item.Code,
            item.Label,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Title,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolRuleView> ToPoolRuleViews(List<PoolRule> items)
    {
        var views = new List<PoolRuleView>();
        foreach (var item in items)
        {
            views.Add(ToPoolRuleView(item));
        }
        return views;
    }

    public static double SumPoolRuleView(PoolRuleView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        return parsedMargin;
    }
}
