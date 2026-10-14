namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

using System.Globalization;

/// <summary>How screens and exports show a adaptive adjustment notice.</summary>
public sealed record AdaptiveAdjustmentNoticeView(string Code, string NameText, string VisibleText, string ManagerId, string Channel, string Checksum, string StartsAt, string StatusLabel)
{
    public static AdaptiveAdjustmentNoticeView ToAdaptiveAdjustmentNoticeView(AdaptiveAdjustmentNotice item)
    {
        return new AdaptiveAdjustmentNoticeView(
            item.Code,
            item.Name,
            $"{item.Visible}",
            item.ManagerId,
            item.Channel,
            item.Checksum,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveAdjustmentNoticeView> ToAdaptiveAdjustmentNoticeViews(List<AdaptiveAdjustmentNotice> items)
    {
        var views = new List<AdaptiveAdjustmentNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveAdjustmentNoticeView(item));
        }
        return views;
    }

    public static double SumAdaptiveAdjustmentNoticeView(AdaptiveAdjustmentNoticeView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt;
    }
}
