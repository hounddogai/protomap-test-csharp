namespace Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive reorg snapshot.</summary>
public sealed record AdaptiveReorgSnapshotView(string CodeText, string Title, string Label, string RatioText, string BudgetText, string ReviewerText, string TeamText, string StatusLabel)
{
    public static AdaptiveReorgSnapshotView ToAdaptiveReorgSnapshotView(AdaptiveReorgSnapshot item)
    {
        return new AdaptiveReorgSnapshotView(
            item.Code,
            item.Title,
            Text.Truncate(item.Label, 80),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 120),
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveReorgSnapshotView> ToAdaptiveReorgSnapshotViews(List<AdaptiveReorgSnapshot> items)
    {
        var views = new List<AdaptiveReorgSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveReorgSnapshotView(item));
        }
        return views;
    }

    public static double SumAdaptiveReorgSnapshotView(AdaptiveReorgSnapshotView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return parsedRatio + parsedBudget;
    }
}
