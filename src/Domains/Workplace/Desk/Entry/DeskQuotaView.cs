namespace Acme.Workforce.Workplace.Desk.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a desk quota.</summary>
public sealed record DeskQuotaView(string Code, string Title, string LeaveDaysText, string EmployeeId, string RetriesText, string Phone, string ApprovedText, string Budget, string DurationMinutes, string RegionText, string Description, string StatusLabel)
{
    public static DeskQuotaView ToDeskQuotaView(DeskQuota item)
    {
        return new DeskQuotaView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 64),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            $"{item.Approved}",
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 120),
            item.Description,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeskQuotaView> ToDeskQuotaViews(List<DeskQuota> items)
    {
        var views = new List<DeskQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToDeskQuotaView(item));
        }
        return views;
    }

    public static double SumDeskQuotaView(DeskQuotaView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedRetries + parsedBudget;
    }
}
