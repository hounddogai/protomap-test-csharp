namespace Acme.Workforce.Workplace.FloorPlan.Notice;

/// <summary>A rule a rolling floor plan segment must pass. Check returns its problems, or none.</summary>
public interface IRollingFloorPlanSegmentRule
{
    List<string> Check(RollingFloorPlanSegment item);
}

public sealed record RollingFloorPlanSegmentRequiredTitleRule(string Message) : IRollingFloorPlanSegmentRule
{
    public List<string> Check(RollingFloorPlanSegment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingFloorPlanSegmentCurrencyLengthRule(int MaxLength) : IRollingFloorPlanSegmentRule
{
    public List<string> Check(RollingFloorPlanSegment item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingFloorPlanSegmentEndsAtRangeRule(int Min, int Max) : IRollingFloorPlanSegmentRule
{
    public List<string> Check(RollingFloorPlanSegment item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingFloorPlanSegmentRatingRangeRule(double Min, double Max) : IRollingFloorPlanSegmentRule
{
    public List<string> Check(RollingFloorPlanSegment item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling floor plan segment.</summary>
public sealed record RollingFloorPlanSegmentValidator(List<IRollingFloorPlanSegmentRule> Rules)
{
    public List<string> Validate(RollingFloorPlanSegment item)
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

    public void RequireValid(RollingFloorPlanSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingFloorPlanSegment> items)
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

/// <summary>Functions of rolling floor plan segment rules.</summary>
public static class RollingFloorPlanSegmentRules
{
    public static RollingFloorPlanSegmentValidator DefaultRollingFloorPlanSegmentValidator()
    {
        return new RollingFloorPlanSegmentValidator(
            new List<IRollingFloorPlanSegmentRule>
            {
                new RollingFloorPlanSegmentRequiredTitleRule("rolling floor plan segment needs a value"),
                new RollingFloorPlanSegmentCurrencyLengthRule(255),
                new RollingFloorPlanSegmentEndsAtRangeRule(16, 255),
                new RollingFloorPlanSegmentRatingRangeRule(4.7, 892.2),
            });
    }
}
