namespace Acme.Workforce.Workplace.FloorPlan.Baseline;

/// <summary>A rule a floor plan estimate must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanEstimateRule
{
    List<string> Check(FloorPlanEstimate item);
}

public sealed record FloorPlanEstimateRequiredLabelRule(string Message) : IFloorPlanEstimateRule
{
    public List<string> Check(FloorPlanEstimate item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanEstimateRequiredSummaryRule(string Message) : IFloorPlanEstimateRule
{
    public List<string> Check(FloorPlanEstimate item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanEstimateCodePrefixRule(string Prefix) : IFloorPlanEstimateRule
{
    public List<string> Check(FloorPlanEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanEstimateRequiredLocaleRule(string Message) : IFloorPlanEstimateRule
{
    public List<string> Check(FloorPlanEstimate item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanEstimateLocaleLengthRule(int MaxLength) : IFloorPlanEstimateRule
{
    public List<string> Check(FloorPlanEstimate item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan estimate.</summary>
public sealed record FloorPlanEstimateValidator(List<IFloorPlanEstimateRule> Rules)
{
    public List<string> Validate(FloorPlanEstimate item)
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

    public void RequireValid(FloorPlanEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanEstimate> items)
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

/// <summary>Functions of floor plan estimate rules.</summary>
public static class FloorPlanEstimateRules
{
    public static FloorPlanEstimateValidator DefaultFloorPlanEstimateValidator()
    {
        return new FloorPlanEstimateValidator(
            new List<IFloorPlanEstimateRule>
            {
                new FloorPlanEstimateRequiredLabelRule("floor plan estimate needs an entry"),
                new FloorPlanEstimateRequiredSummaryRule("floor plan estimate needs a value"),
                new FloorPlanEstimateCodePrefixRule("REF"),
                new FloorPlanEstimateRequiredLocaleRule("floor plan estimate needs this field"),
                new FloorPlanEstimateLocaleLengthRule(32),
            });
    }
}
