namespace Acme.Workforce.Payrollops.PayRun.Preference;

using System.Globalization;

/// <summary>How screens and exports show a pay run audit.</summary>
public sealed record PayRunAuditView(string Code, string TitleText, string Score, string Threshold, string ManagerId, string ExternalRefText, string StartsAt, string StatusLabel)
{
    public static PayRunAuditView ToPayRunAuditView(PayRunAudit item)
    {
        return new PayRunAuditView(
            item.Code,
            item.Title,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            item.ExternalRef,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayRunAuditView> ToPayRunAuditViews(List<PayRunAudit> items)
    {
        var views = new List<PayRunAuditView>();
        foreach (var item in items)
        {
            views.Add(ToPayRunAuditView(item));
        }
        return views;
    }

    public static double SumPayRunAuditView(PayRunAuditView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return parsedScore + parsedThreshold + (double)parsedStartsAt;
    }
}
