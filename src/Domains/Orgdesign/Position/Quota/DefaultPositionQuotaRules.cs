namespace Acme.Workforce.Orgdesign.Position.Quota;

/// <summary>A rule a default position quota must pass. Check returns its problems, or none.</summary>
public interface IDefaultPositionQuotaRule
{
    List<string> Check(DefaultPositionQuota item);
}

public sealed record DefaultPositionQuotaTagsLimitRule(int MaxCount, List<string> Banned) : IDefaultPositionQuotaRule
{
    public List<string> Check(DefaultPositionQuota item)
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

public sealed record DefaultPositionQuotaRequiredLocationRule(string Message) : IDefaultPositionQuotaRule
{
    public List<string> Check(DefaultPositionQuota item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultPositionQuotaRequiredCodeRule(string Message) : IDefaultPositionQuotaRule
{
    public List<string> Check(DefaultPositionQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default position quota.</summary>
public sealed record DefaultPositionQuotaValidatorExtra(List<IDefaultPositionQuotaRule> Rules)
{
    public List<string> Validate(DefaultPositionQuota item)
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

    public void RequireValid(DefaultPositionQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultPositionQuota> items)
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

/// <summary>Functions of default position quota rules.</summary>
public static class DefaultPositionQuotaRules
{
    public static DefaultPositionQuotaValidatorExtra DefaultDefaultPositionQuotaValidator()
    {
        return new DefaultPositionQuotaValidatorExtra(
            new List<IDefaultPositionQuotaRule>
            {
                new DefaultPositionQuotaTagsLimitRule(64, new List<string> { "global", "primary" }),
                new DefaultPositionQuotaRequiredLocationRule("default position quota needs a value"),
                new DefaultPositionQuotaRequiredCodeRule("default position quota needs an entry"),
            });
    }
}
