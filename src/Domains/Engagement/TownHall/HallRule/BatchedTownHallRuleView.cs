namespace Acme.Workforce.Engagement.TownHall.HallRule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched town hall rule.</summary>
public sealed record BatchedTownHallRuleView(string Code, string NameText, string Title, string Retries, string UnitText, string StatusLabel)
{
    public static BatchedTownHallRuleView ToBatchedTownHallRuleView(BatchedTownHallRule item)
    {
        return new BatchedTownHallRuleView(
            item.Code,
            item.Name,
            Text.Truncate(item.Title, 64),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedTownHallRuleView> ToBatchedTownHallRuleViews(List<BatchedTownHallRule> items)
    {
        var views = new List<BatchedTownHallRuleView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedTownHallRuleView(item));
        }
        return views;
    }

    public static double SumBatchedTownHallRuleView(BatchedTownHallRuleView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedRetries;
    }
}
