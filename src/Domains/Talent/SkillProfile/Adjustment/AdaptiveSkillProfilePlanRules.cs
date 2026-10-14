namespace Acme.Workforce.Talent.SkillProfile.Adjustment;

/// <summary>A rule a adaptive skill profile plan must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveSkillProfilePlanRule
{
    List<string> Check(AdaptiveSkillProfilePlan item);
}

public sealed record AdaptiveSkillProfilePlanDescriptionLengthRule(int MaxLength) : IAdaptiveSkillProfilePlanRule
{
    public List<string> Check(AdaptiveSkillProfilePlan item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveSkillProfilePlanDurationMinutesRangeRule(int Min, int Max) : IAdaptiveSkillProfilePlanRule
{
    public List<string> Check(AdaptiveSkillProfilePlan item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveSkillProfilePlanCodePrefixRule(string Prefix) : IAdaptiveSkillProfilePlanRule
{
    public List<string> Check(AdaptiveSkillProfilePlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveSkillProfilePlanNameLengthRule(int MaxLength) : IAdaptiveSkillProfilePlanRule
{
    public List<string> Check(AdaptiveSkillProfilePlan item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveSkillProfilePlanNotesLengthRule(int MaxLength) : IAdaptiveSkillProfilePlanRule
{
    public List<string> Check(AdaptiveSkillProfilePlan item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveSkillProfilePlanCodeLengthRule(int MaxLength) : IAdaptiveSkillProfilePlanRule
{
    public List<string> Check(AdaptiveSkillProfilePlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive skill profile plan.</summary>
public sealed record AdaptiveSkillProfilePlanValidator(List<IAdaptiveSkillProfilePlanRule> Rules)
{
    public List<string> Validate(AdaptiveSkillProfilePlan item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(AdaptiveSkillProfilePlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveSkillProfilePlan> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of adaptive skill profile plan rules.</summary>
public static class AdaptiveSkillProfilePlanRules
{
    public static AdaptiveSkillProfilePlanValidator DefaultAdaptiveSkillProfilePlanValidator()
    {
        return new AdaptiveSkillProfilePlanValidator(
            new List<IAdaptiveSkillProfilePlanRule>
            {
                new AdaptiveSkillProfilePlanDescriptionLengthRule(80),
                new AdaptiveSkillProfilePlanDurationMinutesRangeRule(64, 80),
                new AdaptiveSkillProfilePlanCodePrefixRule("ACM"),
                new AdaptiveSkillProfilePlanNameLengthRule(255),
                new AdaptiveSkillProfilePlanNotesLengthRule(64),
                new AdaptiveSkillProfilePlanCodeLengthRule(3600),
            });
    }
}
