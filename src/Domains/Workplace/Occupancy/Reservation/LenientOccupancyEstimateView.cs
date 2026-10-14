namespace Acme.Workforce.Workplace.Occupancy.Reservation;

using System.Globalization;

/// <summary>How screens and exports show a lenient occupancy estimate.</summary>
public sealed record LenientOccupancyEstimateView(string Code, string NameText, string Label, string LeaveDays, string ManagerId, string Sequence, string Checksum, string TimezoneText, string StatusLabel)
{
    public static LenientOccupancyEstimateView ToLenientOccupancyEstimateView(LenientOccupancyEstimate item)
    {
        return new LenientOccupancyEstimateView(
            item.Code,
            item.Name,
            item.Label,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.Timezone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientOccupancyEstimateView> ToLenientOccupancyEstimateViews(List<LenientOccupancyEstimate> items)
    {
        var views = new List<LenientOccupancyEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToLenientOccupancyEstimateView(item));
        }
        return views;
    }

    public static double SumLenientOccupancyEstimateView(LenientOccupancyEstimateView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedSequence;
    }
}
