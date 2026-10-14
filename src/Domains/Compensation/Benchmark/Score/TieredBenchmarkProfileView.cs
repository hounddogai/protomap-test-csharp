namespace Acme.Workforce.Compensation.Benchmark.Score;

using System.Globalization;

/// <summary>How screens and exports show a tiered benchmark profile.</summary>
public sealed record TieredBenchmarkProfileView(string Code, string NameText, string CreatedAt, string WindowDays, string OwnerIdText, string Region, string StatusLabel)
{
    public static TieredBenchmarkProfileView ToTieredBenchmarkProfileView(TieredBenchmarkProfile item)
    {
        return new TieredBenchmarkProfileView(
            item.Code,
            item.Name,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredBenchmarkProfileView> ToTieredBenchmarkProfileViews(List<TieredBenchmarkProfile> items)
    {
        var views = new List<TieredBenchmarkProfileView>();
        foreach (var item in items)
        {
            views.Add(ToTieredBenchmarkProfileView(item));
        }
        return views;
    }

    public static double SumTieredBenchmarkProfileView(TieredBenchmarkProfileView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedWindowDays;
    }
}
