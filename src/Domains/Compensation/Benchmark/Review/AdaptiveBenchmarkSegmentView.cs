namespace Acme.Workforce.Compensation.Benchmark.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive benchmark segment.</summary>
public sealed record AdaptiveBenchmarkSegmentView(string Code, string Name, string UpdatedAt, string Reviewer, string VisibleText, string Grade, string StatusLabel)
{
    public static AdaptiveBenchmarkSegmentView ToAdaptiveBenchmarkSegmentView(AdaptiveBenchmarkSegment item)
    {
        return new AdaptiveBenchmarkSegmentView(
            item.Code,
            item.Name,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            $"{item.Visible}",
            Text.Truncate(item.Grade, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveBenchmarkSegmentView> ToAdaptiveBenchmarkSegmentViews(List<AdaptiveBenchmarkSegment> items)
    {
        var views = new List<AdaptiveBenchmarkSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveBenchmarkSegmentView(item));
        }
        return views;
    }

    public static double SumAdaptiveBenchmarkSegmentView(AdaptiveBenchmarkSegmentView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt;
    }
}
