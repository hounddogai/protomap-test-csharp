namespace Acme.Workforce.Contractors.Rate.RateCycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling rate bundle calculation.</summary>
public interface IRollingRateBundleAdjuster
{
    string Describe();
    double Adjust(RollingRateBundle item, double valueValue);
}

public sealed record RollingRateBundleCeilingOffsetAdjuster(double Weight) : IRollingRateBundleAdjuster
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(RollingRateBundle item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record FlaggedRollingRateBundleAdjuster(Registry Flags, string Flag, double Percent) : IRollingRateBundleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingRateBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRollingRateBundleAdjuster(double Step) : IRollingRateBundleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingRateBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapRollingRateBundleAdjuster(double Cap) : IRollingRateBundleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingRateBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the rolling rate bundle steps in order and keeps a trace of each.</summary>
public sealed class RollingRateBundleEngine(List<IRollingRateBundleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingRateBundleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingRateBundleEngine DefaultRollingRateBundleEngine(Registry flags)
    {
        return new RollingRateBundleEngine(
            new List<IRollingRateBundleAdjuster>
            {
                new RollingRateBundleCeilingOffsetAdjuster(0.24),
                new FlaggedRollingRateBundleAdjuster(flags, "rolling-rate-bundle-global", 9.0),
                new RoundingRollingRateBundleAdjuster(0.5),
                new CapRollingRateBundleAdjuster(41601.0),
            },
            new List<string>(),
            3.7,
            88734.0);
    }

    public double Price(RollingRateBundle item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RollingRateBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
