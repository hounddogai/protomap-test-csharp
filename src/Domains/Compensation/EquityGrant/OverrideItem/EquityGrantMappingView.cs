namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a equity grant mapping.</summary>
public sealed record EquityGrantMappingView(string Code, string Name, string Category, string ReviewerText, string BudgetText, string StatusLabel)
{
    public static EquityGrantMappingView ToEquityGrantMappingView(EquityGrantMapping item)
    {
        return new EquityGrantMappingView(
            item.Code,
            item.Name,
            Text.Truncate(item.Category, 80),
            item.Reviewer,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantMappingView> ToEquityGrantMappingViews(List<EquityGrantMapping> items)
    {
        var views = new List<EquityGrantMappingView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantMappingView(item));
        }
        return views;
    }

    public static double SumEquityGrantMappingView(EquityGrantMappingView view)
    {
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return parsedBudget;
    }
}
