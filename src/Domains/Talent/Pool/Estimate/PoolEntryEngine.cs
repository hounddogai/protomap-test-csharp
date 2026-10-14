namespace Acme.Workforce.Talent.Pool.Estimate;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pool entry calculation.</summary>
public interface IPoolEntryModifier
{
    string Describe();
    double Adjust(PoolEntry item, double valueValue);
}

public sealed record TieredPoolEntryAdjuster(List<double> Bounds, List<double> Rates) : IPoolEntryModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PoolEntry item, double valueValue)
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

public sealed record FlaggedPoolEntryAdjuster(Registry Flags, string Flag, double Percent) : IPoolEntryModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PoolEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPoolEntryAdjuster(double Minimum) : IPoolEntryModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PoolEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPoolEntryAdjuster(double Percent) : IPoolEntryModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PoolEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PoolEntryDiscountOffsetAdjuster(double Weight) : IPoolEntryModifier
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(PoolEntry item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

/// <summary>Runs the pool entry steps in order and keeps a trace of each.</summary>
public sealed class PoolEntryEngine(List<IPoolEntryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolEntryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolEntryEngine DefaultPoolEntryEngine(Registry flags)
    {
        return new PoolEntryEngine(
            new List<IPoolEntryModifier>
            {
                new TieredPoolEntryAdjuster(
                    new List<double> { 3708.0, 3881.0, 9025.0, 9368.0 },
                    new List<double> { 1.03, 1.1, 1.12, 1.18 }),
                new FlaggedPoolEntryAdjuster(flags, "pool-entry-strict", 4.1),
                new MinimumPoolEntryAdjuster(8.6),
                new PercentPoolEntryAdjuster(-12.9),
                new PoolEntryDiscountOffsetAdjuster(1.97),
            },
            new List<string>(),
            4.5,
            86730.1);
    }

    public double Run(PoolEntry item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PoolEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
