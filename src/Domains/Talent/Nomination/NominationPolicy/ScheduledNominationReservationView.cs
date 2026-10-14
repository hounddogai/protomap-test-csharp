namespace Acme.Workforce.Talent.Nomination.NominationPolicy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled nomination reservation.</summary>
public sealed record ScheduledNominationReservationView(string Code, string TitleText, string Fte, string WindowDaysText, string CategoryText, string Margin, string StatusLabel)
{
    public static ScheduledNominationReservationView ToScheduledNominationReservationView(ScheduledNominationReservation item)
    {
        return new ScheduledNominationReservationView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 40),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledNominationReservationView> ToScheduledNominationReservationViews(List<ScheduledNominationReservation> items)
    {
        var views = new List<ScheduledNominationReservationView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledNominationReservationView(item));
        }
        return views;
    }

    public static double SumScheduledNominationReservationView(ScheduledNominationReservationView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedWindowDays + parsedMargin;
    }
}
