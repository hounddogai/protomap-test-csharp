namespace Acme.Workforce.Compensation.Benchmark.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a benchmark score.</summary>
public sealed record BenchmarkScoreView(string CodeText, string LabelText, string Revision, string Grade, string OwnerId, string Visible, string Pinned, string Source, string StatusLabel)
{
    public static BenchmarkScoreView ToBenchmarkScoreView(BenchmarkScore item)
    {
        return new BenchmarkScoreView(
            item.Code,
            item.Label,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.OwnerId,
            $"{item.Visible}",
            $"{item.Pinned}",
            Text.Truncate(item.Source, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BenchmarkScoreView> ToBenchmarkScoreViews(List<BenchmarkScore> items)
    {
        var views = new List<BenchmarkScoreView>();
        foreach (var item in items)
        {
            views.Add(ToBenchmarkScoreView(item));
        }
        return views;
    }

    public static double SumBenchmarkScoreView(BenchmarkScoreView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedRevision;
    }
}
