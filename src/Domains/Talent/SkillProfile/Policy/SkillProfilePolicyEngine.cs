namespace Acme.Workforce.Talent.SkillProfile.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the skill profile policy calculation.</summary>
public interface ISkillProfilePolicyModifier
{
    string Describe();
    double Adjust(SkillProfilePolicy item, double valueValue);
}

public sealed record TieredSkillProfilePolicyAdjuster(List<double> Bounds, List<double> Rates) : ISkillProfilePolicyModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SkillProfilePolicy item, double valueValue)
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

public sealed record MinimumSkillProfilePolicyAdjuster(double Minimum) : ISkillProfilePolicyModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SkillProfilePolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingSkillProfilePolicyAdjuster(double Step) : ISkillProfilePolicyModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SkillProfilePolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedSkillProfilePolicyAdjuster(Registry Flags, string Flag, double Percent) : ISkillProfilePolicyModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SkillProfilePolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentSkillProfilePolicyAdjuster(double Percent) : ISkillProfilePolicyModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SkillProfilePolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the skill profile policy steps in order and keeps a trace of each.</summary>
public sealed class SkillProfilePolicyEngine(List<ISkillProfilePolicyModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfilePolicyModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfilePolicyEngine DefaultSkillProfilePolicyEngine(Registry flags)
    {
        return new SkillProfilePolicyEngine(
            new List<ISkillProfilePolicyModifier>
            {
                new TieredSkillProfilePolicyAdjuster(
                    new List<double> { 1221.0, 2185.0 },
                    new List<double> { 0.88, 0.88 }),
                new MinimumSkillProfilePolicyAdjuster(27.0),
                new RoundingSkillProfilePolicyAdjuster(5.0),
                new FlaggedSkillProfilePolicyAdjuster(flags, "skill-profile-policy-seasonal", -1.7),
                new PercentSkillProfilePolicyAdjuster(-10.8),
            },
            new List<string>(),
            3.1,
            17503.8);
    }

    public double Compute(SkillProfilePolicy item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfilePolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
