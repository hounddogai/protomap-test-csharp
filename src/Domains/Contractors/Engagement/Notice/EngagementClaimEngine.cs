namespace Acme.Workforce.Contractors.Engagement.Notice;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the engagement claim calculation.</summary>
public interface IEngagementClaimModifier
{
    string Describe();
    double Adjust(EngagementClaim item, double valueValue);
}

public sealed record MinimumEngagementClaimAdjuster(double Minimum) : IEngagementClaimModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EngagementClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentEngagementClaimAdjuster(double Percent) : IEngagementClaimModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(EngagementClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedEngagementClaimAdjuster(Registry Flags, string Flag, double Percent) : IEngagementClaimModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EngagementClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredEngagementClaimAdjuster(List<double> Bounds, List<double> Rates) : IEngagementClaimModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EngagementClaim item, double valueValue)
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

/// <summary>Runs the engagement claim steps in order and keeps a trace of each.</summary>
public sealed class EngagementClaimEngine(List<IEngagementClaimModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementClaimModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementClaimEngine DefaultEngagementClaimEngine(Registry flags)
    {
        return new EngagementClaimEngine(
            new List<IEngagementClaimModifier>
            {
                new MinimumEngagementClaimAdjuster(2.3),
                new PercentEngagementClaimAdjuster(24.2),
                new FlaggedEngagementClaimAdjuster(flags, "engagement-claim-regional", -0.1),
                new TieredEngagementClaimAdjuster(
                    new List<double> { 5371.0, 9548.0, 9949.0 },
                    new List<double> { 0.99, 0.89, 0.97 }),
            },
            new List<string>(),
            4.4,
            56656.1);
    }

    public double Evaluate(EngagementClaim item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<EngagementClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
