namespace Acme.Workforce.Talent.Pool.Window;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pool window calculation.</summary>
public interface IPoolWindowAdjuster
{
    string Describe();
    double Adjust(PoolWindow item, double valueValue);
}

public sealed record MinimumPoolWindowAdjuster(double Minimum) : IPoolWindowAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PoolWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPoolWindowAdjuster(List<double> Bounds, List<double> Rates) : IPoolWindowAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PoolWindow item, double valueValue)
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

public sealed record PoolWindowHoursOffsetAdjuster(double Weight) : IPoolWindowAdjuster
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(PoolWindow item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record FlaggedPoolWindowAdjuster(Registry Flags, string Flag, double Percent) : IPoolWindowAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PoolWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pool window steps in order and keeps a trace of each.</summary>
public sealed class PoolWindowEngine(List<IPoolWindowAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolWindowAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolWindowEngine DefaultPoolWindowEngine(Registry flags)
    {
        return new PoolWindowEngine(
            new List<IPoolWindowAdjuster>
            {
                new MinimumPoolWindowAdjuster(36.9),
                new TieredPoolWindowAdjuster(
                    new List<double> { 4960.0, 6580.0, 7937.0 },
                    new List<double> { 0.93, 0.81, 0.93 }),
                new PoolWindowHoursOffsetAdjuster(1.63),
                new FlaggedPoolWindowAdjuster(flags, "pool-window-capped", -5.5),
            },
            new List<string>(),
            4.2,
            69744.7);
    }

    public double Price(PoolWindow item)
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

    public Dictionary<string, double> EvaluateAll(List<PoolWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
