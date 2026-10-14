namespace Acme.Workforce.Workplace.Visitor.Slot;

using Acme.Workforce.Workplace.Desk.Milestone;

/// <summary>Functions of visitor slot from checkpoint.</summary>
public static class VisitorSlotFromCheckpoint
{
    public static Dictionary<string, double> SummarizeDeskCheckpointsForSlot(List<DeskCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<VisitorSlot> DeriveVisitorSlotsFromCheckpoint(List<DeskCheckpoint> items)
    {
        var outValue = new List<VisitorSlot>();
        foreach (var item in items)
        {
            var derived = VisitorSlot.NewVisitorSlot(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectVisitorSlot(VisitorSlot item)
    {
        return DeskMilestonePolicy.NormalizeDeskMilestoneCode(item.Code);
    }
}
