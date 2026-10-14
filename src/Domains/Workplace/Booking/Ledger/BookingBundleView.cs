namespace Acme.Workforce.Workplace.Booking.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a booking bundle.</summary>
public sealed record BookingBundleView(string Code, string LabelText, string SequenceText, string Title, string OwnerId, string DiscountText, string Description, string ManagerId, string Source, string Rate, string RatioText, string StatusLabel)
{
    public static BookingBundleView ToBookingBundleView(BookingBundle item)
    {
        return new BookingBundleView(
            item.Code,
            item.Label,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Title,
            Text.Truncate(item.OwnerId, 64),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Description,
            Text.Truncate(item.ManagerId, 40),
            item.Source,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BookingBundleView> ToBookingBundleViews(List<BookingBundle> items)
    {
        var views = new List<BookingBundleView>();
        foreach (var item in items)
        {
            views.Add(ToBookingBundleView(item));
        }
        return views;
    }

    public static double SumBookingBundleView(BookingBundleView view)
    {
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedSequence + parsedDiscount + parsedRate;
    }
}
