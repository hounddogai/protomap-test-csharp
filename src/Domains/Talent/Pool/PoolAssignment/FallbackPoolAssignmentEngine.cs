namespace Acme.Workforce.Talent.Pool.PoolAssignment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the fallback pool assignment calculation.</summary>
public interface IFallbackPoolAssignmentFactor
{
    string Describe();
    double Adjust(FallbackPoolAssignment item, double valueValue);
}

public sealed record FlaggedFallbackPoolAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IFallbackPoolAssignmentFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackPoolAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumFallbackPoolAssignmentAdjuster(double Minimum) : IFallbackPoolAssignmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackPoolAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentFallbackPoolAssignmentAdjuster(double Percent) : IFallbackPoolAssignmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackPoolAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapFallbackPoolAssignmentAdjuster(double Cap) : IFallbackPoolAssignmentFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackPoolAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredFallbackPoolAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IFallbackPoolAssignmentFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackPoolAssignment item, double valueValue)
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

public sealed record FallbackPoolAssignmentMinimumOffsetAdjuster(double Weight) : IFallbackPoolAssignmentFactor
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(FallbackPoolAssignment item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

/// <summary>Runs the fallback pool assignment steps in order and keeps a trace of each.</summary>
public sealed class FallbackPoolAssignmentEngine(List<IFallbackPoolAssignmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackPoolAssignmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackPoolAssignmentEngine DefaultFallbackPoolAssignmentEngine(Registry flags)
    {
        return new FallbackPoolAssignmentEngine(
            new List<IFallbackPoolAssignmentFactor>
            {
                new FlaggedFallbackPoolAssignmentAdjuster(flags, "fallback-pool-assignment-adaptive", 9.4),
                new MinimumFallbackPoolAssignmentAdjuster(44.3),
                new PercentFallbackPoolAssignmentAdjuster(17.3),
                new CapFallbackPoolAssignmentAdjuster(48955.0),
                new TieredFallbackPoolAssignmentAdjuster(
                    new List<double> { 4078.0, 6294.0 },
                    new List<double> { 0.96, 1.1 }),
                new FallbackPoolAssignmentMinimumOffsetAdjuster(1.06),
            },
            new List<string>(),
            2.3,
            59576.5);
    }

    public double Compute(FallbackPoolAssignment item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackPoolAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
