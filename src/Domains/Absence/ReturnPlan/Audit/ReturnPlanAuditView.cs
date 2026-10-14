namespace Acme.Workforce.Absence.ReturnPlan.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a return plan audit.</summary>
public sealed record ReturnPlanAuditView(string Code, string Name, string LocationText, string TenureMonthsText, string OwnerIdText, string Fte, string ArchivedText, string CreatedAtText, string BatchSize, string Title, string Grade, string StatusLabel)
{
    public static ReturnPlanAuditView ToReturnPlanAuditView(ReturnPlanAudit item)
    {
        return new ReturnPlanAuditView(
            item.Code,
            item.Name,
            Text.Truncate(item.Location, 80),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Title,
            item.Grade,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReturnPlanAuditView> ToReturnPlanAuditViews(List<ReturnPlanAudit> items)
    {
        var views = new List<ReturnPlanAuditView>();
        foreach (var item in items)
        {
            views.Add(ToReturnPlanAuditView(item));
        }
        return views;
    }

    public static double SumReturnPlanAuditView(ReturnPlanAuditView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + parsedFte + (double)parsedCreatedAt;
    }
}
