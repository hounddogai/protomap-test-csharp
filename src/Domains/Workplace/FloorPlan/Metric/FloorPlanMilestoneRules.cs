namespace Acme.Workforce.Workplace.FloorPlan.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a floor plan milestone must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanMilestoneRule
{
    List<string> Check(FloorPlanMilestone item);
}

public sealed record FloorPlanMilestoneVisibleGuardRule(int Limit) : IFloorPlanMilestoneRule
{
    public List<string> Check(FloorPlanMilestone item)
    {
        if (item.Visible && item.Capacity > Limit)
        {
            return new List<string> { $"capacity {item.Capacity} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanMilestoneAllowedPhoneRule(List<string> Allowed) : IFloorPlanMilestoneRule
{
    public List<string> Check(FloorPlanMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanMilestoneLeaveDaysRangeRule(int Min, int Max) : IFloorPlanMilestoneRule
{
    public List<string> Check(FloorPlanMilestone item)
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

public sealed record FloorPlanMilestoneChannelLengthRule(int MaxLength) : IFloorPlanMilestoneRule
{
    public List<string> Check(FloorPlanMilestone item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanMilestoneRequiredPhoneRule(string Message) : IFloorPlanMilestoneRule
{
    public List<string> Check(FloorPlanMilestone item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan milestone.</summary>
public sealed record FloorPlanMilestoneValidator(List<IFloorPlanMilestoneRule> Rules)
{
    public List<string> Validate(FloorPlanMilestone item)
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

    public void RequireValid(FloorPlanMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanMilestone> items)
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

/// <summary>Functions of floor plan milestone rules.</summary>
public static class FloorPlanMilestoneRules
{
    public static FloorPlanMilestoneValidator DefaultFloorPlanMilestoneValidator()
    {
        return new FloorPlanMilestoneValidator(
            new List<IFloorPlanMilestoneRule>
            {
                new FloorPlanMilestoneVisibleGuardRule(16),
                new FloorPlanMilestoneAllowedPhoneRule(new List<string> { "legacy", "fallback" }),
                new FloorPlanMilestoneLeaveDaysRangeRule(16, 3600),
                new FloorPlanMilestoneChannelLengthRule(3600),
                new FloorPlanMilestoneRequiredPhoneRule("floor plan milestone needs a value"),
            });
    }
}
