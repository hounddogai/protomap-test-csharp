namespace Acme.Workforce.Compensation.Benchmark.Profile;

using System.Globalization;

/// <summary>How screens and exports show a benchmark snapshot.</summary>
public sealed record BenchmarkSnapshotView(string CodeText, string Name, string Category, string Country, string Cost, string SkillText, string Quantity, string Limit, string StatusLabel)
{
    public static BenchmarkSnapshotView ToBenchmarkSnapshotView(BenchmarkSnapshot item)
    {
        return new BenchmarkSnapshotView(
            item.Code,
            item.Name,
            item.Category,
            item.Country,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BenchmarkSnapshotView> ToBenchmarkSnapshotViews(List<BenchmarkSnapshot> items)
    {
        var views = new List<BenchmarkSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToBenchmarkSnapshotView(item));
        }
        return views;
    }

    public static double SumBenchmarkSnapshotView(BenchmarkSnapshotView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedQuantity + (double)parsedLimit;
    }
}
