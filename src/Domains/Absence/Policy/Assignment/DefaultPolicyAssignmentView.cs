namespace Acme.Workforce.Absence.Policy.Assignment;

using System.Globalization;

/// <summary>How screens and exports show a default policy assignment.</summary>
public sealed record DefaultPolicyAssignmentView(string Code, string NameText, string EmployeeId, string LeaveDaysText, string Revision, string Cost, string Archived, string VersionText, string ActiveText, string TitleText, string Approved, string StatusLabel)
{
    public static DefaultPolicyAssignmentView ToDefaultPolicyAssignmentView(DefaultPolicyAssignment item)
    {
        return new DefaultPolicyAssignmentView(
            item.Code,
            item.Name,
            item.EmployeeId,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Title,
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultPolicyAssignmentView> ToDefaultPolicyAssignmentViews(List<DefaultPolicyAssignment> items)
    {
        var views = new List<DefaultPolicyAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultPolicyAssignmentView(item));
        }
        return views;
    }

    public static double SumDefaultPolicyAssignmentView(DefaultPolicyAssignmentView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedRevision + parsedCost;
    }
}
