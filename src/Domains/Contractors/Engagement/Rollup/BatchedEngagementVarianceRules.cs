namespace Acme.Workforce.Contractors.Engagement.Rollup;

/// <summary>A rule a batched engagement variance must pass. Check returns its problems, or none.</summary>
public interface IBatchedEngagementVarianceRule
{
    List<string> Check(BatchedEngagementVariance item);
}

public sealed record BatchedEngagementVarianceCodeLengthRule(int MaxLength) : IBatchedEngagementVarianceRule
{
    public List<string> Check(BatchedEngagementVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedEngagementVarianceRequiredSkillRule(string Message) : IBatchedEngagementVarianceRule
{
    public List<string> Check(BatchedEngagementVariance item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedEngagementVarianceRequiredCodeRule(string Message) : IBatchedEngagementVarianceRule
{
    public List<string> Check(BatchedEngagementVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched engagement variance.</summary>
public sealed record BatchedEngagementVarianceValidator(List<IBatchedEngagementVarianceRule> Rules)
{
    public List<string> Validate(BatchedEngagementVariance item)
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

    public void RequireValid(BatchedEngagementVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedEngagementVariance> items)
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

/// <summary>Functions of batched engagement variance rules.</summary>
public static class BatchedEngagementVarianceRules
{
    public static BatchedEngagementVarianceValidator DefaultBatchedEngagementVarianceValidator()
    {
        return new BatchedEngagementVarianceValidator(
            new List<IBatchedEngagementVarianceRule>
            {
                new BatchedEngagementVarianceCodeLengthRule(255),
                new BatchedEngagementVarianceRequiredSkillRule("batched engagement variance needs this field"),
                new BatchedEngagementVarianceRequiredCodeRule("batched engagement variance needs this field"),
            });
    }
}
