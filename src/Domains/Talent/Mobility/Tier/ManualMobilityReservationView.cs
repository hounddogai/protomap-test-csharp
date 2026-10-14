namespace Acme.Workforce.Talent.Mobility.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual mobility reservation.</summary>
public sealed record ManualMobilityReservationView(string Code, string Title, string Grade, string MaximumText, string SegmentText, string Currency, string MarginText, string Discount, string ManagerId, string StatusLabel)
{
    public static ManualMobilityReservationView ToManualMobilityReservationView(ManualMobilityReservation item)
    {
        return new ManualMobilityReservationView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 64),
            item.Grade,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 80),
            Text.Truncate(item.Currency, 120),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualMobilityReservationView> ToManualMobilityReservationViews(List<ManualMobilityReservation> items)
    {
        var views = new List<ManualMobilityReservationView>();
        foreach (var item in items)
        {
            views.Add(ToManualMobilityReservationView(item));
        }
        return views;
    }

    public static double SumManualMobilityReservationView(ManualMobilityReservationView view)
    {
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedMargin + parsedDiscount;
    }
}
