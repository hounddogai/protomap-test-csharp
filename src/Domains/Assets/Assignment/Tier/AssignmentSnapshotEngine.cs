namespace Acme.Workforce.Assets.Assignment.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the assignment snapshot calculation.</summary>
public interface IAssignmentSnapshotFactor
{
    string Describe();
    double Adjust(AssignmentSnapshot item, double valueValue);
}

public sealed record PercentAssignmentSnapshotAdjuster(double Percent) : IAssignmentSnapshotFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AssignmentSnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAssignmentSnapshotAdjuster(double Cap) : IAssignmentSnapshotFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AssignmentSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingAssignmentSnapshotAdjuster(double Step) : IAssignmentSnapshotFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssignmentSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameAssignmentSnapshotAdjuster(Dictionary<string, double> Factors) : IAssignmentSnapshotFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AssignmentSnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record AssignmentSnapshotUpdatedAtOffsetAdjuster(double Weight) : IAssignmentSnapshotFactor
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(AssignmentSnapshot item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

/// <summary>Runs the assignment snapshot steps in order and keeps a trace of each.</summary>
public sealed class AssignmentSnapshotEngine(List<IAssignmentSnapshotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentSnapshotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentSnapshotEngine DefaultAssignmentSnapshotEngine()
    {
        return new AssignmentSnapshotEngine(
            new List<IAssignmentSnapshotFactor>
            {
                new PercentAssignmentSnapshotAdjuster(-6.1),
                new CapAssignmentSnapshotAdjuster(8745.0),
                new RoundingAssignmentSnapshotAdjuster(0.05),
                new NameAssignmentSnapshotAdjuster(new Dictionary<string, double> { ["south"] = 0.79 }),
                new AssignmentSnapshotUpdatedAtOffsetAdjuster(1.27),
            },
            new List<string>(),
            3.1,
            69532.2);
    }

    public double Run(AssignmentSnapshot item)
    {
        var current = item.WeightedTotal();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<AssignmentSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
