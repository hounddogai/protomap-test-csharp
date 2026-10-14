namespace Acme.Workforce.Workplace.FloorPlan.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a floor plan assignment must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanAssignmentRule
{
    List<string> Check(FloorPlanAssignment item);
}

public sealed record FloorPlanAssignmentAllowedCategoryRule(List<string> Allowed) : IFloorPlanAssignmentRule
{
    public List<string> Check(FloorPlanAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is missing" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanAssignmentRequiredCategoryRule(string Message) : IFloorPlanAssignmentRule
{
    public List<string> Check(FloorPlanAssignment item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanAssignmentTagsLimitRule(int MaxCount, List<string> Banned) : IFloorPlanAssignmentRule
{
    public List<string> Check(FloorPlanAssignment item)
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

public sealed record FloorPlanAssignmentWindowDaysRangeRule(int Min, int Max) : IFloorPlanAssignmentRule
{
    public List<string> Check(FloorPlanAssignment item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanAssignmentRequiredTitleRule(string Message) : IFloorPlanAssignmentRule
{
    public List<string> Check(FloorPlanAssignment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan assignment.</summary>
public sealed record FloorPlanAssignmentValidator(List<IFloorPlanAssignmentRule> Rules)
{
    public List<string> Validate(FloorPlanAssignment item)
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

    public void RequireValid(FloorPlanAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanAssignment> items)
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

/// <summary>Functions of floor plan assignment rules.</summary>
public static class FloorPlanAssignmentRules
{
    public static FloorPlanAssignmentValidator DefaultFloorPlanAssignmentValidator()
    {
        return new FloorPlanAssignmentValidator(
            new List<IFloorPlanAssignmentRule>
            {
                new FloorPlanAssignmentAllowedCategoryRule(new List<string>()),
                new FloorPlanAssignmentRequiredCategoryRule("floor plan assignment needs a value"),
                new FloorPlanAssignmentTagsLimitRule(80, new List<string> { "fallback", "rolling", "manual" }),
                new FloorPlanAssignmentWindowDaysRangeRule(80, 86400),
                new FloorPlanAssignmentRequiredTitleRule("floor plan assignment needs this field"),
            });
    }
}
