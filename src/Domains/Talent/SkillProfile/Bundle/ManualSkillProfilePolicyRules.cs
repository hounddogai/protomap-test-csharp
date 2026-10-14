namespace Acme.Workforce.Talent.SkillProfile.Bundle;

/// <summary>A rule a manual skill profile policy must pass. Check returns its problems, or none.</summary>
public interface IManualSkillProfilePolicyRule
{
    List<string> Check(ManualSkillProfilePolicy item);
}

public sealed record ManualSkillProfilePolicyUpdatedAtRangeRule(int Min, int Max) : IManualSkillProfilePolicyRule
{
    public List<string> Check(ManualSkillProfilePolicy item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualSkillProfilePolicyRequiredManagerIdRule(string Message) : IManualSkillProfilePolicyRule
{
    public List<string> Check(ManualSkillProfilePolicy item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualSkillProfilePolicyRequiredTitleRule(string Message) : IManualSkillProfilePolicyRule
{
    public List<string> Check(ManualSkillProfilePolicy item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual skill profile policy.</summary>
public sealed record ManualSkillProfilePolicyValidator(List<IManualSkillProfilePolicyRule> Rules)
{
    public List<string> Validate(ManualSkillProfilePolicy item)
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

    public void RequireValid(ManualSkillProfilePolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualSkillProfilePolicy> items)
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

/// <summary>Functions of manual skill profile policy rules.</summary>
public static class ManualSkillProfilePolicyRules
{
    public static ManualSkillProfilePolicyValidator DefaultManualSkillProfilePolicyValidator()
    {
        return new ManualSkillProfilePolicyValidator(
            new List<IManualSkillProfilePolicyRule>
            {
                new ManualSkillProfilePolicyUpdatedAtRangeRule(32, 64),
                new ManualSkillProfilePolicyRequiredManagerIdRule("manual skill profile policy needs an entry"),
                new ManualSkillProfilePolicyRequiredTitleRule("manual skill profile policy needs an entry"),
            });
    }
}
