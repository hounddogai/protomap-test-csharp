namespace Acme.Workforce.Workplace.FloorPlan.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a floor plan baseline must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanBaselineRule
{
    List<string> Check(FloorPlanBaseline item);
}

public sealed record FloorPlanBaselineRequiredCodeRule(string Message) : IFloorPlanBaselineRule
{
    public List<string> Check(FloorPlanBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanBaselineAllowedCodeRule(List<string> Allowed) : IFloorPlanBaselineRule
{
    public List<string> Check(FloorPlanBaseline item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanBaselineRequiredNameRule(string Message) : IFloorPlanBaselineRule
{
    public List<string> Check(FloorPlanBaseline item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanBaselineScoreRangeRule(double Min, double Max) : IFloorPlanBaselineRule
{
    public List<string> Check(FloorPlanBaseline item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan baseline.</summary>
public sealed record FloorPlanBaselineValidator(List<IFloorPlanBaselineRule> Rules)
{
    public List<string> Validate(FloorPlanBaseline item)
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

    public void RequireValid(FloorPlanBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanBaseline> items)
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

/// <summary>Functions of floor plan baseline rules.</summary>
public static class FloorPlanBaselineRules
{
    public static FloorPlanBaselineValidator DefaultFloorPlanBaselineValidator()
    {
        return new FloorPlanBaselineValidator(
            new List<IFloorPlanBaselineRule>
            {
                new FloorPlanBaselineRequiredCodeRule("floor plan baseline needs this field"),
                new FloorPlanBaselineAllowedCodeRule(new List<string>()),
                new FloorPlanBaselineRequiredNameRule("floor plan baseline needs a value"),
                new FloorPlanBaselineScoreRangeRule(6.0, 332.4),
            });
    }
}
