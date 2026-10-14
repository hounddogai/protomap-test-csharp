namespace Acme.Workforce.Workplace.Occupancy.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a occupancy draft.</summary>
public sealed record OccupancyDraftView(string Code, string Label, string Score, string Name, string Summary, string ManagerId, string LeaveDays, string Weight, string Source, string Budget, string StatusLabel)
{
    public static OccupancyDraftView ToOccupancyDraftView(OccupancyDraft item)
    {
        return new OccupancyDraftView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            item.Summary,
            Text.Truncate(item.ManagerId, 120),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Source,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OccupancyDraftView> ToOccupancyDraftViews(List<OccupancyDraft> items)
    {
        var views = new List<OccupancyDraftView>();
        foreach (var item in items)
        {
            views.Add(ToOccupancyDraftView(item));
        }
        return views;
    }

    public static double SumOccupancyDraftView(OccupancyDraftView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedLeaveDays + parsedWeight;
    }
}
