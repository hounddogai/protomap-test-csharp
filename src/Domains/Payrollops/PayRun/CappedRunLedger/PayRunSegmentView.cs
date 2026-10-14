namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay run segment.</summary>
public sealed record PayRunSegmentView(string Code, string Label, string Attempts, string OwnerId, string Region, string RatingText, string Team, string Locale, string Revision, string StatusLabel)
{
    public static PayRunSegmentView ToPayRunSegmentView(PayRunSegment item)
    {
        return new PayRunSegmentView(
            item.Code,
            item.Label,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Region,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 80),
            item.Locale,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayRunSegmentView> ToPayRunSegmentViews(List<PayRunSegment> items)
    {
        var views = new List<PayRunSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToPayRunSegmentView(item));
        }
        return views;
    }

    public static double SumPayRunSegmentView(PayRunSegmentView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedRating + (double)parsedRevision;
    }
}
