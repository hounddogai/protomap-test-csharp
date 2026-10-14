namespace Acme.Workforce.Workplace.Occupancy.Ledger;

/// <summary>A rule a default occupancy snapshot must pass. Check returns its problems, or none.</summary>
public interface IDefaultOccupancySnapshotRule
{
    List<string> Check(DefaultOccupancySnapshot item);
}

public sealed record DefaultOccupancySnapshotLocationLengthRule(int MaxLength) : IDefaultOccupancySnapshotRule
{
    public List<string> Check(DefaultOccupancySnapshot item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultOccupancySnapshotVisibleGuardRule(double Limit) : IDefaultOccupancySnapshotRule
{
    public List<string> Check(DefaultOccupancySnapshot item)
    {
        if (item.Visible && item.Fte > Limit)
        {
            return new List<string> { $"fte {item.Fte} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record DefaultOccupancySnapshotRequiredSummaryRule(string Message) : IDefaultOccupancySnapshotRule
{
    public List<string> Check(DefaultOccupancySnapshot item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultOccupancySnapshotRequiredLabelRule(string Message) : IDefaultOccupancySnapshotRule
{
    public List<string> Check(DefaultOccupancySnapshot item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default occupancy snapshot.</summary>
public sealed record DefaultOccupancySnapshotValidator(List<IDefaultOccupancySnapshotRule> Rules)
{
    public List<string> Validate(DefaultOccupancySnapshot item)
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

    public void RequireValid(DefaultOccupancySnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultOccupancySnapshot> items)
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

/// <summary>Functions of default occupancy snapshot rules.</summary>
public static class DefaultOccupancySnapshotRules
{
    public static DefaultOccupancySnapshotValidator DefaultDefaultOccupancySnapshotValidator()
    {
        return new DefaultOccupancySnapshotValidator(
            new List<IDefaultOccupancySnapshotRule>
            {
                new DefaultOccupancySnapshotLocationLengthRule(1000),
                new DefaultOccupancySnapshotVisibleGuardRule(5.3),
                new DefaultOccupancySnapshotRequiredSummaryRule("default occupancy snapshot needs this field"),
                new DefaultOccupancySnapshotRequiredLabelRule("default occupancy snapshot needs this field"),
            });
    }
}
