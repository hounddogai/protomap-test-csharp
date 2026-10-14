namespace Acme.Workforce.Assets.Refresh.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched refresh assignment must pass. Check returns its problems, or none.</summary>
public interface IBatchedRefreshAssignmentRule
{
    List<string> Check(BatchedRefreshAssignment item);
}

public sealed record BatchedRefreshAssignmentAllowedCodeRule(List<string> Allowed) : IBatchedRefreshAssignmentRule
{
    public List<string> Check(BatchedRefreshAssignment item)
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

public sealed record BatchedRefreshAssignmentRequiredNameRule(string Message) : IBatchedRefreshAssignmentRule
{
    public List<string> Check(BatchedRefreshAssignment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedRefreshAssignmentBatchSizeRangeRule(int Min, int Max) : IBatchedRefreshAssignmentRule
{
    public List<string> Check(BatchedRefreshAssignment item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedRefreshAssignmentRetriesRangeRule(int Min, int Max) : IBatchedRefreshAssignmentRule
{
    public List<string> Check(BatchedRefreshAssignment item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedRefreshAssignmentCodePrefixRule(string Prefix) : IBatchedRefreshAssignmentRule
{
    public List<string> Check(BatchedRefreshAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched refresh assignment.</summary>
public sealed record BatchedRefreshAssignmentValidator(List<IBatchedRefreshAssignmentRule> Rules)
{
    public List<string> Validate(BatchedRefreshAssignment item)
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

    public void RequireValid(BatchedRefreshAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedRefreshAssignment> items)
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

/// <summary>Functions of batched refresh assignment rules.</summary>
public static class BatchedRefreshAssignmentRules
{
    public static BatchedRefreshAssignmentValidator DefaultBatchedRefreshAssignmentValidator()
    {
        return new BatchedRefreshAssignmentValidator(
            new List<IBatchedRefreshAssignmentRule>
            {
                new BatchedRefreshAssignmentAllowedCodeRule(new List<string> { "fallback", "scheduled" }),
                new BatchedRefreshAssignmentRequiredNameRule("batched refresh assignment needs this field"),
                new BatchedRefreshAssignmentBatchSizeRangeRule(80, 120),
                new BatchedRefreshAssignmentRetriesRangeRule(64, 120),
                new BatchedRefreshAssignmentCodePrefixRule("X"),
            });
    }
}
