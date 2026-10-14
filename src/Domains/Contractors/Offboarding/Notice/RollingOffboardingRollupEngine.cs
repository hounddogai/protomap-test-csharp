namespace Acme.Workforce.Contractors.Offboarding.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling offboarding rollup calculation.</summary>
public interface IRollingOffboardingRollupAdjuster
{
    string Describe();
    double Adjust(RollingOffboardingRollup item, double valueValue);
}

public sealed record CapRollingOffboardingRollupAdjuster(double Cap) : IRollingOffboardingRollupAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingOffboardingRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRollingOffboardingRollupAdjuster(double Percent) : IRollingOffboardingRollupAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingOffboardingRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRollingOffboardingRollupAdjuster(Registry Flags, string Flag, double Percent) : IRollingOffboardingRollupAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingOffboardingRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRollingOffboardingRollupAdjuster(List<double> Bounds, List<double> Rates) : IRollingOffboardingRollupAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingOffboardingRollup item, double valueValue)
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

public sealed record RoundingRollingOffboardingRollupAdjuster(double Step) : IRollingOffboardingRollupAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingOffboardingRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumRollingOffboardingRollupAdjuster(double Minimum) : IRollingOffboardingRollupAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingOffboardingRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the rolling offboarding rollup steps in order and keeps a trace of each.</summary>
public sealed class RollingOffboardingRollupEngine(List<IRollingOffboardingRollupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingOffboardingRollupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingOffboardingRollupEngine DefaultRollingOffboardingRollupEngine(Registry flags)
    {
        return new RollingOffboardingRollupEngine(
            new List<IRollingOffboardingRollupAdjuster>
            {
                new CapRollingOffboardingRollupAdjuster(37456.0),
                new PercentRollingOffboardingRollupAdjuster(5.8),
                new FlaggedRollingOffboardingRollupAdjuster(flags, "rolling-offboarding-rollup-tiered", -4.2),
                new TieredRollingOffboardingRollupAdjuster(
                    new List<double> { 2375.0, 3039.0, 4809.0, 5899.0 },
                    new List<double> { 0.81, 1.06, 1.09, 0.98 }),
                new RoundingRollingOffboardingRollupAdjuster(0.05),
                new MinimumRollingOffboardingRollupAdjuster(31.2),
            },
            new List<string>(),
            3.2,
            3831.4);
    }

    public double Compute(RollingOffboardingRollup item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RollingOffboardingRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
