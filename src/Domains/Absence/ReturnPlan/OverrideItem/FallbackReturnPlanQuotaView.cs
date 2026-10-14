namespace Acme.Workforce.Absence.ReturnPlan.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback return plan quota.</summary>
public sealed record FallbackReturnPlanQuotaView(string Code, string Title, string ExpiresAt, string LeaveDays, string SegmentText, string Location, string Quantity, string Rate, string Score, string Notes, string StatusLabel)
{
    public static FallbackReturnPlanQuotaView ToFallbackReturnPlanQuotaView(FallbackReturnPlanQuota item)
    {
        return new FallbackReturnPlanQuotaView(
            item.Code,
            item.Title,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 64),
            item.Location,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackReturnPlanQuotaView> ToFallbackReturnPlanQuotaViews(List<FallbackReturnPlanQuota> items)
    {
        var views = new List<FallbackReturnPlanQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackReturnPlanQuotaView(item));
        }
        return views;
    }

    public static double SumFallbackReturnPlanQuotaView(FallbackReturnPlanQuotaView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedLeaveDays + (double)parsedQuantity;
    }
}
