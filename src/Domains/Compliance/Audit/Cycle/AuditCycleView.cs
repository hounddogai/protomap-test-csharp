namespace Acme.Workforce.Compliance.Audit.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a audit cycle.</summary>
public sealed record AuditCycleView(string Code, string LabelText, string Budget, string OwnerIdText, string ArchivedText, string Minimum, string Visible, string TenureMonthsText, string Unit, string UpdatedAt, string StatusLabel)
{
    public static AuditCycleView ToAuditCycleView(AuditCycle item)
    {
        return new AuditCycleView(
            item.Code,
            item.Label,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 64),
            $"{item.Archived}",
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AuditCycleView> ToAuditCycleViews(List<AuditCycle> items)
    {
        var views = new List<AuditCycleView>();
        foreach (var item in items)
        {
            views.Add(ToAuditCycleView(item));
        }
        return views;
    }

    public static double SumAuditCycleView(AuditCycleView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return parsedBudget + parsedMinimum + (double)parsedTenureMonths;
    }
}
