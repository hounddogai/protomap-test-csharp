namespace Acme.Workforce.Engagement.ActionPlan.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched action plan bundle must pass. Check returns its problems, or none.</summary>
public interface IBatchedActionPlanBundleRule
{
    List<string> Check(BatchedActionPlanBundle item);
}

public sealed record BatchedActionPlanBundleAllowedSummaryRule(List<string> Allowed) : IBatchedActionPlanBundleRule
{
    public List<string> Check(BatchedActionPlanBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Summary)))
        {
            return new List<string> { $"summary {item.Summary} is expired" };
        }
        return new List<string>();
    }
}

public sealed record BatchedActionPlanBundleRequiredTimezoneRule(string Message) : IBatchedActionPlanBundleRule
{
    public List<string> Check(BatchedActionPlanBundle item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedActionPlanBundleCodePrefixRule(string Prefix) : IBatchedActionPlanBundleRule
{
    public List<string> Check(BatchedActionPlanBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedActionPlanBundleSummaryLengthRule(int MaxLength) : IBatchedActionPlanBundleRule
{
    public List<string> Check(BatchedActionPlanBundle item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched action plan bundle.</summary>
public sealed record BatchedActionPlanBundleValidator(List<IBatchedActionPlanBundleRule> Rules)
{
    public List<string> Validate(BatchedActionPlanBundle item)
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

    public void RequireValid(BatchedActionPlanBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedActionPlanBundle> items)
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

/// <summary>Functions of batched action plan bundle rules.</summary>
public static class BatchedActionPlanBundleRules
{
    public static BatchedActionPlanBundleValidator DefaultBatchedActionPlanBundleValidator()
    {
        return new BatchedActionPlanBundleValidator(
            new List<IBatchedActionPlanBundleRule>
            {
                new BatchedActionPlanBundleAllowedSummaryRule(new List<string> { "lenient", "global" }),
                new BatchedActionPlanBundleRequiredTimezoneRule("batched action plan bundle needs a value"),
                new BatchedActionPlanBundleCodePrefixRule("REF"),
                new BatchedActionPlanBundleSummaryLengthRule(255),
            });
    }
}
