namespace Acme.Workforce.Contractors.Engagement.Tier;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the engagement entry calculation.</summary>
public interface IEngagementEntryFactor
{
    string Describe();
    double Adjust(EngagementEntry item, double valueValue);
}

public sealed record FlaggedEngagementEntryAdjuster(Registry Flags, string Flag, double Percent) : IEngagementEntryFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EngagementEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TeamEngagementEntryAdjuster(Dictionary<string, double> Factors) : IEngagementEntryFactor
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(EngagementEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapEngagementEntryAdjuster(double Cap) : IEngagementEntryFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EngagementEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredEngagementEntryAdjuster(List<double> Bounds, List<double> Rates) : IEngagementEntryFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EngagementEntry item, double valueValue)
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

/// <summary>Runs the engagement entry steps in order and keeps a trace of each.</summary>
public sealed class EngagementEntryEngine(List<IEngagementEntryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementEntryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementEntryEngine DefaultEngagementEntryEngine(Registry flags)
    {
        return new EngagementEntryEngine(
            new List<IEngagementEntryFactor>
            {
                new FlaggedEngagementEntryAdjuster(flags, "engagement-entry-regional", 7.7),
                new TeamEngagementEntryAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.03, ["retail"] = 1.38, ["online"] = 0.84 }),
                new CapEngagementEntryAdjuster(28073.0),
                new TieredEngagementEntryAdjuster(
                    new List<double> { 2579.0, 9707.0 },
                    new List<double> { 1.2, 0.93 }),
            },
            new List<string>(),
            2.1,
            19365.8);
    }

    public double Run(EngagementEntry item)
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

    public Dictionary<string, double> EvaluateAll(List<EngagementEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
