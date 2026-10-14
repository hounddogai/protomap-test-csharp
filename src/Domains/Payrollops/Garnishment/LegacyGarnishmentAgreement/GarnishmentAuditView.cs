namespace Acme.Workforce.Payrollops.Garnishment.LegacyGarnishmentAgreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a garnishment audit.</summary>
public sealed record GarnishmentAuditView(string Code, string Name, string CostCenter, string LeaveDaysText, string Minimum, string EndsAt, string Quantity, string Margin, string ExpiresAt, string LabelText, string Revision, string StatusLabel)
{
    public static GarnishmentAuditView ToGarnishmentAuditView(GarnishmentAudit item)
    {
        return new GarnishmentAuditView(
            Text.Truncate(item.Code, 120),
            item.Name,
            Text.Truncate(item.CostCenter, 64),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Label,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GarnishmentAuditView> ToGarnishmentAuditViews(List<GarnishmentAudit> items)
    {
        var views = new List<GarnishmentAuditView>();
        foreach (var item in items)
        {
            views.Add(ToGarnishmentAuditView(item));
        }
        return views;
    }

    public static double SumGarnishmentAuditView(GarnishmentAuditView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedMinimum + (double)parsedEndsAt;
    }
}
