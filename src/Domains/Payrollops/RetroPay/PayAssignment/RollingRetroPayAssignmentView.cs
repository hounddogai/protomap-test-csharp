namespace Acme.Workforce.Payrollops.RetroPay.PayAssignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling retro pay assignment.</summary>
public sealed record RollingRetroPayAssignmentView(string CodeText, string Title, string Location, string Revision, string DurationMinutes, string Locale, string StatusLabel)
{
    public static RollingRetroPayAssignmentView ToRollingRetroPayAssignmentView(RollingRetroPayAssignment item)
    {
        return new RollingRetroPayAssignmentView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Location,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingRetroPayAssignmentView> ToRollingRetroPayAssignmentViews(List<RollingRetroPayAssignment> items)
    {
        var views = new List<RollingRetroPayAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToRollingRetroPayAssignmentView(item));
        }
        return views;
    }

    public static double SumRollingRetroPayAssignmentView(RollingRetroPayAssignmentView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedDurationMinutes;
    }
}
