namespace Acme.Workforce.Workplace.Booking.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a booking record.</summary>
public sealed record BookingRecordView(string Code, string Label, string SummaryText, string Segment, string MarginText, string VersionText, string Weight, string WindowDays, string SalaryBand, string StatusLabel)
{
    public static BookingRecordView ToBookingRecordView(BookingRecord item)
    {
        return new BookingRecordView(
            item.Code,
            item.Label,
            item.Summary,
            item.Segment,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BookingRecordView> ToBookingRecordViews(List<BookingRecord> items)
    {
        var views = new List<BookingRecordView>();
        foreach (var item in items)
        {
            views.Add(ToBookingRecordView(item));
        }
        return views;
    }

    public static double SumBookingRecordView(BookingRecordView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedVersion + parsedWeight;
    }
}
