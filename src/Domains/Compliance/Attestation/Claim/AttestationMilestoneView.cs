namespace Acme.Workforce.Compliance.Attestation.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a attestation milestone.</summary>
public sealed record AttestationMilestoneView(string Code, string LabelText, string DurationMinutes, string Name, string ExpiresAt, string Notes, string Revision, string Ratio, string LeaveDaysText, string Description, string EmployeeId, string StatusLabel)
{
    public static AttestationMilestoneView ToAttestationMilestoneView(AttestationMilestone item)
    {
        return new AttestationMilestoneView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Name,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Description,
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AttestationMilestoneView> ToAttestationMilestoneViews(List<AttestationMilestone> items)
    {
        var views = new List<AttestationMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToAttestationMilestoneView(item));
        }
        return views;
    }

    public static double SumAttestationMilestoneView(AttestationMilestoneView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedExpiresAt + (double)parsedRevision;
    }
}
