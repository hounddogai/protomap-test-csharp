namespace Acme.Workforce.Compensation.Benchmark.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a benchmark window.</summary>
public sealed record BenchmarkWindowView(string Code, string Name, string Version, string Active, string TitleText, string Currency, string Rate, string FteText, string StatusLabel)
{
    public static BenchmarkWindowView ToBenchmarkWindowView(BenchmarkWindow item)
    {
        return new BenchmarkWindowView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Name, 80),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Title,
            item.Currency,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BenchmarkWindowView> ToBenchmarkWindowViews(List<BenchmarkWindow> items)
    {
        var views = new List<BenchmarkWindowView>();
        foreach (var item in items)
        {
            views.Add(ToBenchmarkWindowView(item));
        }
        return views;
    }

    public static double SumBenchmarkWindowView(BenchmarkWindowView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedRate + parsedFte;
    }
}
