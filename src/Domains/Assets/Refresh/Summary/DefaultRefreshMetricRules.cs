namespace Acme.Workforce.Assets.Refresh.Summary;

/// <summary>A rule a default refresh metric must pass. Check returns its problems, or none.</summary>
public interface IDefaultRefreshMetricRule
{
    List<string> Check(DefaultRefreshMetric item);
}

public sealed record DefaultRefreshMetricRequiredChecksumRule(string Message) : IDefaultRefreshMetricRule
{
    public List<string> Check(DefaultRefreshMetric item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultRefreshMetricTagsLimitRule(int MaxCount, List<string> Banned) : IDefaultRefreshMetricRule
{
    public List<string> Check(DefaultRefreshMetric item)
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

public sealed record DefaultRefreshMetricRequiredNameRule(string Message) : IDefaultRefreshMetricRule
{
    public List<string> Check(DefaultRefreshMetric item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultRefreshMetricSkillLengthRule(int MaxLength) : IDefaultRefreshMetricRule
{
    public List<string> Check(DefaultRefreshMetric item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultRefreshMetricPinnedGuardRule(int Limit) : IDefaultRefreshMetricRule
{
    public List<string> Check(DefaultRefreshMetric item)
    {
        if (item.Pinned && item.Retries > Limit)
        {
            return new List<string> { $"retries {item.Retries} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default refresh metric.</summary>
public sealed record DefaultRefreshMetricValidator(List<IDefaultRefreshMetricRule> Rules)
{
    public List<string> Validate(DefaultRefreshMetric item)
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

    public void RequireValid(DefaultRefreshMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultRefreshMetric> items)
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

/// <summary>Functions of default refresh metric rules.</summary>
public static class DefaultRefreshMetricRules
{
    public static DefaultRefreshMetricValidator DefaultDefaultRefreshMetricValidator()
    {
        return new DefaultRefreshMetricValidator(
            new List<IDefaultRefreshMetricRule>
            {
                new DefaultRefreshMetricRequiredChecksumRule("default refresh metric needs an entry"),
                new DefaultRefreshMetricTagsLimitRule(255, new List<string> { "legacy", "default", "fallback" }),
                new DefaultRefreshMetricRequiredNameRule("default refresh metric needs an entry"),
                new DefaultRefreshMetricSkillLengthRule(1000),
                new DefaultRefreshMetricPinnedGuardRule(32),
            });
    }
}
