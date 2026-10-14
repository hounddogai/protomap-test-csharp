namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the legacy bonus pool allocation calculation.</summary>
public interface ILegacyBonusPoolAllocationAdjuster
{
    string Describe();
    double Adjust(LegacyBonusPoolAllocation item, double valueValue);
}

public sealed record LegacyBonusPoolAllocationLeaveDaysOffsetAdjuster(double Weight) : ILegacyBonusPoolAllocationAdjuster
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(LegacyBonusPoolAllocation item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record CapLegacyBonusPoolAllocationAdjuster(double Cap) : ILegacyBonusPoolAllocationAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyBonusPoolAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLegacyBonusPoolAllocationAdjuster(double Minimum) : ILegacyBonusPoolAllocationAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyBonusPoolAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SegmentLegacyBonusPoolAllocationAdjuster(Dictionary<string, double> Factors) : ILegacyBonusPoolAllocationAdjuster
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(LegacyBonusPoolAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedLegacyBonusPoolAllocationAdjuster(Registry Flags, string Flag, double Percent) : ILegacyBonusPoolAllocationAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyBonusPoolAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy bonus pool allocation steps in order and keeps a trace of each.</summary>
public sealed class LegacyBonusPoolAllocationEngine(List<ILegacyBonusPoolAllocationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyBonusPoolAllocationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyBonusPoolAllocationEngine DefaultLegacyBonusPoolAllocationEngine(Registry flags)
    {
        return new LegacyBonusPoolAllocationEngine(
            new List<ILegacyBonusPoolAllocationAdjuster>
            {
                new LegacyBonusPoolAllocationLeaveDaysOffsetAdjuster(1.25),
                new CapLegacyBonusPoolAllocationAdjuster(9353.0),
                new MinimumLegacyBonusPoolAllocationAdjuster(48.1),
                new SegmentLegacyBonusPoolAllocationAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.85, ["online"] = 0.79, ["south"] = 0.71 }),
                new FlaggedLegacyBonusPoolAllocationAdjuster(flags, "legacy-bonus-pool-allocation-strict", 8.7),
            },
            new List<string>(),
            4.2,
            65772.1);
    }

    public double Compute(LegacyBonusPoolAllocation item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyBonusPoolAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
