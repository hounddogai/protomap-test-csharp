namespace Acme.Workforce.Workplace.AccessBadge.Policy;

/// <summary>A rule a capped access badge threshold must pass. Check returns its problems, or none.</summary>
public interface ICappedAccessBadgeThresholdRule
{
    List<string> Check(CappedAccessBadgeThreshold item);
}

public sealed record CappedAccessBadgeThresholdTitleLengthRule(int MaxLength) : ICappedAccessBadgeThresholdRule
{
    public List<string> Check(CappedAccessBadgeThreshold item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAccessBadgeThresholdCodeLengthRule(int MaxLength) : ICappedAccessBadgeThresholdRule
{
    public List<string> Check(CappedAccessBadgeThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAccessBadgeThresholdGradeLengthRule(int MaxLength) : ICappedAccessBadgeThresholdRule
{
    public List<string> Check(CappedAccessBadgeThreshold item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAccessBadgeThresholdReviewerLengthRule(int MaxLength) : ICappedAccessBadgeThresholdRule
{
    public List<string> Check(CappedAccessBadgeThreshold item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAccessBadgeThresholdRequiredTitleRule(string Message) : ICappedAccessBadgeThresholdRule
{
    public List<string> Check(CappedAccessBadgeThreshold item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedAccessBadgeThresholdCodePrefixRule(string Prefix) : ICappedAccessBadgeThresholdRule
{
    public List<string> Check(CappedAccessBadgeThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped access badge threshold.</summary>
public sealed record CappedAccessBadgeThresholdValidator(List<ICappedAccessBadgeThresholdRule> Rules)
{
    public List<string> Validate(CappedAccessBadgeThreshold item)
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

    public void RequireValid(CappedAccessBadgeThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedAccessBadgeThreshold> items)
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

/// <summary>Functions of capped access badge threshold rules.</summary>
public static class CappedAccessBadgeThresholdRules
{
    public static CappedAccessBadgeThresholdValidator DefaultCappedAccessBadgeThresholdValidator()
    {
        return new CappedAccessBadgeThresholdValidator(
            new List<ICappedAccessBadgeThresholdRule>
            {
                new CappedAccessBadgeThresholdTitleLengthRule(80),
                new CappedAccessBadgeThresholdCodeLengthRule(80),
                new CappedAccessBadgeThresholdGradeLengthRule(1000),
                new CappedAccessBadgeThresholdReviewerLengthRule(16),
                new CappedAccessBadgeThresholdRequiredTitleRule("capped access badge threshold needs this field"),
                new CappedAccessBadgeThresholdCodePrefixRule("OPS"),
            });
    }
}
