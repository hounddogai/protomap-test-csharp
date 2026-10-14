namespace Acme.Workforce.Workplace.FloorPlan.Adjustment;

/// <summary>A rule a floor plan checkpoint must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanCheckpointRule
{
    List<string> Check(FloorPlanCheckpoint item);
}

public sealed record FloorPlanCheckpointCodePrefixRule(string Prefix) : IFloorPlanCheckpointRule
{
    public List<string> Check(FloorPlanCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanCheckpointSegmentLengthRule(int MaxLength) : IFloorPlanCheckpointRule
{
    public List<string> Check(FloorPlanCheckpoint item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanCheckpointCodeLengthRule(int MaxLength) : IFloorPlanCheckpointRule
{
    public List<string> Check(FloorPlanCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanCheckpointRequiredSegmentRule(string Message) : IFloorPlanCheckpointRule
{
    public List<string> Check(FloorPlanCheckpoint item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanCheckpointVersionRangeRule(int Min, int Max) : IFloorPlanCheckpointRule
{
    public List<string> Check(FloorPlanCheckpoint item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FloorPlanCheckpointRequiredLabelRule(string Message) : IFloorPlanCheckpointRule
{
    public List<string> Check(FloorPlanCheckpoint item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan checkpoint.</summary>
public sealed record FloorPlanCheckpointValidator(List<IFloorPlanCheckpointRule> Rules)
{
    public List<string> Validate(FloorPlanCheckpoint item)
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

    public void RequireValid(FloorPlanCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanCheckpoint> items)
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

/// <summary>Functions of floor plan checkpoint rules.</summary>
public static class FloorPlanCheckpointRules
{
    public static FloorPlanCheckpointValidator DefaultFloorPlanCheckpointValidator()
    {
        return new FloorPlanCheckpointValidator(
            new List<IFloorPlanCheckpointRule>
            {
                new FloorPlanCheckpointCodePrefixRule("ACM"),
                new FloorPlanCheckpointSegmentLengthRule(1000),
                new FloorPlanCheckpointCodeLengthRule(3600),
                new FloorPlanCheckpointRequiredSegmentRule("floor plan checkpoint needs an entry"),
                new FloorPlanCheckpointVersionRangeRule(80, 255),
                new FloorPlanCheckpointRequiredLabelRule("floor plan checkpoint needs an entry"),
            });
    }
}
