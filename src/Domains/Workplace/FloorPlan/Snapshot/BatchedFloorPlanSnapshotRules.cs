namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched floor plan snapshot must pass. Check returns its problems, or none.</summary>
public interface IBatchedFloorPlanSnapshotRule
{
    List<string> Check(BatchedFloorPlanSnapshot item);
}

public sealed record BatchedFloorPlanSnapshotAllowedNameRule(List<string> Allowed) : IBatchedFloorPlanSnapshotRule
{
    public List<string> Check(BatchedFloorPlanSnapshot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is missing" };
        }
        return new List<string>();
    }
}

public sealed record BatchedFloorPlanSnapshotRequiredContactEmailRule(string Message) : IBatchedFloorPlanSnapshotRule
{
    public List<string> Check(BatchedFloorPlanSnapshot item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedFloorPlanSnapshotEmployeeIdLengthRule(int MaxLength) : IBatchedFloorPlanSnapshotRule
{
    public List<string> Check(BatchedFloorPlanSnapshot item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedFloorPlanSnapshotCodePrefixRule(string Prefix) : IBatchedFloorPlanSnapshotRule
{
    public List<string> Check(BatchedFloorPlanSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedFloorPlanSnapshotBatchSizeRangeRule(int Min, int Max) : IBatchedFloorPlanSnapshotRule
{
    public List<string> Check(BatchedFloorPlanSnapshot item)
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

/// <summary>Runs every rule of a batched floor plan snapshot.</summary>
public sealed record BatchedFloorPlanSnapshotValidator(List<IBatchedFloorPlanSnapshotRule> Rules)
{
    public List<string> Validate(BatchedFloorPlanSnapshot item)
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

    public void RequireValid(BatchedFloorPlanSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedFloorPlanSnapshot> items)
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

/// <summary>Functions of batched floor plan snapshot rules.</summary>
public static class BatchedFloorPlanSnapshotRules
{
    public static BatchedFloorPlanSnapshotValidator DefaultBatchedFloorPlanSnapshotValidator()
    {
        return new BatchedFloorPlanSnapshotValidator(
            new List<IBatchedFloorPlanSnapshotRule>
            {
                new BatchedFloorPlanSnapshotAllowedNameRule(new List<string>()),
                new BatchedFloorPlanSnapshotRequiredContactEmailRule("batched floor plan snapshot needs an entry"),
                new BatchedFloorPlanSnapshotEmployeeIdLengthRule(16),
                new BatchedFloorPlanSnapshotCodePrefixRule("X"),
                new BatchedFloorPlanSnapshotBatchSizeRangeRule(16, 3600),
            });
    }
}
