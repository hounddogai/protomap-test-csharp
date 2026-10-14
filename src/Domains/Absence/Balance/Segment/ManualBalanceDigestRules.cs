namespace Acme.Workforce.Absence.Balance.Segment;

/// <summary>A rule a manual balance digest must pass. Check returns its problems, or none.</summary>
public interface IManualBalanceDigestRule
{
    List<string> Check(ManualBalanceDigest item);
}

public sealed record ManualBalanceDigestSegmentLengthRule(int MaxLength) : IManualBalanceDigestRule
{
    public List<string> Check(ManualBalanceDigest item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBalanceDigestTagsLimitRule(int MaxCount, List<string> Banned) : IManualBalanceDigestRule
{
    public List<string> Check(ManualBalanceDigest item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record ManualBalanceDigestRequiredTitleRule(string Message) : IManualBalanceDigestRule
{
    public List<string> Check(ManualBalanceDigest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualBalanceDigestSkillLengthRule(int MaxLength) : IManualBalanceDigestRule
{
    public List<string> Check(ManualBalanceDigest item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBalanceDigestExpiresAtRangeRule(int Min, int Max) : IManualBalanceDigestRule
{
    public List<string> Check(ManualBalanceDigest item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a manual balance digest.</summary>
public sealed record ManualBalanceDigestValidator(List<IManualBalanceDigestRule> Rules)
{
    public List<string> Validate(ManualBalanceDigest item)
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

    public void RequireValid(ManualBalanceDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualBalanceDigest> items)
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

/// <summary>Functions of manual balance digest rules.</summary>
public static class ManualBalanceDigestRules
{
    public static ManualBalanceDigestValidator DefaultManualBalanceDigestValidator()
    {
        return new ManualBalanceDigestValidator(
            new List<IManualBalanceDigestRule>
            {
                new ManualBalanceDigestSegmentLengthRule(255),
                new ManualBalanceDigestTagsLimitRule(64, new List<string> { "batched", "default" }),
                new ManualBalanceDigestRequiredTitleRule("manual balance digest needs an entry"),
                new ManualBalanceDigestSkillLengthRule(32),
                new ManualBalanceDigestExpiresAtRangeRule(64, 64),
            });
    }
}
