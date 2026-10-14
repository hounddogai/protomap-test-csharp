namespace Acme.Workforce.Workplace.Occupancy.Score;

/// <summary>A rule a occupancy adjustment must pass. Check returns its problems, or none.</summary>
public interface IOccupancyAdjustmentRule
{
    List<string> Check(OccupancyAdjustment item);
}

public sealed record OccupancyAdjustmentTitleLengthRule(int MaxLength) : IOccupancyAdjustmentRule
{
    public List<string> Check(OccupancyAdjustment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyAdjustmentActiveGuardRule(double Limit) : IOccupancyAdjustmentRule
{
    public List<string> Check(OccupancyAdjustment item)
    {
        if (item.Active && item.Cost > Limit)
        {
            return new List<string> { $"cost {item.Cost} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyAdjustmentRequiredOwnerIdRule(string Message) : IOccupancyAdjustmentRule
{
    public List<string> Check(OccupancyAdjustment item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy adjustment.</summary>
public sealed record OccupancyAdjustmentValidator(List<IOccupancyAdjustmentRule> Rules)
{
    public List<string> Validate(OccupancyAdjustment item)
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

    public void RequireValid(OccupancyAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyAdjustment> items)
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

/// <summary>Functions of occupancy adjustment rules.</summary>
public static class OccupancyAdjustmentRules
{
    public static OccupancyAdjustmentValidator DefaultOccupancyAdjustmentValidator()
    {
        return new OccupancyAdjustmentValidator(
            new List<IOccupancyAdjustmentRule>
            {
                new OccupancyAdjustmentTitleLengthRule(1000),
                new OccupancyAdjustmentActiveGuardRule(4.9),
                new OccupancyAdjustmentRequiredOwnerIdRule("occupancy adjustment needs this field"),
            });
    }
}
