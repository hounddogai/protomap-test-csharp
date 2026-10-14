namespace Acme.Workforce.Contractors.Engagement.EngagementSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default engagement score must pass. Check returns its problems, or none.</summary>
public interface IDefaultEngagementScoreRule
{
    List<string> Check(DefaultEngagementScore item);
}

public sealed record DefaultEngagementScoreCodePrefixRule(string Prefix) : IDefaultEngagementScoreRule
{
    public List<string> Check(DefaultEngagementScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultEngagementScoreRequiredSegmentRule(string Message) : IDefaultEngagementScoreRule
{
    public List<string> Check(DefaultEngagementScore item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultEngagementScoreRequiredCodeRule(string Message) : IDefaultEngagementScoreRule
{
    public List<string> Check(DefaultEngagementScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultEngagementScoreAllowedCodeRule(List<string> Allowed) : IDefaultEngagementScoreRule
{
    public List<string> Check(DefaultEngagementScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default engagement score.</summary>
public sealed record DefaultEngagementScoreValidator(List<IDefaultEngagementScoreRule> Rules)
{
    public List<string> Validate(DefaultEngagementScore item)
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

    public void RequireValid(DefaultEngagementScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultEngagementScore> items)
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

/// <summary>Functions of default engagement score rules.</summary>
public static class DefaultEngagementScoreRules
{
    public static DefaultEngagementScoreValidator DefaultDefaultEngagementScoreValidator()
    {
        return new DefaultEngagementScoreValidator(
            new List<IDefaultEngagementScoreRule>
            {
                new DefaultEngagementScoreCodePrefixRule("ACM"),
                new DefaultEngagementScoreRequiredSegmentRule("default engagement score needs a value"),
                new DefaultEngagementScoreRequiredCodeRule("default engagement score needs an entry"),
                new DefaultEngagementScoreAllowedCodeRule(new List<string> { "rolling" }),
            });
    }
}
