namespace Acme.Workforce.Workplace.Occupancy.Snapshot;

/// <summary>A rule a scheduled occupancy snapshot must pass. Check returns its problems, or none.</summary>
public interface IScheduledOccupancySnapshotRule
{
    List<string> Check(ScheduledOccupancySnapshot item);
}

public sealed record ScheduledOccupancySnapshotRevisionRangeRule(int Min, int Max) : IScheduledOccupancySnapshotRule
{
    public List<string> Check(ScheduledOccupancySnapshot item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledOccupancySnapshotRequiredNameRule(string Message) : IScheduledOccupancySnapshotRule
{
    public List<string> Check(ScheduledOccupancySnapshot item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledOccupancySnapshotNameLengthRule(int MaxLength) : IScheduledOccupancySnapshotRule
{
    public List<string> Check(ScheduledOccupancySnapshot item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled occupancy snapshot.</summary>
public sealed record ScheduledOccupancySnapshotValidator(List<IScheduledOccupancySnapshotRule> Rules)
{
    public List<string> Validate(ScheduledOccupancySnapshot item)
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

    public void RequireValid(ScheduledOccupancySnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledOccupancySnapshot> items)
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

/// <summary>Functions of scheduled occupancy snapshot rules.</summary>
public static class ScheduledOccupancySnapshotRules
{
    public static ScheduledOccupancySnapshotValidator DefaultScheduledOccupancySnapshotValidator()
    {
        return new ScheduledOccupancySnapshotValidator(
            new List<IScheduledOccupancySnapshotRule>
            {
                new ScheduledOccupancySnapshotRevisionRangeRule(255, 3600),
                new ScheduledOccupancySnapshotRequiredNameRule("scheduled occupancy snapshot needs a value"),
                new ScheduledOccupancySnapshotNameLengthRule(64),
            });
    }
}
