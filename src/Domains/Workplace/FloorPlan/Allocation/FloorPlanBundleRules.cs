namespace Acme.Workforce.Workplace.FloorPlan.Allocation;

/// <summary>A rule a floor plan bundle must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanBundleRule
{
    List<string> Check(FloorPlanBundle item);
}

public sealed record FloorPlanBundleCodePrefixRule(string Prefix) : IFloorPlanBundleRule
{
    public List<string> Check(FloorPlanBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanBundleSalaryBandLengthRule(int MaxLength) : IFloorPlanBundleRule
{
    public List<string> Check(FloorPlanBundle item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanBundleMinimumRangeRule(double Min, double Max) : IFloorPlanBundleRule
{
    public List<string> Check(FloorPlanBundle item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FloorPlanBundleRequiredSalaryBandRule(string Message) : IFloorPlanBundleRule
{
    public List<string> Check(FloorPlanBundle item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan bundle.</summary>
public sealed record FloorPlanBundleValidator(List<IFloorPlanBundleRule> Rules)
{
    public List<string> Validate(FloorPlanBundle item)
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

    public void RequireValid(FloorPlanBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanBundle> items)
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

/// <summary>Functions of floor plan bundle rules.</summary>
public static class FloorPlanBundleRules
{
    public static FloorPlanBundleValidator DefaultFloorPlanBundleValidator()
    {
        return new FloorPlanBundleValidator(
            new List<IFloorPlanBundleRule>
            {
                new FloorPlanBundleCodePrefixRule("ACM"),
                new FloorPlanBundleSalaryBandLengthRule(120),
                new FloorPlanBundleMinimumRangeRule(4.4, 2253.1),
                new FloorPlanBundleRequiredSalaryBandRule("floor plan bundle needs this field"),
            });
    }
}
