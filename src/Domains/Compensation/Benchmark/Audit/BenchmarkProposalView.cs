namespace Acme.Workforce.Compensation.Benchmark.Audit;

using System.Globalization;

/// <summary>How screens and exports show a benchmark proposal.</summary>
public sealed record BenchmarkProposalView(string CodeText, string Title, string Phone, string Rating, string Locale, string StatusLabel)
{
    public static BenchmarkProposalView ToBenchmarkProposalView(BenchmarkProposal item)
    {
        return new BenchmarkProposalView(
            item.Code,
            item.Title,
            item.Phone,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BenchmarkProposalView> ToBenchmarkProposalViews(List<BenchmarkProposal> items)
    {
        var views = new List<BenchmarkProposalView>();
        foreach (var item in items)
        {
            views.Add(ToBenchmarkProposalView(item));
        }
        return views;
    }

    public static double SumBenchmarkProposalView(BenchmarkProposalView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedRating;
    }
}
