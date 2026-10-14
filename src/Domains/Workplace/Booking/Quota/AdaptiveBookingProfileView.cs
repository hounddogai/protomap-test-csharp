namespace Acme.Workforce.Workplace.Booking.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive booking profile.</summary>
public sealed record AdaptiveBookingProfileView(string CodeText, string LabelText, string JobCodeText, string EndsAtText, string Active, string ManagerId, string Pinned, string Grade, string StatusLabel)
{
    public static AdaptiveBookingProfileView ToAdaptiveBookingProfileView(AdaptiveBookingProfile item)
    {
        return new AdaptiveBookingProfileView(
            item.Code,
            item.Label,
            item.JobCode,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.ManagerId,
            $"{item.Pinned}",
            Text.Truncate(item.Grade, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveBookingProfileView> ToAdaptiveBookingProfileViews(List<AdaptiveBookingProfile> items)
    {
        var views = new List<AdaptiveBookingProfileView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveBookingProfileView(item));
        }
        return views;
    }

    public static double SumAdaptiveBookingProfileView(AdaptiveBookingProfileView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt;
    }
}
