namespace Acme.Workforce.Talent.SkillProfile.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the skill profile digest calculation.</summary>
public interface ISkillProfileDigestModifier
{
    string Describe();
    double Adjust(SkillProfileDigest item, double valueValue);
}

public sealed record SkillProfileDigestExpiresAtOffsetAdjuster(double Weight) : ISkillProfileDigestModifier
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(SkillProfileDigest item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record TieredSkillProfileDigestAdjuster(List<double> Bounds, List<double> Rates) : ISkillProfileDigestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SkillProfileDigest item, double valueValue)
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

public sealed record FlaggedSkillProfileDigestAdjuster(Registry Flags, string Flag, double Percent) : ISkillProfileDigestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SkillProfileDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the skill profile digest steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileDigestEngine(List<ISkillProfileDigestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileDigestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileDigestEngine DefaultSkillProfileDigestEngine(Registry flags)
    {
        return new SkillProfileDigestEngine(
            new List<ISkillProfileDigestModifier>
            {
                new SkillProfileDigestExpiresAtOffsetAdjuster(0.35),
                new TieredSkillProfileDigestAdjuster(
                    new List<double> { 2378.0, 5632.0, 6673.0 },
                    new List<double> { 1.16, 0.92, 0.91 }),
                new FlaggedSkillProfileDigestAdjuster(flags, "skill-profile-digest-weighted", 5.0),
            },
            new List<string>(),
            4.6,
            11558.3);
    }

    public double Compute(SkillProfileDigest item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
