namespace Acme.Workforce.Workplace.Occupancy.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a occupancy batch.</summary>
public sealed record OccupancyBatchView(string Code, string LabelText, string Score, string Department, string RevisionText, string Region, string StartsAt, string StatusLabel)
{
    public static OccupancyBatchView ToOccupancyBatchView(OccupancyBatch item)
    {
        return new OccupancyBatchView(
            item.Code,
            item.Label,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 64),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Region,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OccupancyBatchView> ToOccupancyBatchViews(List<OccupancyBatch> items)
    {
        var views = new List<OccupancyBatchView>();
        foreach (var item in items)
        {
            views.Add(ToOccupancyBatchView(item));
        }
        return views;
    }

    public static double SumOccupancyBatchView(OccupancyBatchView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedRevision + (double)parsedStartsAt;
    }
}
