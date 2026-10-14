namespace Acme.Workforce.Contractors.Rate.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rate entry calculation.</summary>
public interface IRateEntryFactor
{
    string Describe();
    double Adjust(RateEntry item, double valueValue);
}

public sealed record FlaggedRateEntryAdjuster(Registry Flags, string Flag, double Percent) : IRateEntryFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RateEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRateEntryAdjuster(List<double> Bounds, List<double> Rates) : IRateEntryFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RateEntry item, double valueValue)
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

public sealed record RoundingRateEntryAdjuster(double Step) : IRateEntryFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RateEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapRateEntryAdjuster(double Cap) : IRateEntryFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RateEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TeamRateEntryAdjuster(Dictionary<string, double> Factors) : IRateEntryFactor
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(RateEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentRateEntryAdjuster(double Percent) : IRateEntryFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RateEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rate entry steps in order and keeps a trace of each.</summary>
public sealed class RateEntryEngine(List<IRateEntryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRateEntryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RateEntryEngine DefaultRateEntryEngine(Registry flags)
    {
        return new RateEntryEngine(
            new List<IRateEntryFactor>
            {
                new FlaggedRateEntryAdjuster(flags, "rate-entry-tiered", 7.0),
                new TieredRateEntryAdjuster(new List<double> { 7208.0, 7289.0 }, new List<double> { 1.12, 0.88 }),
                new RoundingRateEntryAdjuster(5.0),
                new CapRateEntryAdjuster(13383.0),
                new TeamRateEntryAdjuster(new Dictionary<string, double> { ["central"] = 0.8 }),
                new PercentRateEntryAdjuster(-10.0),
            },
            new List<string>(),
            2.9,
            30415.8);
    }

    public double Run(RateEntry item)
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

    public Dictionary<string, double> EvaluateAll(List<RateEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
