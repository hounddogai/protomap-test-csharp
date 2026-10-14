namespace Acme.Workforce.Talent.Mobility.MobilityReservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending mobility reservation.</summary>
public sealed record PendingMobilityReservationView(string CodeText, string Title, string ExternalRefText, string SkillText, string Notes, string DepartmentText, string Threshold, string VisibleText, string StatusLabel)
{
    public static PendingMobilityReservationView ToPendingMobilityReservationView(PendingMobilityReservation item)
    {
        return new PendingMobilityReservationView(
            item.Code,
            item.Title,
            item.ExternalRef,
            item.Skill,
            Text.Truncate(item.Notes, 80),
            item.Department,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingMobilityReservationView> ToPendingMobilityReservationViews(List<PendingMobilityReservation> items)
    {
        var views = new List<PendingMobilityReservationView>();
        foreach (var item in items)
        {
            views.Add(ToPendingMobilityReservationView(item));
        }
        return views;
    }

    public static double SumPendingMobilityReservationView(PendingMobilityReservationView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedThreshold;
    }
}
