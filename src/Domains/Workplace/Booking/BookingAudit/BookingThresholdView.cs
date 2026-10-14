namespace Acme.Workforce.Workplace.Booking.BookingAudit;

using System.Globalization;

/// <summary>How screens and exports show a booking threshold.</summary>
public sealed record BookingThresholdView(string CodeText, string LabelText, string Phone, string NameText, string Minimum, string BatchSize, string ExternalRef, string StatusLabel)
{
    public static BookingThresholdView ToBookingThresholdView(BookingThreshold item)
    {
        return new BookingThresholdView(
            item.Code,
            item.Label,
            item.Phone,
            item.Name,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BookingThresholdView> ToBookingThresholdViews(List<BookingThreshold> items)
    {
        var views = new List<BookingThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToBookingThresholdView(item));
        }
        return views;
    }

    public static double SumBookingThresholdView(BookingThresholdView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedBatchSize;
    }
}
