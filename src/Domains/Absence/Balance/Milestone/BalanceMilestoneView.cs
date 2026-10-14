namespace Acme.Workforce.Absence.Balance.Milestone;

using System.Globalization;

/// <summary>How screens and exports show a balance milestone.</summary>
public sealed record BalanceMilestoneView(string Code, string Label, string PinnedText, string Minimum, string Ratio, string StatusLabel)
{
    public static BalanceMilestoneView ToBalanceMilestoneView(BalanceMilestone item)
    {
        return new BalanceMilestoneView(
            item.Code,
            item.Label,
            $"{item.Pinned}",
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalanceMilestoneView> ToBalanceMilestoneViews(List<BalanceMilestone> items)
    {
        var views = new List<BalanceMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToBalanceMilestoneView(item));
        }
        return views;
    }

    public static double SumBalanceMilestoneView(BalanceMilestoneView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedRatio;
    }
}
