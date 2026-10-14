namespace Acme.Workforce.Compliance.Exception.ExceptionGroup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback exception tier calculation.</summary>
public interface IFallbackExceptionTierAdjuster
{
    string Describe();
    double Adjust(FallbackExceptionTier item, double valueValue);
}

public sealed record TieredFallbackExceptionTierAdjuster(List<double> Bounds, List<double> Rates) : IFallbackExceptionTierAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackExceptionTier item, double valueValue)
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

public sealed record FlaggedFallbackExceptionTierAdjuster(Registry Flags, string Flag, double Percent) : IFallbackExceptionTierAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackExceptionTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CategoryFallbackExceptionTierAdjuster(Dictionary<string, double> Factors) : IFallbackExceptionTierAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(FallbackExceptionTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingFallbackExceptionTierAdjuster(double Step) : IFallbackExceptionTierAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackExceptionTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentFallbackExceptionTierAdjuster(double Percent) : IFallbackExceptionTierAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackExceptionTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FallbackExceptionTierAttemptsOffsetAdjuster(double Weight) : IFallbackExceptionTierAdjuster
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(FallbackExceptionTier item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

/// <summary>Runs the fallback exception tier steps in order and keeps a trace of each.</summary>
public sealed class FallbackExceptionTierEngine(List<IFallbackExceptionTierAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackExceptionTierAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackExceptionTierEngine DefaultFallbackExceptionTierEngine(Registry flags)
    {
        return new FallbackExceptionTierEngine(
            new List<IFallbackExceptionTierAdjuster>
            {
                new TieredFallbackExceptionTierAdjuster(
                    new List<double> { 2511.0, 8909.0 },
                    new List<double> { 0.84, 1.04 }),
                new FlaggedFallbackExceptionTierAdjuster(flags, "fallback-exception-tier-scheduled", -9.7),
                new CategoryFallbackExceptionTierAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.28, ["online"] = 0.86, ["central"] = 0.79 }),
                new RoundingFallbackExceptionTierAdjuster(1.0),
                new PercentFallbackExceptionTierAdjuster(-19.5),
                new FallbackExceptionTierAttemptsOffsetAdjuster(1.02),
            },
            new List<string>(),
            3.6,
            78881.9);
    }

    public double Run(FallbackExceptionTier item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackExceptionTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
