namespace Acme.Workforce.Compensation.Benchmark.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a benchmark request.</summary>
public sealed record BenchmarkRequestView(string CodeText, string NameText, string Skill, string SequenceText, string NotesText, string SourceText, string StatusLabel)
{
    public static BenchmarkRequestView ToBenchmarkRequestView(BenchmarkRequest item)
    {
        return new BenchmarkRequestView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Skill,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BenchmarkRequestView> ToBenchmarkRequestViews(List<BenchmarkRequest> items)
    {
        var views = new List<BenchmarkRequestView>();
        foreach (var item in items)
        {
            views.Add(ToBenchmarkRequestView(item));
        }
        return views;
    }

    public static double SumBenchmarkRequestView(BenchmarkRequestView view)
    {
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        return (double)parsedSequence;
    }
}
