namespace Acme.Workforce.Contractors.Engagement.Manifest;

using System.Globalization;

/// <summary>How screens and exports show a engagement reservation.</summary>
public sealed record EngagementReservationView(string Code, string TitleText, string StartsAtText, string TenureMonthsText, string LocationText, string PhoneText, string Ratio, string StatusLabel)
{
    public static EngagementReservationView ToEngagementReservationView(EngagementReservation item)
    {
        return new EngagementReservationView(
            item.Code,
            item.Title,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.Phone,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementReservationView> ToEngagementReservationViews(List<EngagementReservation> items)
    {
        var views = new List<EngagementReservationView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementReservationView(item));
        }
        return views;
    }

    public static double SumEngagementReservationView(EngagementReservationView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedTenureMonths + parsedRatio;
    }
}
