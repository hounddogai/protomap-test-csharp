namespace Acme.Workforce.Compensation.MeritCycle.Claim;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the global merit cycle rollup calculation.</summary>
public interface IGlobalMeritCycleRollupFactor
{
    string Describe();
    double Adjust(GlobalMeritCycleRollup item, double valueValue);
}

public sealed record FlaggedGlobalMeritCycleRollupAdjuster(Registry Flags, string Flag, double Percent) : IGlobalMeritCycleRollupFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalMeritCycleRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapGlobalMeritCycleRollupAdjuster(double Cap) : IGlobalMeritCycleRollupFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalMeritCycleRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredGlobalMeritCycleRollupAdjuster(List<double> Bounds, List<double> Rates) : IGlobalMeritCycleRollupFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalMeritCycleRollup item, double valueValue)
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

public sealed record MinimumGlobalMeritCycleRollupAdjuster(double Minimum) : IGlobalMeritCycleRollupFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalMeritCycleRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the global merit cycle rollup steps in order and keeps a trace of each.</summary>
public sealed class GlobalMeritCycleRollupEngine(List<IGlobalMeritCycleRollupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalMeritCycleRollupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalMeritCycleRollupEngine DefaultGlobalMeritCycleRollupEngine(Registry flags)
    {
        return new GlobalMeritCycleRollupEngine(
            new List<IGlobalMeritCycleRollupFactor>
            {
                new FlaggedGlobalMeritCycleRollupAdjuster(flags, "global-merit-cycle-rollup-fallback", -0.9),
                new CapGlobalMeritCycleRollupAdjuster(34765.0),
                new TieredGlobalMeritCycleRollupAdjuster(
                    new List<double> { 2627.0, 5959.0, 6958.0, 8220.0 },
                    new List<double> { 1.18, 0.86, 0.96, 0.92 }),
                new MinimumGlobalMeritCycleRollupAdjuster(38.8),
            },
            new List<string>(),
            3.4,
            84040.9);
    }

    public double Price(GlobalMeritCycleRollup item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalMeritCycleRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
