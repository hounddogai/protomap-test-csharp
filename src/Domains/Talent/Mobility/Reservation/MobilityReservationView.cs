namespace Acme.Workforce.Talent.Mobility.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a mobility reservation.</summary>
public sealed record MobilityReservationView(string CodeText, string LabelText, string Country, string WindowDays, string BatchSize, string DiscountText, string Capacity, string Maximum, string Fte, string GradeText, string StatusLabel)
{
    public static MobilityReservationView ToMobilityReservationView(MobilityReservation item)
    {
        return new MobilityReservationView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Label, 80),
            item.Country,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilityReservationView> ToMobilityReservationViews(List<MobilityReservation> items)
    {
        var views = new List<MobilityReservationView>();
        foreach (var item in items)
        {
            views.Add(ToMobilityReservationView(item));
        }
        return views;
    }

    public static double SumMobilityReservationView(MobilityReservationView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedBatchSize + parsedDiscount;
    }
}
