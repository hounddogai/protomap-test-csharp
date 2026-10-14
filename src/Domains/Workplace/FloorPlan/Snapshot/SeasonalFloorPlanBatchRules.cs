namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>A rule a seasonal floor plan batch must pass. Check returns its problems, or none.</summary>
public interface ISeasonalFloorPlanBatchRule
{
    List<string> Check(SeasonalFloorPlanBatch item);
}

public sealed record SeasonalFloorPlanBatchRequiredNameRule(string Message) : ISeasonalFloorPlanBatchRule
{
    public List<string> Check(SeasonalFloorPlanBatch item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFloorPlanBatchCodeLengthRule(int MaxLength) : ISeasonalFloorPlanBatchRule
{
    public List<string> Check(SeasonalFloorPlanBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFloorPlanBatchAmountRangeRule(double Min, double Max) : ISeasonalFloorPlanBatchRule
{
    public List<string> Check(SeasonalFloorPlanBatch item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal floor plan batch.</summary>
public sealed record SeasonalFloorPlanBatchValidator(List<ISeasonalFloorPlanBatchRule> Rules)
{
    public List<string> Validate(SeasonalFloorPlanBatch item)
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

    public void RequireValid(SeasonalFloorPlanBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalFloorPlanBatch> items)
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

/// <summary>Functions of seasonal floor plan batch rules.</summary>
public static class SeasonalFloorPlanBatchRules
{
    public static SeasonalFloorPlanBatchValidator DefaultSeasonalFloorPlanBatchValidator()
    {
        return new SeasonalFloorPlanBatchValidator(
            new List<ISeasonalFloorPlanBatchRule>
            {
                new SeasonalFloorPlanBatchRequiredNameRule("seasonal floor plan batch needs an entry"),
                new SeasonalFloorPlanBatchCodeLengthRule(32),
                new SeasonalFloorPlanBatchAmountRangeRule(2.0, 603.5),
            });
    }
}
