namespace Acme.Workforce.Absence.Balance.RecordItem;

/// <summary>A rule a batched balance mapping must pass. Check returns its problems, or none.</summary>
public interface IBatchedBalanceMappingRule
{
    List<string> Check(BatchedBalanceMapping item);
}

public sealed record BatchedBalanceMappingVersionRangeRule(int Min, int Max) : IBatchedBalanceMappingRule
{
    public List<string> Check(BatchedBalanceMapping item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedBalanceMappingRequiredNameRule(string Message) : IBatchedBalanceMappingRule
{
    public List<string> Check(BatchedBalanceMapping item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedBalanceMappingRequiredCodeRule(string Message) : IBatchedBalanceMappingRule
{
    public List<string> Check(BatchedBalanceMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedBalanceMappingCodePrefixRule(string Prefix) : IBatchedBalanceMappingRule
{
    public List<string> Check(BatchedBalanceMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedBalanceMappingJobCodeLengthRule(int MaxLength) : IBatchedBalanceMappingRule
{
    public List<string> Check(BatchedBalanceMapping item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched balance mapping.</summary>
public sealed record BatchedBalanceMappingValidator(List<IBatchedBalanceMappingRule> Rules)
{
    public List<string> Validate(BatchedBalanceMapping item)
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

    public void RequireValid(BatchedBalanceMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedBalanceMapping> items)
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

/// <summary>Functions of batched balance mapping rules.</summary>
public static class BatchedBalanceMappingRules
{
    public static BatchedBalanceMappingValidator DefaultBatchedBalanceMappingValidator()
    {
        return new BatchedBalanceMappingValidator(
            new List<IBatchedBalanceMappingRule>
            {
                new BatchedBalanceMappingVersionRangeRule(120, 1000),
                new BatchedBalanceMappingRequiredNameRule("batched balance mapping needs an entry"),
                new BatchedBalanceMappingRequiredCodeRule("batched balance mapping needs an entry"),
                new BatchedBalanceMappingCodePrefixRule("ACM"),
                new BatchedBalanceMappingJobCodeLengthRule(86400),
            });
    }
}
