namespace Acme.Workforce.Talent.Pool.PoolAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback pool draft calculation.</summary>
public interface IFallbackPoolDraftFactor
{
    string Describe();
    double Adjust(FallbackPoolDraft item, double valueValue);
}

public sealed record RoundingFallbackPoolDraftAdjuster(double Step) : IFallbackPoolDraftFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackPoolDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LocaleFallbackPoolDraftAdjuster(Dictionary<string, double> Factors) : IFallbackPoolDraftFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(FallbackPoolDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedFallbackPoolDraftAdjuster(Registry Flags, string Flag, double Percent) : IFallbackPoolDraftFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackPoolDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentFallbackPoolDraftAdjuster(double Percent) : IFallbackPoolDraftFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackPoolDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumFallbackPoolDraftAdjuster(double Minimum) : IFallbackPoolDraftFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackPoolDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FallbackPoolDraftCeilingOffsetAdjuster(double Weight) : IFallbackPoolDraftFactor
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(FallbackPoolDraft item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

/// <summary>Runs the fallback pool draft steps in order and keeps a trace of each.</summary>
public sealed class FallbackPoolDraftEngine(List<IFallbackPoolDraftFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackPoolDraftFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackPoolDraftEngine DefaultFallbackPoolDraftEngine(Registry flags)
    {
        return new FallbackPoolDraftEngine(
            new List<IFallbackPoolDraftFactor>
            {
                new RoundingFallbackPoolDraftAdjuster(0.25),
                new LocaleFallbackPoolDraftAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.82, ["online"] = 0.85, ["west"] = 1.05 }),
                new FlaggedFallbackPoolDraftAdjuster(flags, "fallback-pool-draft-lenient", 7.6),
                new PercentFallbackPoolDraftAdjuster(13.0),
                new MinimumFallbackPoolDraftAdjuster(30.4),
                new FallbackPoolDraftCeilingOffsetAdjuster(1.07),
            },
            new List<string>(),
            3.9,
            82915.1);
    }

    public double Price(FallbackPoolDraft item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackPoolDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
