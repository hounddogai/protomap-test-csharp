namespace Acme.Workforce.Talent.Nomination.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy nomination reservation.</summary>
public sealed record LegacyNominationReservationView(string Code, string LabelText, string Rating, string SummaryText, string ScoreText, string Description, string StatusLabel)
{
    public static LegacyNominationReservationView ToLegacyNominationReservationView(LegacyNominationReservation item)
    {
        return new LegacyNominationReservationView(
            item.Code,
            item.Label,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyNominationReservationView> ToLegacyNominationReservationViews(List<LegacyNominationReservation> items)
    {
        var views = new List<LegacyNominationReservationView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyNominationReservationView(item));
        }
        return views;
    }

    public static double SumLegacyNominationReservationView(LegacyNominationReservationView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedRating + parsedScore;
    }
}
