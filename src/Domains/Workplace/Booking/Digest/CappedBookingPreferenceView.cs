namespace Acme.Workforce.Workplace.Booking.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped booking preference.</summary>
public sealed record CappedBookingPreferenceView(string Code, string LabelText, string BatchSize, string ApprovedText, string Unit, string Checksum, string Description, string Visible, string PriorityText, string Team, string StatusLabel)
{
    public static CappedBookingPreferenceView ToCappedBookingPreferenceView(CappedBookingPreference item)
    {
        return new CappedBookingPreferenceView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Unit,
            item.Checksum,
            item.Description,
            $"{item.Visible}",
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedBookingPreferenceView> ToCappedBookingPreferenceViews(List<CappedBookingPreference> items)
    {
        var views = new List<CappedBookingPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToCappedBookingPreferenceView(item));
        }
        return views;
    }

    public static double SumCappedBookingPreferenceView(CappedBookingPreferenceView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedPriority;
    }
}
