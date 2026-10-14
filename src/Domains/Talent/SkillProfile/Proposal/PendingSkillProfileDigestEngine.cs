namespace Acme.Workforce.Talent.SkillProfile.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending skill profile digest calculation.</summary>
public interface IPendingSkillProfileDigestFactor
{
    string Describe();
    double Adjust(PendingSkillProfileDigest item, double valueValue);
}

public sealed record RoundingPendingSkillProfileDigestAdjuster(double Step) : IPendingSkillProfileDigestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingSkillProfileDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NamePendingSkillProfileDigestAdjuster(Dictionary<string, double> Factors) : IPendingSkillProfileDigestFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PendingSkillProfileDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPendingSkillProfileDigestAdjuster(double Percent) : IPendingSkillProfileDigestFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingSkillProfileDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPendingSkillProfileDigestAdjuster(Registry Flags, string Flag, double Percent) : IPendingSkillProfileDigestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingSkillProfileDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPendingSkillProfileDigestAdjuster(double Cap) : IPendingSkillProfileDigestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingSkillProfileDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPendingSkillProfileDigestAdjuster(List<double> Bounds, List<double> Rates) : IPendingSkillProfileDigestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingSkillProfileDigest item, double valueValue)
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

/// <summary>Runs the pending skill profile digest steps in order and keeps a trace of each.</summary>
public sealed class PendingSkillProfileDigestEngine(List<IPendingSkillProfileDigestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingSkillProfileDigestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingSkillProfileDigestEngine DefaultPendingSkillProfileDigestEngine(Registry flags)
    {
        return new PendingSkillProfileDigestEngine(
            new List<IPendingSkillProfileDigestFactor>
            {
                new RoundingPendingSkillProfileDigestAdjuster(0.05),
                new NamePendingSkillProfileDigestAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.13, ["online"] = 1.35, ["south"] = 0.73 }),
                new PercentPendingSkillProfileDigestAdjuster(-5.8),
                new FlaggedPendingSkillProfileDigestAdjuster(flags, "pending-skill-profile-digest-seasonal", -0.4),
                new CapPendingSkillProfileDigestAdjuster(22709.0),
                new TieredPendingSkillProfileDigestAdjuster(
                    new List<double> { 421.0, 5978.0, 7822.0, 9305.0 },
                    new List<double> { 1.18, 0.87, 0.81, 1.18 }),
            },
            new List<string>(),
            3.6,
            20420.4);
    }

    public double Run(PendingSkillProfileDigest item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PendingSkillProfileDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
