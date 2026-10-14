namespace Acme.Workforce.Talent.SkillProfile.Threshold;

/// <summary>A rule a default skill profile policy must pass. Check returns its problems, or none.</summary>
public interface IDefaultSkillProfilePolicyRule
{
    List<string> Check(DefaultSkillProfilePolicy item);
}

public sealed record DefaultSkillProfilePolicyLeaveDaysRangeRule(int Min, int Max) : IDefaultSkillProfilePolicyRule
{
    public List<string> Check(DefaultSkillProfilePolicy item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultSkillProfilePolicyAttemptsRangeRule(int Min, int Max) : IDefaultSkillProfilePolicyRule
{
    public List<string> Check(DefaultSkillProfilePolicy item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultSkillProfilePolicyCodePrefixRule(string Prefix) : IDefaultSkillProfilePolicyRule
{
    public List<string> Check(DefaultSkillProfilePolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default skill profile policy.</summary>
public sealed record DefaultSkillProfilePolicyValidator(List<IDefaultSkillProfilePolicyRule> Rules)
{
    public List<string> Validate(DefaultSkillProfilePolicy item)
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

    public void RequireValid(DefaultSkillProfilePolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultSkillProfilePolicy> items)
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

/// <summary>Functions of default skill profile policy rules.</summary>
public static class DefaultSkillProfilePolicyRules
{
    public static DefaultSkillProfilePolicyValidator DefaultDefaultSkillProfilePolicyValidator()
    {
        return new DefaultSkillProfilePolicyValidator(
            new List<IDefaultSkillProfilePolicyRule>
            {
                new DefaultSkillProfilePolicyLeaveDaysRangeRule(1000, 86400),
                new DefaultSkillProfilePolicyAttemptsRangeRule(32, 64),
                new DefaultSkillProfilePolicyCodePrefixRule("Q"),
            });
    }
}
