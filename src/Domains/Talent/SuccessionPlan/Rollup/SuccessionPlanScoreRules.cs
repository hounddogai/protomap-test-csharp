namespace Acme.Workforce.Talent.SuccessionPlan.Rollup;

/// <summary>A rule a succession plan score must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanScoreRule
{
    List<string> Check(SuccessionPlanScore item);
}

public sealed record SuccessionPlanScoreRequiredCodeRule(string Message) : ISuccessionPlanScoreRule
{
    public List<string> Check(SuccessionPlanScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanScoreTagsLimitRule(int MaxCount, List<string> Banned) : ISuccessionPlanScoreRule
{
    public List<string> Check(SuccessionPlanScore item)
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

public sealed record SuccessionPlanScoreUpdatedAtRangeRule(int Min, int Max) : ISuccessionPlanScoreRule
{
    public List<string> Check(SuccessionPlanScore item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanScoreTitleLengthRule(int MaxLength) : ISuccessionPlanScoreRule
{
    public List<string> Check(SuccessionPlanScore item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanScoreManagerIdLengthRule(int MaxLength) : ISuccessionPlanScoreRule
{
    public List<string> Check(SuccessionPlanScore item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan score.</summary>
public sealed record SuccessionPlanScoreValidator(List<ISuccessionPlanScoreRule> Rules)
{
    public List<string> Validate(SuccessionPlanScore item)
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

    public void RequireValid(SuccessionPlanScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanScore> items)
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

/// <summary>Functions of succession plan score rules.</summary>
public static class SuccessionPlanScoreRules
{
    public static SuccessionPlanScoreValidator DefaultSuccessionPlanScoreValidator()
    {
        return new SuccessionPlanScoreValidator(
            new List<ISuccessionPlanScoreRule>
            {
                new SuccessionPlanScoreRequiredCodeRule("succession plan score needs an entry"),
                new SuccessionPlanScoreTagsLimitRule(3600, new List<string> { "capped", "strict" }),
                new SuccessionPlanScoreUpdatedAtRangeRule(16, 64),
                new SuccessionPlanScoreTitleLengthRule(86400),
                new SuccessionPlanScoreManagerIdLengthRule(1000),
            });
    }
}
