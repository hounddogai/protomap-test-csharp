namespace Acme.Workforce.Contractors.Engagement.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the engagement digest calculation.</summary>
public interface IEngagementDigestModifier
{
    string Describe();
    double Adjust(EngagementDigest item, double valueValue);
}

public sealed record CapEngagementDigestAdjuster(double Cap) : IEngagementDigestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EngagementDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredEngagementDigestAdjuster(List<double> Bounds, List<double> Rates) : IEngagementDigestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EngagementDigest item, double valueValue)
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

public sealed record EngagementDigestRetriesOffsetAdjuster(double Weight) : IEngagementDigestModifier
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(EngagementDigest item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record RoundingEngagementDigestAdjuster(double Step) : IEngagementDigestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EngagementDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentEngagementDigestAdjuster(double Percent) : IEngagementDigestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(EngagementDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedEngagementDigestAdjuster(Registry Flags, string Flag, double Percent) : IEngagementDigestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EngagementDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the engagement digest steps in order and keeps a trace of each.</summary>
public sealed class EngagementDigestEngine(List<IEngagementDigestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementDigestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementDigestEngine DefaultEngagementDigestEngine(Registry flags)
    {
        return new EngagementDigestEngine(
            new List<IEngagementDigestModifier>
            {
                new CapEngagementDigestAdjuster(8320.0),
                new TieredEngagementDigestAdjuster(
                    new List<double> { 5339.0, 7083.0 },
                    new List<double> { 0.94, 1.11 }),
                new EngagementDigestRetriesOffsetAdjuster(0.15),
                new RoundingEngagementDigestAdjuster(0.1),
                new PercentEngagementDigestAdjuster(-8.3),
                new FlaggedEngagementDigestAdjuster(flags, "engagement-digest-lenient", 9.8),
            },
            new List<string>(),
            2.1,
            33210.3);
    }

    public double Evaluate(EngagementDigest item)
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

    public Dictionary<string, double> EvaluateAll(List<EngagementDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
