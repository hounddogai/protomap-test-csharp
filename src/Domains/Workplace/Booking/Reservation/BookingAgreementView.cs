namespace Acme.Workforce.Workplace.Booking.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a booking agreement.</summary>
public sealed record BookingAgreementView(string Code, string Name, string CountryText, string CategoryText, string Notes, string WindowDays, string Currency, string TenureMonths, string CostText, string StatusLabel)
{
    public static BookingAgreementView ToBookingAgreementView(BookingAgreement item)
    {
        return new BookingAgreementView(
            item.Code,
            item.Name,
            item.Country,
            item.Category,
            Text.Truncate(item.Notes, 64),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BookingAgreementView> ToBookingAgreementViews(List<BookingAgreement> items)
    {
        var views = new List<BookingAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToBookingAgreementView(item));
        }
        return views;
    }

    public static double SumBookingAgreementView(BookingAgreementView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedTenureMonths + parsedCost;
    }
}
