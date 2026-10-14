namespace Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a succession plan proposal.</summary>
public sealed record SuccessionPlanProposalView(string Code, string Name, string StartsAt, string ManagerIdText, string NotesText, string CapacityText, string Label, string FloorValueText, string Margin, string SequenceText, string Hours, string StatusLabel)
{
    public static SuccessionPlanProposalView ToSuccessionPlanProposalView(SuccessionPlanProposal item)
    {
        return new SuccessionPlanProposalView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Notes,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Label, 40),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SuccessionPlanProposalView> ToSuccessionPlanProposalViews(List<SuccessionPlanProposal> items)
    {
        var views = new List<SuccessionPlanProposalView>();
        foreach (var item in items)
        {
            views.Add(ToSuccessionPlanProposalView(item));
        }
        return views;
    }

    public static double SumSuccessionPlanProposalView(SuccessionPlanProposalView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedCapacity + parsedFloorValue;
    }
}
