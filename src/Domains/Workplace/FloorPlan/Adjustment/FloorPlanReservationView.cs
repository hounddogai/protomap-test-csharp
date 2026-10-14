namespace Acme.Workforce.Workplace.FloorPlan.Adjustment;

using System.Globalization;

/// <summary>How screens and exports show a floor plan reservation.</summary>
public sealed record FloorPlanReservationView(string CodeText, string Label, string ContactEmail, string ExpiresAt, string Description, string StatusLabel)
{
    public static FloorPlanReservationView ToFloorPlanReservationView(FloorPlanReservation item)
    {
        return new FloorPlanReservationView(
            item.Code,
            item.Label,
            item.ContactEmail,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Description,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanReservationView> ToFloorPlanReservationViews(List<FloorPlanReservation> items)
    {
        var views = new List<FloorPlanReservationView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanReservationView(item));
        }
        return views;
    }

    public static double SumFloorPlanReservationView(FloorPlanReservationView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt;
    }
}
