namespace Acme.Workforce.Compensation.Benchmark.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a benchmark digest.</summary>
public sealed record BenchmarkDigestView(string Code, string Title, string CreatedAtText, string Discount, string TimezoneText, string Cost, string StatusLabel)
{
    public static BenchmarkDigestView ToBenchmarkDigestView(BenchmarkDigest item)
    {
        return new BenchmarkDigestView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 40),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 64),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BenchmarkDigestView> ToBenchmarkDigestViews(List<BenchmarkDigest> items)
    {
        var views = new List<BenchmarkDigestView>();
        foreach (var item in items)
        {
            views.Add(ToBenchmarkDigestView(item));
        }
        return views;
    }

    public static double SumBenchmarkDigestView(BenchmarkDigestView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + parsedDiscount + parsedCost;
    }
}
