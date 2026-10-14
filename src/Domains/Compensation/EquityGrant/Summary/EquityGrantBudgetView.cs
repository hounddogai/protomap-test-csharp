namespace Acme.Workforce.Compensation.EquityGrant.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a equity grant budget.</summary>
public sealed record EquityGrantBudgetView(string CodeText, string Label, string SalaryBand, string OwnerId, string BadgeText, string Channel, string Rate, string StatusLabel)
{
    public static EquityGrantBudgetView ToEquityGrantBudgetView(EquityGrantBudget item)
    {
        return new EquityGrantBudgetView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.SalaryBand,
            Text.Truncate(item.OwnerId, 64),
            Text.Truncate(item.Badge, 64),
            item.Channel,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantBudgetView> ToEquityGrantBudgetViews(List<EquityGrantBudget> items)
    {
        var views = new List<EquityGrantBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantBudgetView(item));
        }
        return views;
    }

    public static double SumEquityGrantBudgetView(EquityGrantBudgetView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedRate;
    }
}
