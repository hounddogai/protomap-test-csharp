namespace Acme.Workforce.Talent.SuccessionPlan.Allocation;

using System.Globalization;

/// <summary>How screens and exports show a succession plan reservation.</summary>
public sealed record SuccessionPlanReservationView(string Code, string Name, string Maximum, string Team, string ContactEmailText, string StatusLabel)
{
    public static SuccessionPlanReservationView ToSuccessionPlanReservationView(SuccessionPlanReservation item)
    {
        return new SuccessionPlanReservationView(
            item.Code,
            item.Name,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SuccessionPlanReservationView> ToSuccessionPlanReservationViews(List<SuccessionPlanReservation> items)
    {
        var views = new List<SuccessionPlanReservationView>();
        foreach (var item in items)
        {
            views.Add(ToSuccessionPlanReservationView(item));
        }
        return views;
    }

    public static double SumSuccessionPlanReservationView(SuccessionPlanReservationView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedMaximum;
    }
}
