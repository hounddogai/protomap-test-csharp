namespace Acme.Workforce.Workplace.AccessBadge.Policy;

/// <summary>A rule a batched access badge allocation must pass. Check returns its problems, or none.</summary>
public interface IBatchedAccessBadgeAllocationRule
{
    List<string> Check(BatchedAccessBadgeAllocation item);
}

public sealed record BatchedAccessBadgeAllocationChecksumLengthRule(int MaxLength) : IBatchedAccessBadgeAllocationRule
{
    public List<string> Check(BatchedAccessBadgeAllocation item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedAccessBadgeAllocationCodePrefixRule(string Prefix) : IBatchedAccessBadgeAllocationRule
{
    public List<string> Check(BatchedAccessBadgeAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedAccessBadgeAllocationRequiredTitleRule(string Message) : IBatchedAccessBadgeAllocationRule
{
    public List<string> Check(BatchedAccessBadgeAllocation item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedAccessBadgeAllocationRequiredChecksumRule(string Message) : IBatchedAccessBadgeAllocationRule
{
    public List<string> Check(BatchedAccessBadgeAllocation item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedAccessBadgeAllocationRequiredCodeRule(string Message) : IBatchedAccessBadgeAllocationRule
{
    public List<string> Check(BatchedAccessBadgeAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedAccessBadgeAllocationScoreRangeRule(double Min, double Max) : IBatchedAccessBadgeAllocationRule
{
    public List<string> Check(BatchedAccessBadgeAllocation item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a batched access badge allocation.</summary>
public sealed record BatchedAccessBadgeAllocationValidator(List<IBatchedAccessBadgeAllocationRule> Rules)
{
    public List<string> Validate(BatchedAccessBadgeAllocation item)
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

    public void RequireValid(BatchedAccessBadgeAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedAccessBadgeAllocation> items)
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

/// <summary>Functions of batched access badge allocation rules.</summary>
public static class BatchedAccessBadgeAllocationRules
{
    public static BatchedAccessBadgeAllocationValidator DefaultBatchedAccessBadgeAllocationValidator()
    {
        return new BatchedAccessBadgeAllocationValidator(
            new List<IBatchedAccessBadgeAllocationRule>
            {
                new BatchedAccessBadgeAllocationChecksumLengthRule(1000),
                new BatchedAccessBadgeAllocationCodePrefixRule("X"),
                new BatchedAccessBadgeAllocationRequiredTitleRule("batched access badge allocation needs this field"),
                new BatchedAccessBadgeAllocationRequiredChecksumRule(
                    "batched access badge allocation needs an entry"),
                new BatchedAccessBadgeAllocationRequiredCodeRule("batched access badge allocation needs this field"),
                new BatchedAccessBadgeAllocationScoreRangeRule(7.2, 1997.2),
            });
    }
}
