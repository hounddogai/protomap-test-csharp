namespace Acme.Workforce.Workplace.FloorPlan.Audit;

/// <summary>A rule a floor plan segment must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanSegmentRule
{
    List<string> Check(FloorPlanSegment item);
}

public sealed record FloorPlanSegmentRatioRangeRule(double Min, double Max) : IFloorPlanSegmentRule
{
    public List<string> Check(FloorPlanSegment item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FloorPlanSegmentTagsLimitRule(int MaxCount, List<string> Banned) : IFloorPlanSegmentRule
{
    public List<string> Check(FloorPlanSegment item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record FloorPlanSegmentCodeLengthRule(int MaxLength) : IFloorPlanSegmentRule
{
    public List<string> Check(FloorPlanSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanSegmentRequiredCodeRule(string Message) : IFloorPlanSegmentRule
{
    public List<string> Check(FloorPlanSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanSegmentTitleLengthRule(int MaxLength) : IFloorPlanSegmentRule
{
    public List<string> Check(FloorPlanSegment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan segment.</summary>
public sealed record FloorPlanSegmentValidator(List<IFloorPlanSegmentRule> Rules)
{
    public List<string> Validate(FloorPlanSegment item)
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

    public void RequireValid(FloorPlanSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanSegment> items)
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

/// <summary>Functions of floor plan segment rules.</summary>
public static class FloorPlanSegmentRules
{
    public static FloorPlanSegmentValidator DefaultFloorPlanSegmentValidator()
    {
        return new FloorPlanSegmentValidator(
            new List<IFloorPlanSegmentRule>
            {
                new FloorPlanSegmentRatioRangeRule(6.1, 4852.2),
                new FloorPlanSegmentTagsLimitRule(64, new List<string> { "manual", "strict" }),
                new FloorPlanSegmentCodeLengthRule(3600),
                new FloorPlanSegmentRequiredCodeRule("floor plan segment needs a value"),
                new FloorPlanSegmentTitleLengthRule(1000),
            });
    }
}
