namespace Acme.Workforce.Compensation.MeritCycle.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive merit cycle reservation.</summary>
public sealed record AdaptiveMeritCycleReservationView(string CodeText, string TitleText, string EndsAtText, string TimezoneText, string Skill, string Category, string StatusLabel)
{
    public static AdaptiveMeritCycleReservationView ToAdaptiveMeritCycleReservationView(AdaptiveMeritCycleReservation item)
    {
        return new AdaptiveMeritCycleReservationView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            item.Skill,
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveMeritCycleReservationView> ToAdaptiveMeritCycleReservationViews(List<AdaptiveMeritCycleReservation> items)
    {
        var views = new List<AdaptiveMeritCycleReservationView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveMeritCycleReservationView(item));
        }
        return views;
    }

    public static double SumAdaptiveMeritCycleReservationView(AdaptiveMeritCycleReservationView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt;
    }
}
