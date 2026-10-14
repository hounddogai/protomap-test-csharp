namespace Acme.Workforce.Contractors.Rate.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled rate allocation.</summary>
public sealed record ScheduledRateAllocationView(string CodeText, string Label, string Amount, string Department, string Priority, string ExpiresAtText, string StatusLabel)
{
    public static ScheduledRateAllocationView ToScheduledRateAllocationView(ScheduledRateAllocation item)
    {
        return new ScheduledRateAllocationView(
            item.Code,
            item.Label,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 120),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledRateAllocationView> ToScheduledRateAllocationViews(List<ScheduledRateAllocation> items)
    {
        var views = new List<ScheduledRateAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledRateAllocationView(item));
        }
        return views;
    }

    public static double SumScheduledRateAllocationView(ScheduledRateAllocationView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedPriority + (double)parsedExpiresAt;
    }
}
