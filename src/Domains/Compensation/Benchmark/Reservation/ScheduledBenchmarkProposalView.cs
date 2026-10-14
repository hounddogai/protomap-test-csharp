namespace Acme.Workforce.Compensation.Benchmark.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled benchmark proposal.</summary>
public sealed record ScheduledBenchmarkProposalView(string Code, string Title, string Maximum, string Threshold, string Revision, string RegionText, string StatusLabel)
{
    public static ScheduledBenchmarkProposalView ToScheduledBenchmarkProposalView(ScheduledBenchmarkProposal item)
    {
        return new ScheduledBenchmarkProposalView(
            item.Code,
            item.Title,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledBenchmarkProposalView> ToScheduledBenchmarkProposalViews(List<ScheduledBenchmarkProposal> items)
    {
        var views = new List<ScheduledBenchmarkProposalView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledBenchmarkProposalView(item));
        }
        return views;
    }

    public static double SumScheduledBenchmarkProposalView(ScheduledBenchmarkProposalView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedThreshold + (double)parsedRevision;
    }
}
