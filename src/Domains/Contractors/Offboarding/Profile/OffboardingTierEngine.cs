namespace Acme.Workforce.Contractors.Offboarding.Profile;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the offboarding tier calculation.</summary>
public interface IOffboardingTierFactor
{
    string Describe();
    double Adjust(OffboardingTier item, double valueValue);
}

public sealed record FlaggedOffboardingTierAdjuster(Registry Flags, string Flag, double Percent) : IOffboardingTierFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OffboardingTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelOffboardingTierAdjuster(Dictionary<string, double> Factors) : IOffboardingTierFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(OffboardingTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumOffboardingTierAdjuster(double Minimum) : IOffboardingTierFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OffboardingTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapOffboardingTierAdjuster(double Cap) : IOffboardingTierFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OffboardingTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the offboarding tier steps in order and keeps a trace of each.</summary>
public sealed class OffboardingTierEngine(List<IOffboardingTierFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOffboardingTierFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OffboardingTierEngine DefaultOffboardingTierEngine(Registry flags)
    {
        return new OffboardingTierEngine(
            new List<IOffboardingTierFactor>
            {
                new FlaggedOffboardingTierAdjuster(flags, "offboarding-tier-primary", 0.1),
                new LabelOffboardingTierAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.36, ["west"] = 1.05 }),
                new MinimumOffboardingTierAdjuster(25.5),
                new CapOffboardingTierAdjuster(24548.0),
            },
            new List<string>(),
            3.4,
            87373.8);
    }

    public double Run(OffboardingTier item)
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

    public Dictionary<string, double> EvaluateAll(List<OffboardingTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
