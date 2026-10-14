namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched leave request snapshot must pass. Check returns its problems, or none.</summary>
public interface IBatchedLeaveRequestSnapshotRule
{
    List<string> Check(BatchedLeaveRequestSnapshot item);
}

public sealed record BatchedLeaveRequestSnapshotQuantityRangeRule(int Min, int Max) : IBatchedLeaveRequestSnapshotRule
{
    public List<string> Check(BatchedLeaveRequestSnapshot item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedLeaveRequestSnapshotAllowedCodeRule(List<string> Allowed) : IBatchedLeaveRequestSnapshotRule
{
    public List<string> Check(BatchedLeaveRequestSnapshot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record BatchedLeaveRequestSnapshotSalaryBandLengthRule(int MaxLength) : IBatchedLeaveRequestSnapshotRule
{
    public List<string> Check(BatchedLeaveRequestSnapshot item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedLeaveRequestSnapshotRequiredCodeRule(string Message) : IBatchedLeaveRequestSnapshotRule
{
    public List<string> Check(BatchedLeaveRequestSnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedLeaveRequestSnapshotCodePrefixRule(string Prefix) : IBatchedLeaveRequestSnapshotRule
{
    public List<string> Check(BatchedLeaveRequestSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched leave request snapshot.</summary>
public sealed record BatchedLeaveRequestSnapshotValidator(List<IBatchedLeaveRequestSnapshotRule> Rules)
{
    public List<string> Validate(BatchedLeaveRequestSnapshot item)
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

    public void RequireValid(BatchedLeaveRequestSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedLeaveRequestSnapshot> items)
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

/// <summary>Functions of batched leave request snapshot rules.</summary>
public static class BatchedLeaveRequestSnapshotRules
{
    public static BatchedLeaveRequestSnapshotValidator DefaultBatchedLeaveRequestSnapshotValidator()
    {
        return new BatchedLeaveRequestSnapshotValidator(
            new List<IBatchedLeaveRequestSnapshotRule>
            {
                new BatchedLeaveRequestSnapshotQuantityRangeRule(255, 3600),
                new BatchedLeaveRequestSnapshotAllowedCodeRule(new List<string>()),
                new BatchedLeaveRequestSnapshotSalaryBandLengthRule(64),
                new BatchedLeaveRequestSnapshotRequiredCodeRule("batched leave request snapshot needs a value"),
                new BatchedLeaveRequestSnapshotCodePrefixRule("OPS"),
            });
    }
}
