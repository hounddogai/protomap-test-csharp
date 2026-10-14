namespace Acme.Workforce.Workplace.FloorPlan.Score;

/// <summary>A rule a seasonal floor plan checkpoint must pass. Check returns its problems, or none.</summary>
public interface ISeasonalFloorPlanCheckpointRule
{
    List<string> Check(SeasonalFloorPlanCheckpoint item);
}

public sealed record SeasonalFloorPlanCheckpointRequiredCodeRule(string Message) : ISeasonalFloorPlanCheckpointRule
{
    public List<string> Check(SeasonalFloorPlanCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFloorPlanCheckpointQuantityRangeRule(int Min, int Max) : ISeasonalFloorPlanCheckpointRule
{
    public List<string> Check(SeasonalFloorPlanCheckpoint item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFloorPlanCheckpointCeilingRangeRule(double Min, double Max) : ISeasonalFloorPlanCheckpointRule
{
    public List<string> Check(SeasonalFloorPlanCheckpoint item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalFloorPlanCheckpointRequiredNotesRule(string Message) : ISeasonalFloorPlanCheckpointRule
{
    public List<string> Check(SeasonalFloorPlanCheckpoint item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFloorPlanCheckpointCodeLengthRule(int MaxLength) : ISeasonalFloorPlanCheckpointRule
{
    public List<string> Check(SeasonalFloorPlanCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal floor plan checkpoint.</summary>
public sealed record SeasonalFloorPlanCheckpointValidator(List<ISeasonalFloorPlanCheckpointRule> Rules)
{
    public List<string> Validate(SeasonalFloorPlanCheckpoint item)
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

    public void RequireValid(SeasonalFloorPlanCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalFloorPlanCheckpoint> items)
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

/// <summary>Functions of seasonal floor plan checkpoint rules.</summary>
public static class SeasonalFloorPlanCheckpointRules
{
    public static SeasonalFloorPlanCheckpointValidator DefaultSeasonalFloorPlanCheckpointValidator()
    {
        return new SeasonalFloorPlanCheckpointValidator(
            new List<ISeasonalFloorPlanCheckpointRule>
            {
                new SeasonalFloorPlanCheckpointRequiredCodeRule("seasonal floor plan checkpoint needs an entry"),
                new SeasonalFloorPlanCheckpointQuantityRangeRule(120, 255),
                new SeasonalFloorPlanCheckpointCeilingRangeRule(6.5, 1106.7),
                new SeasonalFloorPlanCheckpointRequiredNotesRule("seasonal floor plan checkpoint needs a value"),
                new SeasonalFloorPlanCheckpointCodeLengthRule(255),
            });
    }
}
