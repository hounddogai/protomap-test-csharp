namespace Acme.Workforce.Talent.Pool.Baseline;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pool reservation calculation.</summary>
public interface IPoolReservationAdjuster
{
    string Describe();
    double Adjust(PoolReservation item, double valueValue);
}

public sealed record MinimumPoolReservationAdjuster(double Minimum) : IPoolReservationAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PoolReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedPoolReservationAdjuster(Registry Flags, string Flag, double Percent) : IPoolReservationAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PoolReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PoolReservationCapacityOffsetAdjuster(double Weight) : IPoolReservationAdjuster
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(PoolReservation item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record TieredPoolReservationAdjuster(List<double> Bounds, List<double> Rates) : IPoolReservationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PoolReservation item, double valueValue)
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

public sealed record LabelPoolReservationAdjuster(Dictionary<string, double> Factors) : IPoolReservationAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(PoolReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPoolReservationAdjuster(double Percent) : IPoolReservationAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PoolReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pool reservation steps in order and keeps a trace of each.</summary>
public sealed class PoolReservationEngine(List<IPoolReservationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolReservationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolReservationEngine DefaultPoolReservationEngine(Registry flags)
    {
        return new PoolReservationEngine(
            new List<IPoolReservationAdjuster>
            {
                new MinimumPoolReservationAdjuster(42.3),
                new FlaggedPoolReservationAdjuster(flags, "pool-reservation-fallback", -5.6),
                new PoolReservationCapacityOffsetAdjuster(0.16),
                new TieredPoolReservationAdjuster(
                    new List<double> { 1231.0, 2435.0, 5457.0, 5603.0 },
                    new List<double> { 1.17, 1.11, 1.14, 1.1 }),
                new LabelPoolReservationAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.33, ["east"] = 1.36, ["south"] = 1.2 }),
                new PercentPoolReservationAdjuster(22.2),
            },
            new List<string>(),
            3.4,
            88341.0);
    }

    public double Compute(PoolReservation item)
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

    public Dictionary<string, double> EvaluateAll(List<PoolReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
