namespace Acme.Workforce.Compensation.Tests.Benchmark.Summary;

using Acme.Workforce.Compensation.Benchmark.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BenchmarkReviewViewChecks
{
    public static void BenchmarkReviewViewShowsTheStatusInCapitals()
    {
        var item = BenchmarkReview.NewBenchmarkReview("code-397", "label-744");
        var view = BenchmarkReviewView.ToBenchmarkReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BenchmarkReviewView.ToBenchmarkReviewViews(new List<BenchmarkReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BenchmarkReviewViewShowsTheStatusInCapitals();
    }
}
