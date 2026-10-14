namespace Acme.Workforce.Payrollops.Deduction.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a deduction quota.</summary>
public sealed record DeductionQuotaView(string CodeText, string Name, string Country, string LeaveDays, string UpdatedAt, string SourceText, string Approved, string Phone, string Cost, string StatusLabel)
{
    public static DeductionQuotaView ToDeductionQuotaView(DeductionQuota item)
    {
        return new DeductionQuotaView(
            item.Code,
            item.Name,
            Text.Truncate(item.Country, 64),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Source,
            $"{item.Approved}",
            item.Phone,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeductionQuotaView> ToDeductionQuotaViews(List<DeductionQuota> items)
    {
        var views = new List<DeductionQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToDeductionQuotaView(item));
        }
        return views;
    }

    public static double SumDeductionQuotaView(DeductionQuotaView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedUpdatedAt + parsedCost;
    }
}
