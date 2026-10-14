namespace Acme.Workforce.Absence.LeaveRequest.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary leave request claim.</summary>
public sealed record PrimaryLeaveRequestClaimView(string CodeText, string Name, string ApprovedText, string LeaveDays, string Skill, string BatchSizeText, string FloorValue, string StatusLabel)
{
    public static PrimaryLeaveRequestClaimView ToPrimaryLeaveRequestClaimView(PrimaryLeaveRequestClaim item)
    {
        return new PrimaryLeaveRequestClaimView(
            item.Code,
            item.Name,
            $"{item.Approved}",
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Skill, 120),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryLeaveRequestClaimView> ToPrimaryLeaveRequestClaimViews(List<PrimaryLeaveRequestClaim> items)
    {
        var views = new List<PrimaryLeaveRequestClaimView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryLeaveRequestClaimView(item));
        }
        return views;
    }

    public static double SumPrimaryLeaveRequestClaimView(PrimaryLeaveRequestClaimView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedBatchSize + parsedFloorValue;
    }
}
