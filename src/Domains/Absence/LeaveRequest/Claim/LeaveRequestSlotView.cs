namespace Acme.Workforce.Absence.LeaveRequest.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a leave request slot.</summary>
public sealed record LeaveRequestSlotView(string CodeText, string Label, string NameText, string Archived, string Approved, string CostCenterText, string StartsAt, string AttemptsText, string Fte, string Quantity, string StatusLabel)
{
    public static LeaveRequestSlotView ToLeaveRequestSlotView(LeaveRequestSlot item)
    {
        return new LeaveRequestSlotView(
            item.Code,
            item.Label,
            item.Name,
            $"{item.Archived}",
            $"{item.Approved}",
            Text.Truncate(item.CostCenter, 40),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestSlotView> ToLeaveRequestSlotViews(List<LeaveRequestSlot> items)
    {
        var views = new List<LeaveRequestSlotView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestSlotView(item));
        }
        return views;
    }

    public static double SumLeaveRequestSlotView(LeaveRequestSlotView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedAttempts + parsedFte;
    }
}
