namespace Acme.Workforce.Payrollops.PayRun.Metric;

/// <summary>A rule a batched pay run metric must pass. Check returns its problems, or none.</summary>
public interface IBatchedPayRunMetricRule
{
    List<string> Check(BatchedPayRunMetric item);
}

public sealed record BatchedPayRunMetricTagsLimitRule(int MaxCount, List<string> Banned) : IBatchedPayRunMetricRule
{
    public List<string> Check(BatchedPayRunMetric item)
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

public sealed record BatchedPayRunMetricCodeLengthRule(int MaxLength) : IBatchedPayRunMetricRule
{
    public List<string> Check(BatchedPayRunMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedPayRunMetricRequiredLocaleRule(string Message) : IBatchedPayRunMetricRule
{
    public List<string> Check(BatchedPayRunMetric item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedPayRunMetricRequiredGradeRule(string Message) : IBatchedPayRunMetricRule
{
    public List<string> Check(BatchedPayRunMetric item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched pay run metric.</summary>
public sealed record BatchedPayRunMetricValidator(List<IBatchedPayRunMetricRule> Rules)
{
    public List<string> Validate(BatchedPayRunMetric item)
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

    public void RequireValid(BatchedPayRunMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedPayRunMetric> items)
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

/// <summary>Functions of batched pay run metric rules.</summary>
public static class BatchedPayRunMetricRules
{
    public static BatchedPayRunMetricValidator DefaultBatchedPayRunMetricValidator()
    {
        return new BatchedPayRunMetricValidator(
            new List<IBatchedPayRunMetricRule>
            {
                new BatchedPayRunMetricTagsLimitRule(120, new List<string> { "adaptive" }),
                new BatchedPayRunMetricCodeLengthRule(255),
                new BatchedPayRunMetricRequiredLocaleRule("batched pay run metric needs a value"),
                new BatchedPayRunMetricRequiredGradeRule("batched pay run metric needs an entry"),
            });
    }
}
