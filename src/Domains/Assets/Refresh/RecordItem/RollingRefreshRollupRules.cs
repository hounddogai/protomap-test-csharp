namespace Acme.Workforce.Assets.Refresh.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling refresh rollup must pass. Check returns its problems, or none.</summary>
public interface IRollingRefreshRollupRule
{
    List<string> Check(RollingRefreshRollup item);
}

public sealed record RollingRefreshRollupTagsLimitRule(int MaxCount, List<string> Banned) : IRollingRefreshRollupRule
{
    public List<string> Check(RollingRefreshRollup item)
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

public sealed record RollingRefreshRollupRequiredUnitRule(string Message) : IRollingRefreshRollupRule
{
    public List<string> Check(RollingRefreshRollup item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingRefreshRollupRequiredCodeRule(string Message) : IRollingRefreshRollupRule
{
    public List<string> Check(RollingRefreshRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingRefreshRollupRatioRangeRule(double Min, double Max) : IRollingRefreshRollupRule
{
    public List<string> Check(RollingRefreshRollup item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRefreshRollupAllowedTitleRule(List<string> Allowed) : IRollingRefreshRollupRule
{
    public List<string> Check(RollingRefreshRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record RollingRefreshRollupUnitLengthRule(int MaxLength) : IRollingRefreshRollupRule
{
    public List<string> Check(RollingRefreshRollup item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling refresh rollup.</summary>
public sealed record RollingRefreshRollupValidator(List<IRollingRefreshRollupRule> Rules)
{
    public List<string> Validate(RollingRefreshRollup item)
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

    public void RequireValid(RollingRefreshRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingRefreshRollup> items)
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

/// <summary>Functions of rolling refresh rollup rules.</summary>
public static class RollingRefreshRollupRules
{
    public static RollingRefreshRollupValidator DefaultRollingRefreshRollupValidator()
    {
        return new RollingRefreshRollupValidator(
            new List<IRollingRefreshRollupRule>
            {
                new RollingRefreshRollupTagsLimitRule(16, new List<string> { "primary", "default" }),
                new RollingRefreshRollupRequiredUnitRule("rolling refresh rollup needs an entry"),
                new RollingRefreshRollupRequiredCodeRule("rolling refresh rollup needs this field"),
                new RollingRefreshRollupRatioRangeRule(0.6, 4011.5),
                new RollingRefreshRollupAllowedTitleRule(new List<string> { "strict", "pending" }),
                new RollingRefreshRollupUnitLengthRule(32),
            });
    }
}
