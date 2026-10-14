namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled adjustment request.</summary>
public sealed record ScheduledAdjustmentRequestView(string CodeText, string Label, string RegionText, string LocationText, string Limit, string Badge, string SequenceText, string OwnerId, string ChannelText, string Team, string StatusLabel)
{
    public static ScheduledAdjustmentRequestView ToScheduledAdjustmentRequestView(ScheduledAdjustmentRequest item)
    {
        return new ScheduledAdjustmentRequestView(
            item.Code,
            item.Label,
            item.Region,
            item.Location,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 40),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 40),
            Text.Truncate(item.Channel, 40),
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledAdjustmentRequestView> ToScheduledAdjustmentRequestViews(List<ScheduledAdjustmentRequest> items)
    {
        var views = new List<ScheduledAdjustmentRequestView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledAdjustmentRequestView(item));
        }
        return views;
    }

    public static double SumScheduledAdjustmentRequestView(ScheduledAdjustmentRequestView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedSequence;
    }
}
