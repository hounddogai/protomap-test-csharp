namespace Acme.Workforce.Talent.SkillProfile.Policy;

/// <summary>A rule a skill profile group must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileGroupRule
{
    List<string> Check(SkillProfileGroup item);
}

public sealed record SkillProfileGroupLimitRangeRule(int Min, int Max) : ISkillProfileGroupRule
{
    public List<string> Check(SkillProfileGroup item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SkillProfileGroupCodePrefixRule(string Prefix) : ISkillProfileGroupRule
{
    public List<string> Check(SkillProfileGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileGroupReviewerLengthRule(int MaxLength) : ISkillProfileGroupRule
{
    public List<string> Check(SkillProfileGroup item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile group.</summary>
public sealed record SkillProfileGroupValidator(List<ISkillProfileGroupRule> Rules)
{
    public List<string> Validate(SkillProfileGroup item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(SkillProfileGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileGroup> items)
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

/// <summary>Functions of skill profile group rules.</summary>
public static class SkillProfileGroupRules
{
    public static SkillProfileGroupValidator DefaultSkillProfileGroupValidator()
    {
        return new SkillProfileGroupValidator(
            new List<ISkillProfileGroupRule>
            {
                new SkillProfileGroupLimitRangeRule(32, 64),
                new SkillProfileGroupCodePrefixRule("Q"),
                new SkillProfileGroupReviewerLengthRule(16),
            });
    }
}
