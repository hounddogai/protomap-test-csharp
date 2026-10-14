namespace Acme.Workforce.Contractors.Rate.Milestone;

using System.Globalization;

/// <summary>How screens and exports show a fallback rate milestone.</summary>
public sealed record FallbackRateMilestoneView(string Code, string Name, string RevisionText, string Margin, string StatusLabel)
{
    public static FallbackRateMilestoneView ToFallbackRateMilestoneView(FallbackRateMilestone item)
    {
        return new FallbackRateMilestoneView(
            item.Code,
            item.Name,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackRateMilestoneView> ToFallbackRateMilestoneViews(List<FallbackRateMilestone> items)
    {
        var views = new List<FallbackRateMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackRateMilestoneView(item));
        }
        return views;
    }

    public static double SumFallbackRateMilestoneView(FallbackRateMilestoneView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedMargin;
    }
}
