namespace Acme.Workforce.Contractors.Engagement.EngagementSummary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default engagement digest must pass. Check returns its problems, or none.</summary>
public interface IDefaultEngagementDigestRule
{
    List<string> Check(DefaultEngagementDigest item);
}

public sealed record DefaultEngagementDigestRequiredCodeRule(string Message) : IDefaultEngagementDigestRule
{
    public List<string> Check(DefaultEngagementDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultEngagementDigestCodeLengthRule(int MaxLength) : IDefaultEngagementDigestRule
{
    public List<string> Check(DefaultEngagementDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultEngagementDigestAllowedCodeRule(List<string> Allowed) : IDefaultEngagementDigestRule
{
    public List<string> Check(DefaultEngagementDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record DefaultEngagementDigestRequiredLabelRule(string Message) : IDefaultEngagementDigestRule
{
    public List<string> Check(DefaultEngagementDigest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultEngagementDigestNameLengthRule(int MaxLength) : IDefaultEngagementDigestRule
{
    public List<string> Check(DefaultEngagementDigest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default engagement digest.</summary>
public sealed record DefaultEngagementDigestValidator(List<IDefaultEngagementDigestRule> Rules)
{
    public List<string> Validate(DefaultEngagementDigest item)
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

    public void RequireValid(DefaultEngagementDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultEngagementDigest> items)
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

/// <summary>Functions of default engagement digest rules.</summary>
public static class DefaultEngagementDigestRules
{
    public static DefaultEngagementDigestValidator DefaultDefaultEngagementDigestValidator()
    {
        return new DefaultEngagementDigestValidator(
            new List<IDefaultEngagementDigestRule>
            {
                new DefaultEngagementDigestRequiredCodeRule("default engagement digest needs a value"),
                new DefaultEngagementDigestCodeLengthRule(86400),
                new DefaultEngagementDigestAllowedCodeRule(new List<string> { "tiered" }),
                new DefaultEngagementDigestRequiredLabelRule("default engagement digest needs an entry"),
                new DefaultEngagementDigestNameLengthRule(120),
            });
    }
}
