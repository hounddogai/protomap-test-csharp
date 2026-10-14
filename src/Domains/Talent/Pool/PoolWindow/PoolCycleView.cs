namespace Acme.Workforce.Talent.Pool.PoolWindow;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pool cycle.</summary>
public sealed record PoolCycleView(string Code, string Title, string Team, string Category, string Skill, string ExternalRef, string Retries, string BudgetText, string Country, string StatusLabel)
{
    public static PoolCycleView ToPoolCycleView(PoolCycle item)
    {
        return new PoolCycleView(
            item.Code,
            item.Title,
            item.Team,
            item.Category,
            Text.Truncate(item.Skill, 120),
            Text.Truncate(item.ExternalRef, 120),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolCycleView> ToPoolCycleViews(List<PoolCycle> items)
    {
        var views = new List<PoolCycleView>();
        foreach (var item in items)
        {
            views.Add(ToPoolCycleView(item));
        }
        return views;
    }

    public static double SumPoolCycleView(PoolCycleView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedBudget;
    }
}
