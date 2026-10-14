namespace Acme.Workforce.Talent.SkillProfile.Policy;

/// <summary>A rule a skill profile policy must pass. Check returns its problems, or none.</summary>
public interface ISkillProfilePolicyRule
{
    List<string> Check(SkillProfilePolicy item);
}

public sealed record SkillProfilePolicyPriorityRangeRule(int Min, int Max) : ISkillProfilePolicyRule
{
    public List<string> Check(SkillProfilePolicy item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SkillProfilePolicyRequiredContactEmailRule(string Message) : ISkillProfilePolicyRule
{
    public List<string> Check(SkillProfilePolicy item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfilePolicyRequiredCodeRule(string Message) : ISkillProfilePolicyRule
{
    public List<string> Check(SkillProfilePolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfilePolicyCodePrefixRule(string Prefix) : ISkillProfilePolicyRule
{
    public List<string> Check(SkillProfilePolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile policy.</summary>
public sealed record SkillProfilePolicyValidator(List<ISkillProfilePolicyRule> Rules)
{
    public List<string> Validate(SkillProfilePolicy item)
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

    public void RequireValid(SkillProfilePolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfilePolicy> items)
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

/// <summary>Functions of skill profile policy rules.</summary>
public static class SkillProfilePolicyRules
{
    public static SkillProfilePolicyValidator DefaultSkillProfilePolicyValidator()
    {
        return new SkillProfilePolicyValidator(
            new List<ISkillProfilePolicyRule>
            {
                new SkillProfilePolicyPriorityRangeRule(120, 86400),
                new SkillProfilePolicyRequiredContactEmailRule("skill profile policy needs a value"),
                new SkillProfilePolicyRequiredCodeRule("skill profile policy needs this field"),
                new SkillProfilePolicyCodePrefixRule("Q"),
            });
    }
}
