namespace Acme.Workforce.Workplace.Occupancy.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched occupancy snapshot must pass. Check returns its problems, or none.</summary>
public interface IBatchedOccupancySnapshotRule
{
    List<string> Check(BatchedOccupancySnapshot item);
}

public sealed record BatchedOccupancySnapshotTitleLengthRule(int MaxLength) : IBatchedOccupancySnapshotRule
{
    public List<string> Check(BatchedOccupancySnapshot item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedOccupancySnapshotRequiredGradeRule(string Message) : IBatchedOccupancySnapshotRule
{
    public List<string> Check(BatchedOccupancySnapshot item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedOccupancySnapshotRatingRangeRule(double Min, double Max) : IBatchedOccupancySnapshotRule
{
    public List<string> Check(BatchedOccupancySnapshot item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedOccupancySnapshotAllowedGradeRule(List<string> Allowed) : IBatchedOccupancySnapshotRule
{
    public List<string> Check(BatchedOccupancySnapshot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched occupancy snapshot.</summary>
public sealed record BatchedOccupancySnapshotValidator(List<IBatchedOccupancySnapshotRule> Rules)
{
    public List<string> Validate(BatchedOccupancySnapshot item)
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

    public void RequireValid(BatchedOccupancySnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedOccupancySnapshot> items)
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

/// <summary>Functions of batched occupancy snapshot rules.</summary>
public static class BatchedOccupancySnapshotRules
{
    public static BatchedOccupancySnapshotValidator DefaultBatchedOccupancySnapshotValidator()
    {
        return new BatchedOccupancySnapshotValidator(
            new List<IBatchedOccupancySnapshotRule>
            {
                new BatchedOccupancySnapshotTitleLengthRule(3600),
                new BatchedOccupancySnapshotRequiredGradeRule("batched occupancy snapshot needs this field"),
                new BatchedOccupancySnapshotRatingRangeRule(6.0, 1133.0),
                new BatchedOccupancySnapshotAllowedGradeRule(new List<string> { "manual", "batched" }),
            });
    }
}
