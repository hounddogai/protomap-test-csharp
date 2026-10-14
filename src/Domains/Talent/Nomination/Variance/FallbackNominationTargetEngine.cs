namespace Acme.Workforce.Talent.Nomination.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback nomination target calculation.</summary>
public interface IFallbackNominationTargetAdjuster
{
    string Describe();
    double Adjust(FallbackNominationTarget item, double valueValue);
}

public sealed record FlaggedFallbackNominationTargetAdjuster(Registry Flags, string Flag, double Percent) : IFallbackNominationTargetAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackNominationTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentFallbackNominationTargetAdjuster(double Percent) : IFallbackNominationTargetAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackNominationTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapFallbackNominationTargetAdjuster(double Cap) : IFallbackNominationTargetAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackNominationTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredFallbackNominationTargetAdjuster(List<double> Bounds, List<double> Rates) : IFallbackNominationTargetAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackNominationTarget item, double valueValue)
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

public sealed record TitleFallbackNominationTargetAdjuster(Dictionary<string, double> Factors) : IFallbackNominationTargetAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(FallbackNominationTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumFallbackNominationTargetAdjuster(double Minimum) : IFallbackNominationTargetAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackNominationTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the fallback nomination target steps in order and keeps a trace of each.</summary>
public sealed class FallbackNominationTargetEngine(List<IFallbackNominationTargetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackNominationTargetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackNominationTargetEngine DefaultFallbackNominationTargetEngine(Registry flags)
    {
        return new FallbackNominationTargetEngine(
            new List<IFallbackNominationTargetAdjuster>
            {
                new FlaggedFallbackNominationTargetAdjuster(flags, "fallback-nomination-target-batched", 4.3),
                new PercentFallbackNominationTargetAdjuster(8.0),
                new CapFallbackNominationTargetAdjuster(27517.0),
                new TieredFallbackNominationTargetAdjuster(
                    new List<double> { 5831.0, 8668.0 },
                    new List<double> { 1.02, 1.2 }),
                new TitleFallbackNominationTargetAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.21, ["retail"] = 0.89, ["west"] = 0.87 }),
                new MinimumFallbackNominationTargetAdjuster(48.7),
            },
            new List<string>(),
            4.6,
            20732.8);
    }

    public double Compute(FallbackNominationTarget item)
    {
        var current = item.WeightedValue();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<FallbackNominationTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
