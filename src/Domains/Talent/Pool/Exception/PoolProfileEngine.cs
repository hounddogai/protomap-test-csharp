namespace Acme.Workforce.Talent.Pool.Exception;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pool profile calculation.</summary>
public interface IPoolProfileModifier
{
    string Describe();
    double Adjust(PoolProfile item, double valueValue);
}

public sealed record CapPoolProfileAdjuster(double Cap) : IPoolProfileModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PoolProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPoolProfileAdjuster(double Percent) : IPoolProfileModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PoolProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPoolProfileAdjuster(Registry Flags, string Flag, double Percent) : IPoolProfileModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PoolProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPoolProfileAdjuster(double Minimum) : IPoolProfileModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PoolProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingPoolProfileAdjuster(double Step) : IPoolProfileModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PoolProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PoolProfileCreatedAtOffsetAdjuster(double Weight) : IPoolProfileModifier
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(PoolProfile item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

/// <summary>Runs the pool profile steps in order and keeps a trace of each.</summary>
public sealed class PoolProfileEngine(List<IPoolProfileModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolProfileModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolProfileEngine DefaultPoolProfileEngine(Registry flags)
    {
        return new PoolProfileEngine(
            new List<IPoolProfileModifier>
            {
                new CapPoolProfileAdjuster(19186.0),
                new PercentPoolProfileAdjuster(17.7),
                new FlaggedPoolProfileAdjuster(flags, "pool-profile-fallback", 8.6),
                new MinimumPoolProfileAdjuster(27.1),
                new RoundingPoolProfileAdjuster(0.5),
                new PoolProfileCreatedAtOffsetAdjuster(1.36),
            },
            new List<string>(),
            2.3,
            38384.6);
    }

    public double Run(PoolProfile item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PoolProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
