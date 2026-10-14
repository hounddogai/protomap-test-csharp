namespace Acme.Workforce.Compensation.Benchmark.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a benchmark manifest.</summary>
public sealed record BenchmarkManifestView(string Code, string Label, string FloorValueText, string Grade, string VisibleText, string MaximumText, string ExternalRef, string StatusLabel)
{
    public static BenchmarkManifestView ToBenchmarkManifestView(BenchmarkManifest item)
    {
        return new BenchmarkManifestView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            $"{item.Visible}",
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ExternalRef, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BenchmarkManifestView> ToBenchmarkManifestViews(List<BenchmarkManifest> items)
    {
        var views = new List<BenchmarkManifestView>();
        foreach (var item in items)
        {
            views.Add(ToBenchmarkManifestView(item));
        }
        return views;
    }

    public static double SumBenchmarkManifestView(BenchmarkManifestView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedMaximum;
    }
}
