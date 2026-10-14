namespace Acme.Workforce.Compensation.BonusPool.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending bonus pool allocation calculation.</summary>
public interface IPendingBonusPoolAllocationAdjuster
{
    string Describe();
    double Adjust(PendingBonusPoolAllocation item, double valueValue);
}

public sealed record FlaggedPendingBonusPoolAllocationAdjuster(Registry Flags, string Flag, double Percent) : IPendingBonusPoolAllocationAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingBonusPoolAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPendingBonusPoolAllocationAdjuster(double Cap) : IPendingBonusPoolAllocationAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingBonusPoolAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPendingBonusPoolAllocationAdjuster(double Percent) : IPendingBonusPoolAllocationAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingBonusPoolAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending bonus pool allocation steps in order and keeps a trace of each.</summary>
public sealed class PendingBonusPoolAllocationEngine(List<IPendingBonusPoolAllocationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingBonusPoolAllocationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingBonusPoolAllocationEngine DefaultPendingBonusPoolAllocationEngine(Registry flags)
    {
        return new PendingBonusPoolAllocationEngine(
            new List<IPendingBonusPoolAllocationAdjuster>
            {
                new FlaggedPendingBonusPoolAllocationAdjuster(flags, "pending-bonus-pool-allocation-scheduled", 8.2),
                new CapPendingBonusPoolAllocationAdjuster(33333.0),
                new PercentPendingBonusPoolAllocationAdjuster(-23.7),
            },
            new List<string>(),
            5.0,
            49852.3);
    }

    public double Evaluate(PendingBonusPoolAllocation item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingBonusPoolAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
