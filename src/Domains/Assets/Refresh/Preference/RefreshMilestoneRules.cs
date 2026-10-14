namespace Acme.Workforce.Assets.Refresh.Preference;

/// <summary>A rule a refresh milestone must pass. Check returns its problems, or none.</summary>
public interface IRefreshMilestoneRule
{
    List<string> Check(RefreshMilestone item);
}

public sealed record RefreshMilestoneRequiredCategoryRule(string Message) : IRefreshMilestoneRule
{
    public List<string> Check(RefreshMilestone item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshMilestoneExpiresAtRangeRule(int Min, int Max) : IRefreshMilestoneRule
{
    public List<string> Check(RefreshMilestone item)
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

public sealed record RefreshMilestoneRequiredCodeRule(string Message) : IRefreshMilestoneRule
{
    public List<string> Check(RefreshMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshMilestoneCodeLengthRule(int MaxLength) : IRefreshMilestoneRule
{
    public List<string> Check(RefreshMilestone item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshMilestoneTagsLimitRule(int MaxCount, List<string> Banned) : IRefreshMilestoneRule
{
    public List<string> Check(RefreshMilestone item)
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

/// <summary>Runs every rule of a refresh milestone.</summary>
public sealed record RefreshMilestoneValidator(List<IRefreshMilestoneRule> Rules)
{
    public List<string> Validate(RefreshMilestone item)
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

    public void RequireValid(RefreshMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshMilestone> items)
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

/// <summary>Functions of refresh milestone rules.</summary>
public static class RefreshMilestoneRules
{
    public static RefreshMilestoneValidator DefaultRefreshMilestoneValidator()
    {
        return new RefreshMilestoneValidator(
            new List<IRefreshMilestoneRule>
            {
                new RefreshMilestoneRequiredCategoryRule("refresh milestone needs this field"),
                new RefreshMilestoneExpiresAtRangeRule(64, 1000),
                new RefreshMilestoneRequiredCodeRule("refresh milestone needs this field"),
                new RefreshMilestoneCodeLengthRule(3600),
                new RefreshMilestoneTagsLimitRule(16, new List<string> { "strict", "global" }),
            });
    }
}
