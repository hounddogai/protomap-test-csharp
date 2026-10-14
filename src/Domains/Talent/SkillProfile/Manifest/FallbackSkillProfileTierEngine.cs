namespace Acme.Workforce.Talent.SkillProfile.Manifest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback skill profile tier calculation.</summary>
public interface IFallbackSkillProfileTierStep
{
    string Describe();
    double Adjust(FallbackSkillProfileTier item, double valueValue);
}

public sealed record TieredFallbackSkillProfileTierAdjuster(List<double> Bounds, List<double> Rates) : IFallbackSkillProfileTierStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackSkillProfileTier item, double valueValue)
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

public sealed record FlaggedFallbackSkillProfileTierAdjuster(Registry Flags, string Flag, double Percent) : IFallbackSkillProfileTierStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackSkillProfileTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingFallbackSkillProfileTierAdjuster(double Step) : IFallbackSkillProfileTierStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackSkillProfileTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the fallback skill profile tier steps in order and keeps a trace of each.</summary>
public sealed class FallbackSkillProfileTierEngine(List<IFallbackSkillProfileTierStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackSkillProfileTierStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackSkillProfileTierEngine DefaultFallbackSkillProfileTierEngine(Registry flags)
    {
        return new FallbackSkillProfileTierEngine(
            new List<IFallbackSkillProfileTierStep>
            {
                new TieredFallbackSkillProfileTierAdjuster(
                    new List<double> { 235.0, 5039.0 },
                    new List<double> { 1.16, 0.98 }),
                new FlaggedFallbackSkillProfileTierAdjuster(flags, "fallback-skill-profile-tier-adaptive", -9.5),
                new RoundingFallbackSkillProfileTierAdjuster(0.05),
            },
            new List<string>(),
            2.6,
            89696.2);
    }

    public double Run(FallbackSkillProfileTier item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackSkillProfileTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
