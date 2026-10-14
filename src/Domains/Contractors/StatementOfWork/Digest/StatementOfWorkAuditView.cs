namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

using System.Globalization;

/// <summary>How screens and exports show a statement of work audit.</summary>
public sealed record StatementOfWorkAuditView(string Code, string Name, string Pinned, string Version, string ExpiresAt, string ThresholdText, string Weight, string EmployeeId, string WindowDays, string LeaveDays, string StatusLabel)
{
    public static StatementOfWorkAuditView ToStatementOfWorkAuditView(StatementOfWorkAudit item)
    {
        return new StatementOfWorkAuditView(
            item.Code,
            item.Name,
            $"{item.Pinned}",
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkAuditView> ToStatementOfWorkAuditViews(List<StatementOfWorkAudit> items)
    {
        var views = new List<StatementOfWorkAuditView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkAuditView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkAuditView(StatementOfWorkAuditView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedExpiresAt + parsedThreshold;
    }
}
