namespace Acme.Workforce.Compensation.BonusPool.Entry;

/// <summary>A rule a batched bonus pool threshold must pass. Check returns its problems, or none.</summary>
public interface IBatchedBonusPoolThresholdRule
{
    List<string> Check(BatchedBonusPoolThreshold item);
}

public sealed record BatchedBonusPoolThresholdRequiredCodeRule(string Message) : IBatchedBonusPoolThresholdRule
{
    public List<string> Check(BatchedBonusPoolThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedBonusPoolThresholdRequiredExternalRefRule(string Message) : IBatchedBonusPoolThresholdRule
{
    public List<string> Check(BatchedBonusPoolThreshold item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedBonusPoolThresholdCeilingRangeRule(double Min, double Max) : IBatchedBonusPoolThresholdRule
{
    public List<string> Check(BatchedBonusPoolThreshold item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedBonusPoolThresholdRequiredUnitRule(string Message) : IBatchedBonusPoolThresholdRule
{
    public List<string> Check(BatchedBonusPoolThreshold item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedBonusPoolThresholdCodePrefixRule(string Prefix) : IBatchedBonusPoolThresholdRule
{
    public List<string> Check(BatchedBonusPoolThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedBonusPoolThresholdExternalRefLengthRule(int MaxLength) : IBatchedBonusPoolThresholdRule
{
    public List<string> Check(BatchedBonusPoolThreshold item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched bonus pool threshold.</summary>
public sealed record BatchedBonusPoolThresholdValidator(List<IBatchedBonusPoolThresholdRule> Rules)
{
    public List<string> Validate(BatchedBonusPoolThreshold item)
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

    public void RequireValid(BatchedBonusPoolThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedBonusPoolThreshold> items)
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

/// <summary>Functions of batched bonus pool threshold rules.</summary>
public static class BatchedBonusPoolThresholdRules
{
    public static BatchedBonusPoolThresholdValidator DefaultBatchedBonusPoolThresholdValidator()
    {
        return new BatchedBonusPoolThresholdValidator(
            new List<IBatchedBonusPoolThresholdRule>
            {
                new BatchedBonusPoolThresholdRequiredCodeRule("batched bonus pool threshold needs an entry"),
                new BatchedBonusPoolThresholdRequiredExternalRefRule("batched bonus pool threshold needs a value"),
                new BatchedBonusPoolThresholdCeilingRangeRule(9.7, 2516.8),
                new BatchedBonusPoolThresholdRequiredUnitRule("batched bonus pool threshold needs a value"),
                new BatchedBonusPoolThresholdCodePrefixRule("ACM"),
                new BatchedBonusPoolThresholdExternalRefLengthRule(32),
            });
    }
}
