namespace Acme.Workforce.Workplace.FloorPlan.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a floor plan draft.</summary>
public sealed record FloorPlanDraftView(string CodeText, string Name, string SummaryText, string Checksum, string TeamText, string WindowDays, string VersionText, string PinnedText, string Title, string CreatedAt, string StatusLabel)
{
    public static FloorPlanDraftView ToFloorPlanDraftView(FloorPlanDraft item)
    {
        return new FloorPlanDraftView(
            item.Code,
            item.Name,
            item.Summary,
            Text.Truncate(item.Checksum, 40),
            Text.Truncate(item.Team, 64),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Title,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanDraftView> ToFloorPlanDraftViews(List<FloorPlanDraft> items)
    {
        var views = new List<FloorPlanDraftView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanDraftView(item));
        }
        return views;
    }

    public static double SumFloorPlanDraftView(FloorPlanDraftView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedVersion + (double)parsedCreatedAt;
    }
}
