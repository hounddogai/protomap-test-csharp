namespace Acme.Workforce.Compensation.Adjustment.Segment;

/// <summary>A rule a adjustment segment must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentSegmentRule
{
    List<string> Check(AdjustmentSegment item);
}

public sealed record AdjustmentSegmentRequiredCodeRule(string Message) : IAdjustmentSegmentRule
{
    public List<string> Check(AdjustmentSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentSegmentCapacityRangeRule(int Min, int Max) : IAdjustmentSegmentRule
{
    public List<string> Check(AdjustmentSegment item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdjustmentSegmentStartsAtRangeRule(int Min, int Max) : IAdjustmentSegmentRule
{
    public List<string> Check(AdjustmentSegment item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a adjustment segment.</summary>
public sealed record AdjustmentSegmentValidator(List<IAdjustmentSegmentRule> Rules)
{
    public List<string> Validate(AdjustmentSegment item)
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

    public void RequireValid(AdjustmentSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentSegment> items)
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

/// <summary>Functions of adjustment segment rules.</summary>
public static class AdjustmentSegmentRules
{
    public static AdjustmentSegmentValidator DefaultAdjustmentSegmentValidator()
    {
        return new AdjustmentSegmentValidator(
            new List<IAdjustmentSegmentRule>
            {
                new AdjustmentSegmentRequiredCodeRule("adjustment segment needs an entry"),
                new AdjustmentSegmentCapacityRangeRule(255, 3600),
                new AdjustmentSegmentStartsAtRangeRule(80, 255),
            });
    }
}
