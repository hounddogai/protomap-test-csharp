namespace Acme.Workforce.Workplace.Booking.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered booking record.</summary>
public sealed record TieredBookingRecordView(string Code, string Name, string Rate, string Locale, string Weight, string Reviewer, string Sequence, string Grade, string Pinned, string ContactEmail, string Notes, string StatusLabel)
{
    public static TieredBookingRecordView ToTieredBookingRecordView(TieredBookingRecord item)
    {
        return new TieredBookingRecordView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Name, 80),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 64),
            $"{item.Pinned}",
            Text.Truncate(item.ContactEmail, 120),
            Text.Truncate(item.Notes, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredBookingRecordView> ToTieredBookingRecordViews(List<TieredBookingRecord> items)
    {
        var views = new List<TieredBookingRecordView>();
        foreach (var item in items)
        {
            views.Add(ToTieredBookingRecordView(item));
        }
        return views;
    }

    public static double SumTieredBookingRecordView(TieredBookingRecordView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return parsedRate + parsedWeight + (double)parsedSequence;
    }
}
