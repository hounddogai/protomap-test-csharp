namespace Acme.Workforce.Absence.LeaveRequest.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a leave request quota.</summary>
public sealed record LeaveRequestQuotaView(string CodeText, string NameText, string Summary, string Revision, string Pinned, string Grade, string FloorValueText, string StatusLabel)
{
    public static LeaveRequestQuotaView ToLeaveRequestQuotaView(LeaveRequestQuota item)
    {
        return new LeaveRequestQuotaView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Summary,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Grade,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestQuotaView> ToLeaveRequestQuotaViews(List<LeaveRequestQuota> items)
    {
        var views = new List<LeaveRequestQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestQuotaView(item));
        }
        return views;
    }

    public static double SumLeaveRequestQuotaView(LeaveRequestQuotaView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedFloorValue;
    }
}
