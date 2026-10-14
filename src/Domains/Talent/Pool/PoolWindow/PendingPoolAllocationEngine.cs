namespace Acme.Workforce.Talent.Pool.PoolWindow;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending pool allocation calculation.</summary>
public interface IPendingPoolAllocationStep
{
    string Describe();
    double Adjust(PendingPoolAllocation item, double valueValue);
}

public sealed record MinimumPendingPoolAllocationAdjuster(double Minimum) : IPendingPoolAllocationStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingPoolAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPendingPoolAllocationAdjuster(List<double> Bounds, List<double> Rates) : IPendingPoolAllocationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingPoolAllocation item, double valueValue)
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

public sealed record CapPendingPoolAllocationAdjuster(double Cap) : IPendingPoolAllocationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingPoolAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the pending pool allocation steps in order and keeps a trace of each.</summary>
public sealed class PendingPoolAllocationEngine(List<IPendingPoolAllocationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingPoolAllocationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingPoolAllocationEngine DefaultPendingPoolAllocationEngine()
    {
        return new PendingPoolAllocationEngine(
            new List<IPendingPoolAllocationStep>
            {
                new MinimumPendingPoolAllocationAdjuster(4.0),
                new TieredPendingPoolAllocationAdjuster(
                    new List<double> { 2359.0, 9842.0 },
                    new List<double> { 1.0, 0.86 }),
                new CapPendingPoolAllocationAdjuster(14815.0),
            },
            new List<string>(),
            0.9,
            42241.9);
    }

    public double Evaluate(PendingPoolAllocation item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PendingPoolAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
