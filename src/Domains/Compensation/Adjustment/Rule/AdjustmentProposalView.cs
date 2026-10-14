namespace Acme.Workforce.Compensation.Adjustment.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adjustment proposal.</summary>
public sealed record AdjustmentProposalView(string Code, string Name, string Label, string StartsAt, string Sequence, string WindowDays, string OwnerId, string Minimum, string ManagerId, string StatusLabel)
{
    public static AdjustmentProposalView ToAdjustmentProposalView(AdjustmentProposal item)
    {
        return new AdjustmentProposalView(
            item.Code,
            item.Name,
            Text.Truncate(item.Label, 40),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentProposalView> ToAdjustmentProposalViews(List<AdjustmentProposal> items)
    {
        var views = new List<AdjustmentProposalView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentProposalView(item));
        }
        return views;
    }

    public static double SumAdjustmentProposalView(AdjustmentProposalView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedSequence + (double)parsedWindowDays;
    }
}
