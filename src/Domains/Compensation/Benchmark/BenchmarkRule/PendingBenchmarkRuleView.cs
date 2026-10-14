namespace Acme.Workforce.Compensation.Benchmark.BenchmarkRule;

using System.Globalization;

/// <summary>How screens and exports show a pending benchmark rule.</summary>
public sealed record PendingBenchmarkRuleView(string Code, string Name, string CreatedAt, string GraceDays, string Score, string Fte, string Cost, string Channel, string BudgetText, string Margin, string StatusLabel)
{
    public static PendingBenchmarkRuleView ToPendingBenchmarkRuleView(PendingBenchmarkRule item)
    {
        return new PendingBenchmarkRuleView(
            item.Code,
            item.Name,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Channel,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingBenchmarkRuleView> ToPendingBenchmarkRuleViews(List<PendingBenchmarkRule> items)
    {
        var views = new List<PendingBenchmarkRuleView>();
        foreach (var item in items)
        {
            views.Add(ToPendingBenchmarkRuleView(item));
        }
        return views;
    }

    public static double SumPendingBenchmarkRuleView(PendingBenchmarkRuleView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedGraceDays + parsedScore;
    }
}
