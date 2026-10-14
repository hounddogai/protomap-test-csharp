namespace Acme.Workforce.Assets.Assignment.Checkpoint;

using Acme.Workforce.Assets.Laptop.Milestone;

/// <summary>Functions of assignment checkpoint from window.</summary>
public static class AssignmentCheckpointFromWindow
{
    public static Dictionary<string, double> SummarizeLaptopWindowsForCheckpoint(List<LaptopWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<AssignmentCheckpoint> DeriveAssignmentCheckpointsFromWindow(List<LaptopWindow> items)
    {
        var outValue = new List<AssignmentCheckpoint>();
        foreach (var item in items)
        {
            var derived = AssignmentCheckpoint.NewAssignmentCheckpoint(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyAssignmentCheckpoint(AssignmentCheckpoint item)
    {
        return LaptopTierPolicy.ClassifyLaptopTier(item.ProjectedVolume());
    }
}
