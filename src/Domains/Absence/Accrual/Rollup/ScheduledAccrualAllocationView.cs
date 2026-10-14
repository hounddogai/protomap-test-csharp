namespace Acme.Workforce.Absence.Accrual.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled accrual allocation.</summary>
public sealed record ScheduledAccrualAllocationView(string Code, string NameText, string ManagerIdText, string Threshold, string MinimumText, string Locale, string JobCode, string Amount, string ApprovedText, string CurrencyText, string CapacityText, string StatusLabel)
{
    public static ScheduledAccrualAllocationView ToScheduledAccrualAllocationView(ScheduledAccrualAllocation item)
    {
        return new ScheduledAccrualAllocationView(
            item.Code,
            item.Name,
            Text.Truncate(item.ManagerId, 80),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            item.JobCode,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Currency,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledAccrualAllocationView> ToScheduledAccrualAllocationViews(List<ScheduledAccrualAllocation> items)
    {
        var views = new List<ScheduledAccrualAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledAccrualAllocationView(item));
        }
        return views;
    }

    public static double SumScheduledAccrualAllocationView(ScheduledAccrualAllocationView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedMinimum + parsedAmount;
    }
}
