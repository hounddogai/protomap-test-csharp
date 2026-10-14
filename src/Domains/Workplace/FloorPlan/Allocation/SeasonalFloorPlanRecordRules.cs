namespace Acme.Workforce.Workplace.FloorPlan.Allocation;

/// <summary>A rule a seasonal floor plan record must pass. Check returns its problems, or none.</summary>
public interface ISeasonalFloorPlanRecordRule
{
    List<string> Check(SeasonalFloorPlanRecord item);
}

public sealed record SeasonalFloorPlanRecordRequiredDescriptionRule(string Message) : ISeasonalFloorPlanRecordRule
{
    public List<string> Check(SeasonalFloorPlanRecord item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFloorPlanRecordStartsAtRangeRule(int Min, int Max) : ISeasonalFloorPlanRecordRule
{
    public List<string> Check(SeasonalFloorPlanRecord item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFloorPlanRecordUnitLengthRule(int MaxLength) : ISeasonalFloorPlanRecordRule
{
    public List<string> Check(SeasonalFloorPlanRecord item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFloorPlanRecordCodePrefixRule(string Prefix) : ISeasonalFloorPlanRecordRule
{
    public List<string> Check(SeasonalFloorPlanRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFloorPlanRecordCodeLengthRule(int MaxLength) : ISeasonalFloorPlanRecordRule
{
    public List<string> Check(SeasonalFloorPlanRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal floor plan record.</summary>
public sealed record SeasonalFloorPlanRecordValidator(List<ISeasonalFloorPlanRecordRule> Rules)
{
    public List<string> Validate(SeasonalFloorPlanRecord item)
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

    public void RequireValid(SeasonalFloorPlanRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalFloorPlanRecord> items)
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

/// <summary>Functions of seasonal floor plan record rules.</summary>
public static class SeasonalFloorPlanRecordRules
{
    public static SeasonalFloorPlanRecordValidator DefaultSeasonalFloorPlanRecordValidator()
    {
        return new SeasonalFloorPlanRecordValidator(
            new List<ISeasonalFloorPlanRecordRule>
            {
                new SeasonalFloorPlanRecordRequiredDescriptionRule("seasonal floor plan record needs a value"),
                new SeasonalFloorPlanRecordStartsAtRangeRule(255, 3600),
                new SeasonalFloorPlanRecordUnitLengthRule(1000),
                new SeasonalFloorPlanRecordCodePrefixRule("ACM"),
                new SeasonalFloorPlanRecordCodeLengthRule(86400),
            });
    }
}
