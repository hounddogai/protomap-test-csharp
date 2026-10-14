namespace Acme.Workforce.Absence.Policy.Baseline;

/// <summary>A rule a batched policy threshold must pass. Check returns its problems, or none.</summary>
public interface IBatchedPolicyThresholdRule
{
    List<string> Check(BatchedPolicyThreshold item);
}

public sealed record BatchedPolicyThresholdFloorValueRangeRule(double Min, double Max) : IBatchedPolicyThresholdRule
{
    public List<string> Check(BatchedPolicyThreshold item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedPolicyThresholdRequiredNameRule(string Message) : IBatchedPolicyThresholdRule
{
    public List<string> Check(BatchedPolicyThreshold item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedPolicyThresholdCapacityRangeRule(int Min, int Max) : IBatchedPolicyThresholdRule
{
    public List<string> Check(BatchedPolicyThreshold item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedPolicyThresholdPriorityRangeRule(int Min, int Max) : IBatchedPolicyThresholdRule
{
    public List<string> Check(BatchedPolicyThreshold item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedPolicyThresholdCodePrefixRule(string Prefix) : IBatchedPolicyThresholdRule
{
    public List<string> Check(BatchedPolicyThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedPolicyThresholdThresholdRangeRule(double Min, double Max) : IBatchedPolicyThresholdRule
{
    public List<string> Check(BatchedPolicyThreshold item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched policy threshold.</summary>
public sealed record BatchedPolicyThresholdValidator(List<IBatchedPolicyThresholdRule> Rules)
{
    public List<string> Validate(BatchedPolicyThreshold item)
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

    public void RequireValid(BatchedPolicyThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedPolicyThreshold> items)
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

/// <summary>Functions of batched policy threshold rules.</summary>
public static class BatchedPolicyThresholdRules
{
    public static BatchedPolicyThresholdValidator DefaultBatchedPolicyThresholdValidator()
    {
        return new BatchedPolicyThresholdValidator(
            new List<IBatchedPolicyThresholdRule>
            {
                new BatchedPolicyThresholdFloorValueRangeRule(4.7, 1717.3),
                new BatchedPolicyThresholdRequiredNameRule("batched policy threshold needs this field"),
                new BatchedPolicyThresholdCapacityRangeRule(64, 64),
                new BatchedPolicyThresholdPriorityRangeRule(64, 3600),
                new BatchedPolicyThresholdCodePrefixRule("REF"),
                new BatchedPolicyThresholdThresholdRangeRule(2.4, 1056.1),
            });
    }
}
