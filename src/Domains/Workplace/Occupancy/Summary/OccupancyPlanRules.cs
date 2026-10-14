namespace Acme.Workforce.Workplace.Occupancy.Summary;

/// <summary>A rule a occupancy plan must pass. Check returns its problems, or none.</summary>
public interface IOccupancyPlanRule
{
    List<string> Check(OccupancyPlan item);
}

public sealed record OccupancyPlanRequiredNameRule(string Message) : IOccupancyPlanRule
{
    public List<string> Check(OccupancyPlan item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyPlanRequiredCategoryRule(string Message) : IOccupancyPlanRule
{
    public List<string> Check(OccupancyPlan item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyPlanWeightRangeRule(double Min, double Max) : IOccupancyPlanRule
{
    public List<string> Check(OccupancyPlan item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record OccupancyPlanCodePrefixRule(string Prefix) : IOccupancyPlanRule
{
    public List<string> Check(OccupancyPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyPlanNameLengthRule(int MaxLength) : IOccupancyPlanRule
{
    public List<string> Check(OccupancyPlan item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy plan.</summary>
public sealed record OccupancyPlanValidator(List<IOccupancyPlanRule> Rules)
{
    public List<string> Validate(OccupancyPlan item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(OccupancyPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyPlan> items)
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

/// <summary>Functions of occupancy plan rules.</summary>
public static class OccupancyPlanRules
{
    public static OccupancyPlanValidator DefaultOccupancyPlanValidator()
    {
        return new OccupancyPlanValidator(
            new List<IOccupancyPlanRule>
            {
                new OccupancyPlanRequiredNameRule("occupancy plan needs an entry"),
                new OccupancyPlanRequiredCategoryRule("occupancy plan needs a value"),
                new OccupancyPlanWeightRangeRule(0.3, 4714.8),
                new OccupancyPlanCodePrefixRule("ACM"),
                new OccupancyPlanNameLengthRule(120),
            });
    }
}
