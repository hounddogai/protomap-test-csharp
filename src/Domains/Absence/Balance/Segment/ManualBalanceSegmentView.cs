namespace Acme.Workforce.Absence.Balance.Segment;

using System.Globalization;

/// <summary>How screens and exports show a manual balance segment.</summary>
public sealed record ManualBalanceSegmentView(string Code, string Title, string BatchSize, string ManagerId, string Country, string Capacity, string RatingText, string Amount, string StatusLabel)
{
    public static ManualBalanceSegmentView ToManualBalanceSegmentView(ManualBalanceSegment item)
    {
        return new ManualBalanceSegmentView(
            item.Code,
            item.Title,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Country,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualBalanceSegmentView> ToManualBalanceSegmentViews(List<ManualBalanceSegment> items)
    {
        var views = new List<ManualBalanceSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToManualBalanceSegmentView(item));
        }
        return views;
    }

    public static double SumManualBalanceSegmentView(ManualBalanceSegmentView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedCapacity + parsedRating;
    }
}
