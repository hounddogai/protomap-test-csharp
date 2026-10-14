namespace Acme.Workforce.Workplace.AccessBadge.Audit;

using System.Globalization;

/// <summary>How screens and exports show a access badge audit.</summary>
public sealed record AccessBadgeAuditView(string Code, string NameText, string Unit, string QuantityText, string Skill, string EmployeeIdText, string Weight, string RetriesText, string CreatedAtText, string AmountText, string StatusLabel)
{
    public static AccessBadgeAuditView ToAccessBadgeAuditView(AccessBadgeAudit item)
    {
        return new AccessBadgeAuditView(
            item.Code,
            item.Name,
            item.Unit,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.EmployeeId,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccessBadgeAuditView> ToAccessBadgeAuditViews(List<AccessBadgeAudit> items)
    {
        var views = new List<AccessBadgeAuditView>();
        foreach (var item in items)
        {
            views.Add(ToAccessBadgeAuditView(item));
        }
        return views;
    }

    public static double SumAccessBadgeAuditView(AccessBadgeAuditView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedWeight + (double)parsedRetries;
    }
}
