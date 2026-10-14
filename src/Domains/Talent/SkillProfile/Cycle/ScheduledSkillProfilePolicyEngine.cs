namespace Acme.Workforce.Talent.SkillProfile.Cycle;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the scheduled skill profile policy calculation.</summary>
public interface IScheduledSkillProfilePolicyStep
{
    string Describe();
    double Adjust(ScheduledSkillProfilePolicy item, double valueValue);
}

public sealed record ScheduledSkillProfilePolicyMaximumOffsetAdjuster(double Weight) : IScheduledSkillProfilePolicyStep
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(ScheduledSkillProfilePolicy item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record PercentScheduledSkillProfilePolicyAdjuster(double Percent) : IScheduledSkillProfilePolicyStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledSkillProfilePolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedScheduledSkillProfilePolicyAdjuster(Registry Flags, string Flag, double Percent) : IScheduledSkillProfilePolicyStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledSkillProfilePolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled skill profile policy steps in order and keeps a trace of each.</summary>
public sealed class ScheduledSkillProfilePolicyEngine(List<IScheduledSkillProfilePolicyStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledSkillProfilePolicyStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledSkillProfilePolicyEngine DefaultScheduledSkillProfilePolicyEngine(Registry flags)
    {
        return new ScheduledSkillProfilePolicyEngine(
            new List<IScheduledSkillProfilePolicyStep>
            {
                new ScheduledSkillProfilePolicyMaximumOffsetAdjuster(0.5),
                new PercentScheduledSkillProfilePolicyAdjuster(-5.1),
                new FlaggedScheduledSkillProfilePolicyAdjuster(flags, "scheduled-skill-profile-policy-strict", 1.4),
            },
            new List<string>(),
            2.0,
            98584.9);
    }

    public double Compute(ScheduledSkillProfilePolicy item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledSkillProfilePolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
