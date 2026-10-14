namespace Acme.Workforce.Compensation.Adjustment.Assignment;

/// <summary>A rule a default adjustment estimate must pass. Check returns its problems, or none.</summary>
public interface IDefaultAdjustmentEstimateRule
{
    List<string> Check(DefaultAdjustmentEstimate item);
}

public sealed record DefaultAdjustmentEstimateRegionLengthRule(int MaxLength) : IDefaultAdjustmentEstimateRule
{
    public List<string> Check(DefaultAdjustmentEstimate item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAdjustmentEstimateDurationMinutesRangeRule(int Min, int Max) : IDefaultAdjustmentEstimateRule
{
    public List<string> Check(DefaultAdjustmentEstimate item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAdjustmentEstimateRequiredRegionRule(string Message) : IDefaultAdjustmentEstimateRule
{
    public List<string> Check(DefaultAdjustmentEstimate item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultAdjustmentEstimateNameLengthRule(int MaxLength) : IDefaultAdjustmentEstimateRule
{
    public List<string> Check(DefaultAdjustmentEstimate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default adjustment estimate.</summary>
public sealed record DefaultAdjustmentEstimateValidator(List<IDefaultAdjustmentEstimateRule> Rules)
{
    public List<string> Validate(DefaultAdjustmentEstimate item)
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

    public void RequireValid(DefaultAdjustmentEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultAdjustmentEstimate> items)
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

/// <summary>Functions of default adjustment estimate rules.</summary>
public static class DefaultAdjustmentEstimateRules
{
    public static DefaultAdjustmentEstimateValidator DefaultDefaultAdjustmentEstimateValidator()
    {
        return new DefaultAdjustmentEstimateValidator(
            new List<IDefaultAdjustmentEstimateRule>
            {
                new DefaultAdjustmentEstimateRegionLengthRule(64),
                new DefaultAdjustmentEstimateDurationMinutesRangeRule(120, 1000),
                new DefaultAdjustmentEstimateRequiredRegionRule("default adjustment estimate needs a value"),
                new DefaultAdjustmentEstimateNameLengthRule(16),
            });
    }
}
