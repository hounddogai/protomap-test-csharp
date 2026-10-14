namespace Acme.Workforce.Talent.SkillProfile.Bundle;

/// <summary>One step of the manual skill profile policy calculation.</summary>
public interface IManualSkillProfilePolicyModifier
{
    string Describe();
    double Adjust(ManualSkillProfilePolicy item, double valueValue);
}

public sealed record PercentManualSkillProfilePolicyAdjuster(double Percent) : IManualSkillProfilePolicyModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualSkillProfilePolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapManualSkillProfilePolicyAdjuster(double Cap) : IManualSkillProfilePolicyModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualSkillProfilePolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ManualSkillProfilePolicyUpdatedAtOffsetAdjuster(double Weight) : IManualSkillProfilePolicyModifier
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(ManualSkillProfilePolicy item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

/// <summary>Runs the manual skill profile policy steps in order and keeps a trace of each.</summary>
public sealed class ManualSkillProfilePolicyEngine(List<IManualSkillProfilePolicyModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualSkillProfilePolicyModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualSkillProfilePolicyEngine DefaultManualSkillProfilePolicyEngine()
    {
        return new ManualSkillProfilePolicyEngine(
            new List<IManualSkillProfilePolicyModifier>
            {
                new PercentManualSkillProfilePolicyAdjuster(5.5),
                new CapManualSkillProfilePolicyAdjuster(9607.0),
                new ManualSkillProfilePolicyUpdatedAtOffsetAdjuster(1.52),
            },
            new List<string>(),
            4.4,
            73106.4);
    }

    public double Price(ManualSkillProfilePolicy item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ManualSkillProfilePolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
