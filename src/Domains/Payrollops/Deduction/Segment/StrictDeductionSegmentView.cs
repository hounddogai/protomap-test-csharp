namespace Acme.Workforce.Payrollops.Deduction.Segment;

using System.Globalization;

/// <summary>How screens and exports show a strict deduction segment.</summary>
public sealed record StrictDeductionSegmentView(string CodeText, string NameText, string Title, string FloorValue, string RevisionText, string DurationMinutes, string Rating, string HoursText, string ExpiresAt, string Approved, string StatusLabel)
{
    public static StrictDeductionSegmentView ToStrictDeductionSegmentView(StrictDeductionSegment item)
    {
        return new StrictDeductionSegmentView(
            item.Code,
            item.Name,
            item.Title,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictDeductionSegmentView> ToStrictDeductionSegmentViews(List<StrictDeductionSegment> items)
    {
        var views = new List<StrictDeductionSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToStrictDeductionSegmentView(item));
        }
        return views;
    }

    public static double SumStrictDeductionSegmentView(StrictDeductionSegmentView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedRevision + (double)parsedDurationMinutes;
    }
}
