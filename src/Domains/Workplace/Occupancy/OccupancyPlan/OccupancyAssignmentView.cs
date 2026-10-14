namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a occupancy assignment.</summary>
public sealed record OccupancyAssignmentView(string CodeText, string Name, string NotesText, string UnitText, string ReviewerText, string DurationMinutesText, string JobCodeText, string Retries, string StatusLabel)
{
    public static OccupancyAssignmentView ToOccupancyAssignmentView(OccupancyAssignment item)
    {
        return new OccupancyAssignmentView(
            item.Code,
            item.Name,
            item.Notes,
            Text.Truncate(item.Unit, 64),
            item.Reviewer,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OccupancyAssignmentView> ToOccupancyAssignmentViews(List<OccupancyAssignment> items)
    {
        var views = new List<OccupancyAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToOccupancyAssignmentView(item));
        }
        return views;
    }

    public static double SumOccupancyAssignmentView(OccupancyAssignmentView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedRetries;
    }
}
