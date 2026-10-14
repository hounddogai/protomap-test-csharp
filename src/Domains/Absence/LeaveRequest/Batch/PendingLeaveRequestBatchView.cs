namespace Acme.Workforce.Absence.LeaveRequest.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending leave request batch.</summary>
public sealed record PendingLeaveRequestBatchView(string Code, string Title, string Segment, string FloorValueText, string EndsAtText, string Visible, string StatusLabel)
{
    public static PendingLeaveRequestBatchView ToPendingLeaveRequestBatchView(PendingLeaveRequestBatch item)
    {
        return new PendingLeaveRequestBatchView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Segment,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingLeaveRequestBatchView> ToPendingLeaveRequestBatchViews(List<PendingLeaveRequestBatch> items)
    {
        var views = new List<PendingLeaveRequestBatchView>();
        foreach (var item in items)
        {
            views.Add(ToPendingLeaveRequestBatchView(item));
        }
        return views;
    }

    public static double SumPendingLeaveRequestBatchView(PendingLeaveRequestBatchView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedEndsAt;
    }
}
