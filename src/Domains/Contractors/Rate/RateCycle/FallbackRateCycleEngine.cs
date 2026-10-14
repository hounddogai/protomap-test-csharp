namespace Acme.Workforce.Contractors.Rate.RateCycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback rate cycle calculation.</summary>
public interface IFallbackRateCycleAdjuster
{
    string Describe();
    double Adjust(FallbackRateCycle item, double valueValue);
}

public sealed record PercentFallbackRateCycleAdjuster(double Percent) : IFallbackRateCycleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackRateCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumFallbackRateCycleAdjuster(double Minimum) : IFallbackRateCycleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackRateCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedFallbackRateCycleAdjuster(Registry Flags, string Flag, double Percent) : IFallbackRateCycleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackRateCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingFallbackRateCycleAdjuster(double Step) : IFallbackRateCycleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackRateCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the fallback rate cycle steps in order and keeps a trace of each.</summary>
public sealed class FallbackRateCycleEngine(List<IFallbackRateCycleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackRateCycleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackRateCycleEngine DefaultFallbackRateCycleEngine(Registry flags)
    {
        return new FallbackRateCycleEngine(
            new List<IFallbackRateCycleAdjuster>
            {
                new PercentFallbackRateCycleAdjuster(-13.7),
                new MinimumFallbackRateCycleAdjuster(20.1),
                new FlaggedFallbackRateCycleAdjuster(flags, "fallback-rate-cycle-default", 7.5),
                new RoundingFallbackRateCycleAdjuster(0.5),
            },
            new List<string>(),
            4.6,
            56528.7);
    }

    public double Evaluate(FallbackRateCycle item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackRateCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
