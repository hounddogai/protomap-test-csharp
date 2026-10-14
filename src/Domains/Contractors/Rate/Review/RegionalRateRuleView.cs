namespace Acme.Workforce.Contractors.Rate.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional rate rule.</summary>
public sealed record RegionalRateRuleView(string CodeText, string Title, string Weight, string Limit, string PriorityText, string Fte, string SequenceText, string StatusLabel)
{
    public static RegionalRateRuleView ToRegionalRateRuleView(RegionalRateRule item)
    {
        return new RegionalRateRuleView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 64),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalRateRuleView> ToRegionalRateRuleViews(List<RegionalRateRule> items)
    {
        var views = new List<RegionalRateRuleView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalRateRuleView(item));
        }
        return views;
    }

    public static double SumRegionalRateRuleView(RegionalRateRuleView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedLimit + (double)parsedPriority;
    }
}
