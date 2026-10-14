namespace Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

/// <summary>A rule a adaptive occupancy template must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveOccupancyTemplateRule
{
    List<string> Check(AdaptiveOccupancyTemplate item);
}

public sealed record AdaptiveOccupancyTemplateThresholdRangeRule(double Min, double Max) : IAdaptiveOccupancyTemplateRule
{
    public List<string> Check(AdaptiveOccupancyTemplate item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveOccupancyTemplateSourceLengthRule(int MaxLength) : IAdaptiveOccupancyTemplateRule
{
    public List<string> Check(AdaptiveOccupancyTemplate item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveOccupancyTemplateLabelLengthRule(int MaxLength) : IAdaptiveOccupancyTemplateRule
{
    public List<string> Check(AdaptiveOccupancyTemplate item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive occupancy template.</summary>
public sealed record AdaptiveOccupancyTemplateValidator(List<IAdaptiveOccupancyTemplateRule> Rules)
{
    public List<string> Validate(AdaptiveOccupancyTemplate item)
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

    public void RequireValid(AdaptiveOccupancyTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveOccupancyTemplate> items)
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

/// <summary>Functions of adaptive occupancy template rules.</summary>
public static class AdaptiveOccupancyTemplateRules
{
    public static AdaptiveOccupancyTemplateValidator DefaultAdaptiveOccupancyTemplateValidator()
    {
        return new AdaptiveOccupancyTemplateValidator(
            new List<IAdaptiveOccupancyTemplateRule>
            {
                new AdaptiveOccupancyTemplateThresholdRangeRule(2.7, 485.7),
                new AdaptiveOccupancyTemplateSourceLengthRule(16),
                new AdaptiveOccupancyTemplateLabelLengthRule(80),
            });
    }
}
