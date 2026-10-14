namespace Acme.Workforce.Workplace.AccessBadge.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a access badge review must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeReviewRule
{
    List<string> Check(AccessBadgeReview item);
}

public sealed record AccessBadgeReviewCodeLengthRule(int MaxLength) : IAccessBadgeReviewRule
{
    public List<string> Check(AccessBadgeReview item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeReviewAttemptsRangeRule(int Min, int Max) : IAccessBadgeReviewRule
{
    public List<string> Check(AccessBadgeReview item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeReviewLimitRangeRule(int Min, int Max) : IAccessBadgeReviewRule
{
    public List<string> Check(AccessBadgeReview item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeReviewAllowedSkillRule(List<string> Allowed) : IAccessBadgeReviewRule
{
    public List<string> Check(AccessBadgeReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge review.</summary>
public sealed record AccessBadgeReviewValidator(List<IAccessBadgeReviewRule> Rules)
{
    public List<string> Validate(AccessBadgeReview item)
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

    public void RequireValid(AccessBadgeReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeReview> items)
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

/// <summary>Functions of access badge review rules.</summary>
public static class AccessBadgeReviewRules
{
    public static AccessBadgeReviewValidator DefaultAccessBadgeReviewValidator()
    {
        return new AccessBadgeReviewValidator(
            new List<IAccessBadgeReviewRule>
            {
                new AccessBadgeReviewCodeLengthRule(120),
                new AccessBadgeReviewAttemptsRangeRule(255, 86400),
                new AccessBadgeReviewLimitRangeRule(1000, 1000),
                new AccessBadgeReviewAllowedSkillRule(new List<string> { "lenient", "manual" }),
            });
    }
}
