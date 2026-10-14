namespace Acme.Workforce.Contractors.Engagement.EngagementProfile;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the engagement override calculation.</summary>
public interface IEngagementOverrideStep
{
    string Describe();
    double Adjust(EngagementOverride item, double valueValue);
}

public sealed record CapEngagementOverrideAdjuster(double Cap) : IEngagementOverrideStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EngagementOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedEngagementOverrideAdjuster(Registry Flags, string Flag, double Percent) : IEngagementOverrideStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EngagementOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredEngagementOverrideAdjuster(List<double> Bounds, List<double> Rates) : IEngagementOverrideStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EngagementOverride item, double valueValue)
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

/// <summary>Runs the engagement override steps in order and keeps a trace of each.</summary>
public sealed class EngagementOverrideEngine(List<IEngagementOverrideStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementOverrideStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementOverrideEngine DefaultEngagementOverrideEngine(Registry flags)
    {
        return new EngagementOverrideEngine(
            new List<IEngagementOverrideStep>
            {
                new CapEngagementOverrideAdjuster(829.0),
                new FlaggedEngagementOverrideAdjuster(flags, "engagement-override-legacy", 5.8),
                new TieredEngagementOverrideAdjuster(
                    new List<double> { 4461.0, 7455.0, 7956.0, 9303.0 },
                    new List<double> { 1.03, 1.15, 0.83, 0.81 }),
            },
            new List<string>(),
            1.4,
            14526.5);
    }

    public double Price(EngagementOverride item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<EngagementOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
