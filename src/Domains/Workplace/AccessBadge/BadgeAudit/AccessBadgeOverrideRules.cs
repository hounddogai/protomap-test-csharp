namespace Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

/// <summary>A rule a access badge override must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeOverrideRule
{
    List<string> Check(AccessBadgeOverride item);
}

public sealed record AccessBadgeOverrideRequiredReviewerRule(string Message) : IAccessBadgeOverrideRule
{
    public List<string> Check(AccessBadgeOverride item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeOverrideCodeLengthRule(int MaxLength) : IAccessBadgeOverrideRule
{
    public List<string> Check(AccessBadgeOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeOverrideRequiredCodeRule(string Message) : IAccessBadgeOverrideRule
{
    public List<string> Check(AccessBadgeOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeOverrideTitleLengthRule(int MaxLength) : IAccessBadgeOverrideRule
{
    public List<string> Check(AccessBadgeOverride item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge override.</summary>
public sealed record AccessBadgeOverrideValidator(List<IAccessBadgeOverrideRule> Rules)
{
    public List<string> Validate(AccessBadgeOverride item)
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

    public void RequireValid(AccessBadgeOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeOverride> items)
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

/// <summary>Functions of access badge override rules.</summary>
public static class AccessBadgeOverrideRules
{
    public static AccessBadgeOverrideValidator DefaultAccessBadgeOverrideValidator()
    {
        return new AccessBadgeOverrideValidator(
            new List<IAccessBadgeOverrideRule>
            {
                new AccessBadgeOverrideRequiredReviewerRule("access badge override needs this field"),
                new AccessBadgeOverrideCodeLengthRule(86400),
                new AccessBadgeOverrideRequiredCodeRule("access badge override needs this field"),
                new AccessBadgeOverrideTitleLengthRule(86400),
            });
    }
}
