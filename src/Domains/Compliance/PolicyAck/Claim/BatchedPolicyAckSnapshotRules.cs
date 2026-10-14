namespace Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>A rule a batched policy ack snapshot must pass. Check returns its problems, or none.</summary>
public interface IBatchedPolicyAckSnapshotRule
{
    List<string> Check(BatchedPolicyAckSnapshot item);
}

public sealed record BatchedPolicyAckSnapshotTitleLengthRule(int MaxLength) : IBatchedPolicyAckSnapshotRule
{
    public List<string> Check(BatchedPolicyAckSnapshot item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedPolicyAckSnapshotRequiredCodeRule(string Message) : IBatchedPolicyAckSnapshotRule
{
    public List<string> Check(BatchedPolicyAckSnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedPolicyAckSnapshotMarginRangeRule(double Min, double Max) : IBatchedPolicyAckSnapshotRule
{
    public List<string> Check(BatchedPolicyAckSnapshot item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedPolicyAckSnapshotExternalRefLengthRule(int MaxLength) : IBatchedPolicyAckSnapshotRule
{
    public List<string> Check(BatchedPolicyAckSnapshot item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedPolicyAckSnapshotCeilingRangeRule(double Min, double Max) : IBatchedPolicyAckSnapshotRule
{
    public List<string> Check(BatchedPolicyAckSnapshot item)
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

public sealed record BatchedPolicyAckSnapshotRequiredTitleRule(string Message) : IBatchedPolicyAckSnapshotRule
{
    public List<string> Check(BatchedPolicyAckSnapshot item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched policy ack snapshot.</summary>
public sealed record BatchedPolicyAckSnapshotValidator(List<IBatchedPolicyAckSnapshotRule> Rules)
{
    public List<string> Validate(BatchedPolicyAckSnapshot item)
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

    public void RequireValid(BatchedPolicyAckSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedPolicyAckSnapshot> items)
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

/// <summary>Functions of batched policy ack snapshot rules.</summary>
public static class BatchedPolicyAckSnapshotRules
{
    public static BatchedPolicyAckSnapshotValidator DefaultBatchedPolicyAckSnapshotValidator()
    {
        return new BatchedPolicyAckSnapshotValidator(
            new List<IBatchedPolicyAckSnapshotRule>
            {
                new BatchedPolicyAckSnapshotTitleLengthRule(86400),
                new BatchedPolicyAckSnapshotRequiredCodeRule("batched policy ack snapshot needs a value"),
                new BatchedPolicyAckSnapshotMarginRangeRule(8.5, 3368.2),
                new BatchedPolicyAckSnapshotExternalRefLengthRule(16),
                new BatchedPolicyAckSnapshotCeilingRangeRule(4.8, 2896.5),
                new BatchedPolicyAckSnapshotRequiredTitleRule("batched policy ack snapshot needs this field"),
            });
    }
}
