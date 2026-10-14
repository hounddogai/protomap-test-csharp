namespace Acme.Workforce.Contractors.Engagement.Reservation;

using System.Globalization;

/// <summary>How screens and exports show a weighted engagement reservation.</summary>
public sealed record WeightedEngagementReservationView(string Code, string Label, string Approved, string Minimum, string Rating, string Country, string Active, string ContactEmail, string SequenceText, string LeaveDays, string StatusLabel)
{
    public static WeightedEngagementReservationView ToWeightedEngagementReservationView(WeightedEngagementReservation item)
    {
        return new WeightedEngagementReservationView(
            item.Code,
            item.Label,
            $"{item.Approved}",
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            $"{item.Active}",
            item.ContactEmail,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedEngagementReservationView> ToWeightedEngagementReservationViews(List<WeightedEngagementReservation> items)
    {
        var views = new List<WeightedEngagementReservationView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedEngagementReservationView(item));
        }
        return views;
    }

    public static double SumWeightedEngagementReservationView(WeightedEngagementReservationView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedRating + (double)parsedSequence;
    }
}
