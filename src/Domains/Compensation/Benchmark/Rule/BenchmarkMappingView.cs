namespace Acme.Workforce.Compensation.Benchmark.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a benchmark mapping.</summary>
public sealed record BenchmarkMappingView(string Code, string Name, string NotesText, string Maximum, string WeightText, string Title, string SalaryBand, string Rate, string Budget, string ManagerId, string EndsAt, string StatusLabel)
{
    public static BenchmarkMappingView ToBenchmarkMappingView(BenchmarkMapping item)
    {
        return new BenchmarkMappingView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Notes,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Title, 64),
            item.SalaryBand,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BenchmarkMappingView> ToBenchmarkMappingViews(List<BenchmarkMapping> items)
    {
        var views = new List<BenchmarkMappingView>();
        foreach (var item in items)
        {
            views.Add(ToBenchmarkMappingView(item));
        }
        return views;
    }

    public static double SumBenchmarkMappingView(BenchmarkMappingView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedWeight + parsedRate;
    }
}
