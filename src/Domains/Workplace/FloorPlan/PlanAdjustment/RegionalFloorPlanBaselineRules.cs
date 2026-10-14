namespace Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

/// <summary>A rule a regional floor plan baseline must pass. Check returns its problems, or none.</summary>
public interface IRegionalFloorPlanBaselineRule
{
    List<string> Check(RegionalFloorPlanBaseline item);
}

public sealed record RegionalFloorPlanBaselineHoursRangeRule(double Min, double Max) : IRegionalFloorPlanBaselineRule
{
    public List<string> Check(RegionalFloorPlanBaseline item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalFloorPlanBaselineRequiredLabelRule(string Message) : IRegionalFloorPlanBaselineRule
{
    public List<string> Check(RegionalFloorPlanBaseline item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalFloorPlanBaselineCodeLengthRule(int MaxLength) : IRegionalFloorPlanBaselineRule
{
    public List<string> Check(RegionalFloorPlanBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional floor plan baseline.</summary>
public sealed record RegionalFloorPlanBaselineValidator(List<IRegionalFloorPlanBaselineRule> Rules)
{
    public List<string> Validate(RegionalFloorPlanBaseline item)
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

    public void RequireValid(RegionalFloorPlanBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalFloorPlanBaseline> items)
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

/// <summary>Functions of regional floor plan baseline rules.</summary>
public static class RegionalFloorPlanBaselineRules
{
    public static RegionalFloorPlanBaselineValidator DefaultRegionalFloorPlanBaselineValidator()
    {
        return new RegionalFloorPlanBaselineValidator(
            new List<IRegionalFloorPlanBaselineRule>
            {
                new RegionalFloorPlanBaselineHoursRangeRule(8.6, 1503.5),
                new RegionalFloorPlanBaselineRequiredLabelRule("regional floor plan baseline needs an entry"),
                new RegionalFloorPlanBaselineCodeLengthRule(16),
            });
    }
}
