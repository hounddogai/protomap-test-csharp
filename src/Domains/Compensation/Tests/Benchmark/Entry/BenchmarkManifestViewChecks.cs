namespace Acme.Workforce.Compensation.Tests.Benchmark.Entry;

using Acme.Workforce.Compensation.Benchmark.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BenchmarkManifestViewChecks
{
    public static void BenchmarkManifestViewShowsTheStatusInCapitals()
    {
        var item = BenchmarkManifest.NewBenchmarkManifest("code-302", "label-773");
        var view = BenchmarkManifestView.ToBenchmarkManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BenchmarkManifestView.ToBenchmarkManifestViews(new List<BenchmarkManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BenchmarkManifestViewShowsTheStatusInCapitals();
    }
}
