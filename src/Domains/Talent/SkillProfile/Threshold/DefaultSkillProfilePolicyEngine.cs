namespace Acme.Workforce.Talent.SkillProfile.Threshold;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the default skill profile policy calculation.</summary>
public interface IDefaultSkillProfilePolicyFactor
{
    string Describe();
    double Adjust(DefaultSkillProfilePolicy item, double valueValue);
}

public sealed record CapDefaultSkillProfilePolicyAdjuster(double Cap) : IDefaultSkillProfilePolicyFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultSkillProfilePolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedDefaultSkillProfilePolicyAdjuster(Registry Flags, string Flag, double Percent) : IDefaultSkillProfilePolicyFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultSkillProfilePolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumDefaultSkillProfilePolicyAdjuster(double Minimum) : IDefaultSkillProfilePolicyFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultSkillProfilePolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the default skill profile policy steps in order and keeps a trace of each.</summary>
public sealed class DefaultSkillProfilePolicyEngine(List<IDefaultSkillProfilePolicyFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultSkillProfilePolicyFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultSkillProfilePolicyEngine DefaultDefaultSkillProfilePolicyEngine(Registry flags)
    {
        return new DefaultSkillProfilePolicyEngine(
            new List<IDefaultSkillProfilePolicyFactor>
            {
                new CapDefaultSkillProfilePolicyAdjuster(4006.0),
                new FlaggedDefaultSkillProfilePolicyAdjuster(flags, "default-skill-profile-policy-adaptive", 8.2),
                new MinimumDefaultSkillProfilePolicyAdjuster(30.0),
            },
            new List<string>(),
            1.8,
            31275.7);
    }

    public double Price(DefaultSkillProfilePolicy item)
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

    public Dictionary<string, double> EvaluateAll(List<DefaultSkillProfilePolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
