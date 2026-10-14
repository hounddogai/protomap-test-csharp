namespace Acme.Workforce.Workplace.Occupancy.OccupancyDraft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling occupancy allocation calculation.</summary>
public interface IRollingOccupancyAllocationStep
{
    string Describe();
    double Adjust(RollingOccupancyAllocation item, double valueValue);
}

public sealed record RoundingRollingOccupancyAllocationAdjuster(double Step) : IRollingOccupancyAllocationStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingOccupancyAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredRollingOccupancyAllocationAdjuster(List<double> Bounds, List<double> Rates) : IRollingOccupancyAllocationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingOccupancyAllocation item, double valueValue)
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

public sealed record PercentRollingOccupancyAllocationAdjuster(double Percent) : IRollingOccupancyAllocationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingOccupancyAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRollingOccupancyAllocationAdjuster(double Minimum) : IRollingOccupancyAllocationStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingOccupancyAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedRollingOccupancyAllocationAdjuster(Registry Flags, string Flag, double Percent) : IRollingOccupancyAllocationStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingOccupancyAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentRollingOccupancyAllocationAdjuster(Dictionary<string, double> Factors) : IRollingOccupancyAllocationStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(RollingOccupancyAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the rolling occupancy allocation steps in order and keeps a trace of each.</summary>
public sealed class RollingOccupancyAllocationEngine(List<IRollingOccupancyAllocationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingOccupancyAllocationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingOccupancyAllocationEngine DefaultRollingOccupancyAllocationEngine(Registry flags)
    {
        return new RollingOccupancyAllocationEngine(
            new List<IRollingOccupancyAllocationStep>
            {
                new RoundingRollingOccupancyAllocationAdjuster(5.0),
                new TieredRollingOccupancyAllocationAdjuster(
                    new List<double> { 1341.0, 6137.0, 6165.0, 7621.0 },
                    new List<double> { 1.15, 1.08, 1.12, 0.92 }),
                new PercentRollingOccupancyAllocationAdjuster(-0.9),
                new MinimumRollingOccupancyAllocationAdjuster(38.7),
                new FlaggedRollingOccupancyAllocationAdjuster(flags, "rolling-occupancy-allocation-seasonal", -4.3),
                new DepartmentRollingOccupancyAllocationAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.29, ["retail"] = 1.39 }),
            },
            new List<string>(),
            2.0,
            65565.0);
    }

    public double Compute(RollingOccupancyAllocation item)
    {
        var current = item.AdjustedTotal();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<RollingOccupancyAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
