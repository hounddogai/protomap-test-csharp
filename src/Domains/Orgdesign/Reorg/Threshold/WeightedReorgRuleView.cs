namespace Acme.Workforce.Orgdesign.Reorg.Threshold;

using System.Globalization;

/// <summary>How screens and exports show a weighted reorg rule.</summary>
public sealed record WeightedReorgRuleView(string CodeText, string Name, string VersionText, string Phone, string StatusLabel)
{
    public static WeightedReorgRuleView ToWeightedReorgRuleView(WeightedReorgRule item)
    {
        return new WeightedReorgRuleView(
            item.Code,
            item.Name,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedReorgRuleView> ToWeightedReorgRuleViews(List<WeightedReorgRule> items)
    {
        var views = new List<WeightedReorgRuleView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedReorgRuleView(item));
        }
        return views;
    }

    public static double SumWeightedReorgRuleView(WeightedReorgRuleView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        return (double)parsedVersion;
    }
}
