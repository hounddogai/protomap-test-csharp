namespace Acme.Workforce.Compensation.Adjustment.Tier;

/// <summary>A rule a adaptive adjustment target must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveAdjustmentTargetRule
{
    List<string> Check(AdaptiveAdjustmentTarget item);
}

public sealed record AdaptiveAdjustmentTargetRevisionRangeRule(int Min, int Max) : IAdaptiveAdjustmentTargetRule
{
    public List<string> Check(AdaptiveAdjustmentTarget item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveAdjustmentTargetLimitRangeRule(int Min, int Max) : IAdaptiveAdjustmentTargetRule
{
    public List<string> Check(AdaptiveAdjustmentTarget item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveAdjustmentTargetRegionLengthRule(int MaxLength) : IAdaptiveAdjustmentTargetRule
{
    public List<string> Check(AdaptiveAdjustmentTarget item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive adjustment target.</summary>
public sealed record AdaptiveAdjustmentTargetValidator(List<IAdaptiveAdjustmentTargetRule> Rules)
{
    public List<string> Validate(AdaptiveAdjustmentTarget item)
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

    public void RequireValid(AdaptiveAdjustmentTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveAdjustmentTarget> items)
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

/// <summary>Functions of adaptive adjustment target rules.</summary>
public static class AdaptiveAdjustmentTargetRules
{
    public static AdaptiveAdjustmentTargetValidator DefaultAdaptiveAdjustmentTargetValidator()
    {
        return new AdaptiveAdjustmentTargetValidator(
            new List<IAdaptiveAdjustmentTargetRule>
            {
                new AdaptiveAdjustmentTargetRevisionRangeRule(1000, 1000),
                new AdaptiveAdjustmentTargetLimitRangeRule(64, 64),
                new AdaptiveAdjustmentTargetRegionLengthRule(3600),
            });
    }
}
