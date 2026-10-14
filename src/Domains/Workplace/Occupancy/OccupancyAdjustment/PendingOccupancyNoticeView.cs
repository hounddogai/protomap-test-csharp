namespace Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending occupancy notice.</summary>
public sealed record PendingOccupancyNoticeView(string Code, string Name, string TenureMonths, string SalaryBand, string Attempts, string ExternalRefText, string OwnerIdText, string FloorValue, string Notes, string Summary, string StatusLabel)
{
    public static PendingOccupancyNoticeView ToPendingOccupancyNoticeView(PendingOccupancyNotice item)
    {
        return new PendingOccupancyNoticeView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ExternalRef, 120),
            item.OwnerId,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 80),
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingOccupancyNoticeView> ToPendingOccupancyNoticeViews(List<PendingOccupancyNotice> items)
    {
        var views = new List<PendingOccupancyNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToPendingOccupancyNoticeView(item));
        }
        return views;
    }

    public static double SumPendingOccupancyNoticeView(PendingOccupancyNoticeView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedAttempts + parsedFloorValue;
    }
}
