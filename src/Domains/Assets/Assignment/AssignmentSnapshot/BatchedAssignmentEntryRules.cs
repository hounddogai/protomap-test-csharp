namespace Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

/// <summary>A rule a batched assignment entry must pass. Check returns its problems, or none.</summary>
public interface IBatchedAssignmentEntryRule
{
    List<string> Check(BatchedAssignmentEntry item);
}

public sealed record BatchedAssignmentEntryApprovedGuardRule(double Limit) : IBatchedAssignmentEntryRule
{
    public List<string> Check(BatchedAssignmentEntry item)
    {
        if (item.Approved && item.Minimum > Limit)
        {
            return new List<string> { $"minimum {item.Minimum} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record BatchedAssignmentEntryBatchSizeRangeRule(int Min, int Max) : IBatchedAssignmentEntryRule
{
    public List<string> Check(BatchedAssignmentEntry item)
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

public sealed record BatchedAssignmentEntryCodePrefixRule(string Prefix) : IBatchedAssignmentEntryRule
{
    public List<string> Check(BatchedAssignmentEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedAssignmentEntryRequiredTitleRule(string Message) : IBatchedAssignmentEntryRule
{
    public List<string> Check(BatchedAssignmentEntry item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched assignment entry.</summary>
public sealed record BatchedAssignmentEntryValidator(List<IBatchedAssignmentEntryRule> Rules)
{
    public List<string> Validate(BatchedAssignmentEntry item)
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

    public void RequireValid(BatchedAssignmentEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedAssignmentEntry> items)
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

/// <summary>Functions of batched assignment entry rules.</summary>
public static class BatchedAssignmentEntryRules
{
    public static BatchedAssignmentEntryValidator DefaultBatchedAssignmentEntryValidator()
    {
        return new BatchedAssignmentEntryValidator(
            new List<IBatchedAssignmentEntryRule>
            {
                new BatchedAssignmentEntryApprovedGuardRule(9.4),
                new BatchedAssignmentEntryBatchSizeRangeRule(64, 120),
                new BatchedAssignmentEntryCodePrefixRule("REF"),
                new BatchedAssignmentEntryRequiredTitleRule("batched assignment entry needs an entry"),
            });
    }
}
