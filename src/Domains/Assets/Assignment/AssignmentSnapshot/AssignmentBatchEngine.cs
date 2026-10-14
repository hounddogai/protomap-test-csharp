namespace Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the assignment batch calculation.</summary>
public interface IAssignmentBatchModifier
{
    string Describe();
    double Adjust(AssignmentBatch item, double valueValue);
}

public sealed record MinimumAssignmentBatchAdjuster(double Minimum) : IAssignmentBatchModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssignmentBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredAssignmentBatchAdjuster(List<double> Bounds, List<double> Rates) : IAssignmentBatchModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssignmentBatch item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record FlaggedAssignmentBatchAdjuster(Registry Flags, string Flag, double Percent) : IAssignmentBatchModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssignmentBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the assignment batch steps in order and keeps a trace of each.</summary>
public sealed class AssignmentBatchEngine(List<IAssignmentBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentBatchEngine DefaultAssignmentBatchEngine(Registry flags)
    {
        return new AssignmentBatchEngine(
            new List<IAssignmentBatchModifier>
            {
                new MinimumAssignmentBatchAdjuster(13.5),
                new TieredAssignmentBatchAdjuster(
                    new List<double> { 871.0, 4282.0, 6378.0, 7222.0 },
                    new List<double> { 0.93, 0.94, 0.83, 0.84 }),
                new FlaggedAssignmentBatchAdjuster(flags, "assignment-batch-weighted", 4.8),
            },
            new List<string>(),
            3.7,
            8193.7);
    }

    public double Evaluate(AssignmentBatch item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AssignmentBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
