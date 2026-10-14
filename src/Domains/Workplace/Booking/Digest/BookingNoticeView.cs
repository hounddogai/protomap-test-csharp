namespace Acme.Workforce.Workplace.Booking.Digest;

using System.Globalization;

/// <summary>How screens and exports show a booking notice.</summary>
public sealed record BookingNoticeView(string Code, string Title, string Segment, string Rating, string NotesText, string MarginText, string Grade, string Hours, string RatioText, string FloorValueText, string StatusLabel)
{
    public static BookingNoticeView ToBookingNoticeView(BookingNotice item)
    {
        return new BookingNoticeView(
            item.Code,
            item.Title,
            item.Segment,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BookingNoticeView> ToBookingNoticeViews(List<BookingNotice> items)
    {
        var views = new List<BookingNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToBookingNoticeView(item));
        }
        return views;
    }

    public static double SumBookingNoticeView(BookingNoticeView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return parsedRating + parsedMargin + parsedHours;
    }
}
