namespace Acme.Workforce.Engagement.ActionPlan.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a action plan snapshot.</summary>
public sealed record ActionPlanSnapshotView(string Code, string Label, string BadgeText, string WindowDaysText, string Rating, string Locale, string Weight, string ExpiresAt, string VersionText, string StatusLabel)
{
    public static ActionPlanSnapshotView ToActionPlanSnapshotView(ActionPlanSnapshot item)
    {
        return new ActionPlanSnapshotView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Badge,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ActionPlanSnapshotView> ToActionPlanSnapshotViews(List<ActionPlanSnapshot> items)
    {
        var views = new List<ActionPlanSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToActionPlanSnapshotView(item));
        }
        return views;
    }

    public static double SumActionPlanSnapshotView(ActionPlanSnapshotView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedRating + parsedWeight;
    }
}
