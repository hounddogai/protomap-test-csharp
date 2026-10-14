namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using System.Globalization;

/// <summary>How screens and exports show a legacy leave request audit.</summary>
public sealed record LegacyLeaveRequestAuditView(string CodeText, string Label, string Retries, string Pinned, string StatusLabel)
{
    public static LegacyLeaveRequestAuditView ToLegacyLeaveRequestAuditView(LegacyLeaveRequestAudit item)
    {
        return new LegacyLeaveRequestAuditView(
            item.Code,
            item.Label,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyLeaveRequestAuditView> ToLegacyLeaveRequestAuditViews(List<LegacyLeaveRequestAudit> items)
    {
        var views = new List<LegacyLeaveRequestAuditView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyLeaveRequestAuditView(item));
        }
        return views;
    }

    public static double SumLegacyLeaveRequestAuditView(LegacyLeaveRequestAuditView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedRetries;
    }
}
