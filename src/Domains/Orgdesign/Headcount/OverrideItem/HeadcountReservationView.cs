namespace Acme.Workforce.Orgdesign.Headcount.OverrideItem;

using System.Globalization;

/// <summary>How screens and exports show a headcount reservation.</summary>
public sealed record HeadcountReservationView(string CodeText, string Title, string LeaveDays, string Limit, string FloorValue, string Pinned, string Attempts, string StatusLabel)
{
    public static HeadcountReservationView ToHeadcountReservationView(HeadcountReservation item)
    {
        return new HeadcountReservationView(
            item.Code,
            item.Title,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HeadcountReservationView> ToHeadcountReservationViews(List<HeadcountReservation> items)
    {
        var views = new List<HeadcountReservationView>();
        foreach (var item in items)
        {
            views.Add(ToHeadcountReservationView(item));
        }
        return views;
    }

    public static double SumHeadcountReservationView(HeadcountReservationView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedLimit + parsedFloorValue;
    }
}
