namespace Acme.Workforce.Talent.SkillProfile.Proposal;

/// <summary>A rule a skill profile tier must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileTierRule
{
    List<string> Check(SkillProfileTier item);
}

public sealed record SkillProfileTierLeaveDaysRangeRule(int Min, int Max) : ISkillProfileTierRule
{
    public List<string> Check(SkillProfileTier item)
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

public sealed record SkillProfileTierCodePrefixRule(string Prefix) : ISkillProfileTierRule
{
    public List<string> Check(SkillProfileTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileTierRequiredCodeRule(string Message) : ISkillProfileTierRule
{
    public List<string> Check(SkillProfileTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileTierCodeLengthRule(int MaxLength) : ISkillProfileTierRule
{
    public List<string> Check(SkillProfileTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile tier.</summary>
public sealed record SkillProfileTierValidator(List<ISkillProfileTierRule> Rules)
{
    public List<string> Validate(SkillProfileTier item)
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

    public void RequireValid(SkillProfileTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileTier> items)
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

/// <summary>Functions of skill profile tier rules.</summary>
public static class SkillProfileTierRules
{
    public static SkillProfileTierValidator DefaultSkillProfileTierValidator()
    {
        return new SkillProfileTierValidator(
            new List<ISkillProfileTierRule>
            {
                new SkillProfileTierLeaveDaysRangeRule(16, 32),
                new SkillProfileTierCodePrefixRule("ACM"),
                new SkillProfileTierRequiredCodeRule("skill profile tier needs an entry"),
                new SkillProfileTierCodeLengthRule(120),
            });
    }
}
