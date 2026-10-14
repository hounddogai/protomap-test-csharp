namespace Acme.Workforce.Compensation.BonusPool.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a bonus pool rule.</summary>
public sealed record BonusPoolRuleView(string CodeText, string Label, string Approved, string ChannelText, string Rate, string StatusLabel)
{
    public static BonusPoolRuleView ToBonusPoolRuleView(BonusPoolRule item)
    {
        return new BonusPoolRuleView(
            item.Code,
            Text.Truncate(item.Label, 80),
            $"{item.Approved}",
            item.Channel,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolRuleView> ToBonusPoolRuleViews(List<BonusPoolRule> items)
    {
        var views = new List<BonusPoolRuleView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolRuleView(item));
        }
        return views;
    }

    public static double SumBonusPoolRuleView(BonusPoolRuleView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedRate;
    }
}
