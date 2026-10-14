namespace Acme.Workforce.Assets.Assignment.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict assignment score.</summary>
public sealed record StrictAssignmentScoreView(string Code, string Name, string Notes, string Approved, string FloorValueText, string Revision, string OwnerId, string Title, string Version, string Location, string StatusLabel)
{
    public static StrictAssignmentScoreView ToStrictAssignmentScoreView(StrictAssignmentScore item)
    {
        return new StrictAssignmentScoreView(
            item.Code,
            item.Name,
            item.Notes,
            $"{item.Approved}",
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            Text.Truncate(item.Title, 40),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictAssignmentScoreView> ToStrictAssignmentScoreViews(List<StrictAssignmentScore> items)
    {
        var views = new List<StrictAssignmentScoreView>();
        foreach (var item in items)
        {
            views.Add(ToStrictAssignmentScoreView(item));
        }
        return views;
    }

    public static double SumStrictAssignmentScoreView(StrictAssignmentScoreView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedRevision + (double)parsedVersion;
    }
}
