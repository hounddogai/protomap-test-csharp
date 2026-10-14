namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

/// <summary>A rule a floor plan schedule must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanScheduleRule
{
    List<string> Check(FloorPlanSchedule item);
}

public sealed record FloorPlanScheduleRequiredNameRule(string Message) : IFloorPlanScheduleRule
{
    public List<string> Check(FloorPlanSchedule item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanScheduleNameLengthRule(int MaxLength) : IFloorPlanScheduleRule
{
    public List<string> Check(FloorPlanSchedule item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanScheduleExpiresAtRangeRule(int Min, int Max) : IFloorPlanScheduleRule
{
    public List<string> Check(FloorPlanSchedule item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanScheduleRetriesRangeRule(int Min, int Max) : IFloorPlanScheduleRule
{
    public List<string> Check(FloorPlanSchedule item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanScheduleVisibleGuardRule(int Limit) : IFloorPlanScheduleRule
{
    public List<string> Check(FloorPlanSchedule item)
    {
        if (item.Visible && item.WindowDays > Limit)
        {
            return new List<string> { $"window days {item.WindowDays} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan schedule.</summary>
public sealed record FloorPlanScheduleValidator(List<IFloorPlanScheduleRule> Rules)
{
    public List<string> Validate(FloorPlanSchedule item)
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

    public void RequireValid(FloorPlanSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanSchedule> items)
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

/// <summary>Functions of floor plan schedule rules.</summary>
public static class FloorPlanScheduleRules
{
    public static FloorPlanScheduleValidator DefaultFloorPlanScheduleValidator()
    {
        return new FloorPlanScheduleValidator(
            new List<IFloorPlanScheduleRule>
            {
                new FloorPlanScheduleRequiredNameRule("floor plan schedule needs this field"),
                new FloorPlanScheduleNameLengthRule(3600),
                new FloorPlanScheduleExpiresAtRangeRule(16, 120),
                new FloorPlanScheduleRetriesRangeRule(32, 255),
                new FloorPlanScheduleVisibleGuardRule(64),
            });
    }
}
