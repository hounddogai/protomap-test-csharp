namespace Acme.Workforce.Compliance.Certification.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a certification assignment.</summary>
public sealed record CertificationAssignmentView(string Code, string Label, string DepartmentText, string DurationMinutes, string HoursText, string StatusLabel)
{
    public static CertificationAssignmentView ToCertificationAssignmentView(CertificationAssignment item)
    {
        return new CertificationAssignmentView(
            item.Code,
            item.Label,
            Text.Truncate(item.Department, 80),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CertificationAssignmentView> ToCertificationAssignmentViews(List<CertificationAssignment> items)
    {
        var views = new List<CertificationAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToCertificationAssignmentView(item));
        }
        return views;
    }

    public static double SumCertificationAssignmentView(CertificationAssignmentView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedHours;
    }
}
