namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the access badge allocation calculation.</summary>
public interface IAccessBadgeAllocationModifier
{
    string Describe();
    double Adjust(AccessBadgeAllocation item, double valueValue);
}

public sealed record FlaggedAccessBadgeAllocationAdjuster(Registry Flags, string Flag, double Percent) : IAccessBadgeAllocationModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccessBadgeAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AccessBadgeAllocationRetriesOffsetAdjuster(double Weight) : IAccessBadgeAllocationModifier
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(AccessBadgeAllocation item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record TieredAccessBadgeAllocationAdjuster(List<double> Bounds, List<double> Rates) : IAccessBadgeAllocationModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccessBadgeAllocation item, double valueValue)
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

public sealed record MinimumAccessBadgeAllocationAdjuster(double Minimum) : IAccessBadgeAllocationModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccessBadgeAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAccessBadgeAllocationAdjuster(double Cap) : IAccessBadgeAllocationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccessBadgeAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingAccessBadgeAllocationAdjuster(double Step) : IAccessBadgeAllocationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccessBadgeAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the access badge allocation steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeAllocationEngine(List<IAccessBadgeAllocationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeAllocationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeAllocationEngine DefaultAccessBadgeAllocationEngine(Registry flags)
    {
        return new AccessBadgeAllocationEngine(
            new List<IAccessBadgeAllocationModifier>
            {
                new FlaggedAccessBadgeAllocationAdjuster(flags, "access-badge-allocation-adaptive", -3.2),
                new AccessBadgeAllocationRetriesOffsetAdjuster(1.62),
                new TieredAccessBadgeAllocationAdjuster(
                    new List<double> { 9477.0, 9865.0 },
                    new List<double> { 1.0, 1.07 }),
                new MinimumAccessBadgeAllocationAdjuster(34.0),
                new CapAccessBadgeAllocationAdjuster(43588.0),
                new RoundingAccessBadgeAllocationAdjuster(5.0),
            },
            new List<string>(),
            1.4,
            71785.3);
    }

    public double Run(AccessBadgeAllocation item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
