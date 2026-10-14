namespace Acme.Workforce.Workplace.FloorPlan.PlanAudit;

/// <summary>A rule a default floor plan segment must pass. Check returns its problems, or none.</summary>
public interface IDefaultFloorPlanSegmentRule
{
    List<string> Check(DefaultFloorPlanSegment item);
}

public sealed record DefaultFloorPlanSegmentLeaveDaysRangeRule(int Min, int Max) : IDefaultFloorPlanSegmentRule
{
    public List<string> Check(DefaultFloorPlanSegment item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultFloorPlanSegmentCodeLengthRule(int MaxLength) : IDefaultFloorPlanSegmentRule
{
    public List<string> Check(DefaultFloorPlanSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultFloorPlanSegmentNameLengthRule(int MaxLength) : IDefaultFloorPlanSegmentRule
{
    public List<string> Check(DefaultFloorPlanSegment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default floor plan segment.</summary>
public sealed record DefaultFloorPlanSegmentValidator(List<IDefaultFloorPlanSegmentRule> Rules)
{
    public List<string> Validate(DefaultFloorPlanSegment item)
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

    public void RequireValid(DefaultFloorPlanSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultFloorPlanSegment> items)
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

/// <summary>Functions of default floor plan segment rules.</summary>
public static class DefaultFloorPlanSegmentRules
{
    public static DefaultFloorPlanSegmentValidator DefaultDefaultFloorPlanSegmentValidator()
    {
        return new DefaultFloorPlanSegmentValidator(
            new List<IDefaultFloorPlanSegmentRule>
            {
                new DefaultFloorPlanSegmentLeaveDaysRangeRule(32, 120),
                new DefaultFloorPlanSegmentCodeLengthRule(255),
                new DefaultFloorPlanSegmentNameLengthRule(3600),
            });
    }
}
