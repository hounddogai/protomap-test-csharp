namespace Acme.Workforce.Workplace.AccessBadge.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a access badge notice must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeNoticeRule
{
    List<string> Check(AccessBadgeNotice item);
}

public sealed record AccessBadgeNoticeSkillLengthRule(int MaxLength) : IAccessBadgeNoticeRule
{
    public List<string> Check(AccessBadgeNotice item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeNoticeRequiredTitleRule(string Message) : IAccessBadgeNoticeRule
{
    public List<string> Check(AccessBadgeNotice item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeNoticeCurrencyLengthRule(int MaxLength) : IAccessBadgeNoticeRule
{
    public List<string> Check(AccessBadgeNotice item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeNoticeRequiredCurrencyRule(string Message) : IAccessBadgeNoticeRule
{
    public List<string> Check(AccessBadgeNotice item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeNoticeRequiredCodeRule(string Message) : IAccessBadgeNoticeRule
{
    public List<string> Check(AccessBadgeNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeNoticeAllowedTitleRule(List<string> Allowed) : IAccessBadgeNoticeRule
{
    public List<string> Check(AccessBadgeNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge notice.</summary>
public sealed record AccessBadgeNoticeValidator(List<IAccessBadgeNoticeRule> Rules)
{
    public List<string> Validate(AccessBadgeNotice item)
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

    public void RequireValid(AccessBadgeNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeNotice> items)
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

/// <summary>Functions of access badge notice rules.</summary>
public static class AccessBadgeNoticeRules
{
    public static AccessBadgeNoticeValidator DefaultAccessBadgeNoticeValidator()
    {
        return new AccessBadgeNoticeValidator(
            new List<IAccessBadgeNoticeRule>
            {
                new AccessBadgeNoticeSkillLengthRule(64),
                new AccessBadgeNoticeRequiredTitleRule("access badge notice needs this field"),
                new AccessBadgeNoticeCurrencyLengthRule(80),
                new AccessBadgeNoticeRequiredCurrencyRule("access badge notice needs this field"),
                new AccessBadgeNoticeRequiredCodeRule("access badge notice needs this field"),
                new AccessBadgeNoticeAllowedTitleRule(new List<string> { "manual" }),
            });
    }
}
