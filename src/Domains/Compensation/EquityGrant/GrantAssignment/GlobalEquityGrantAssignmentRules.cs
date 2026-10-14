namespace Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

/// <summary>A rule a global equity grant assignment must pass. Check returns its problems, or none.</summary>
public interface IGlobalEquityGrantAssignmentRule
{
    List<string> Check(GlobalEquityGrantAssignment item);
}

public sealed record GlobalEquityGrantAssignmentScoreRangeRule(double Min, double Max) : IGlobalEquityGrantAssignmentRule
{
    public List<string> Check(GlobalEquityGrantAssignment item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalEquityGrantAssignmentRequiredLabelRule(string Message) : IGlobalEquityGrantAssignmentRule
{
    public List<string> Check(GlobalEquityGrantAssignment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalEquityGrantAssignmentQuantityRangeRule(int Min, int Max) : IGlobalEquityGrantAssignmentRule
{
    public List<string> Check(GlobalEquityGrantAssignment item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalEquityGrantAssignmentWeightRangeRule(double Min, double Max) : IGlobalEquityGrantAssignmentRule
{
    public List<string> Check(GlobalEquityGrantAssignment item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global equity grant assignment.</summary>
public sealed record GlobalEquityGrantAssignmentValidator(List<IGlobalEquityGrantAssignmentRule> Rules)
{
    public List<string> Validate(GlobalEquityGrantAssignment item)
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

    public void RequireValid(GlobalEquityGrantAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalEquityGrantAssignment> items)
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

/// <summary>Functions of global equity grant assignment rules.</summary>
public static class GlobalEquityGrantAssignmentRules
{
    public static GlobalEquityGrantAssignmentValidator DefaultGlobalEquityGrantAssignmentValidator()
    {
        return new GlobalEquityGrantAssignmentValidator(
            new List<IGlobalEquityGrantAssignmentRule>
            {
                new GlobalEquityGrantAssignmentScoreRangeRule(8.2, 1560.2),
                new GlobalEquityGrantAssignmentRequiredLabelRule("global equity grant assignment needs a value"),
                new GlobalEquityGrantAssignmentQuantityRangeRule(16, 120),
                new GlobalEquityGrantAssignmentWeightRangeRule(7.7, 645.4),
            });
    }
}
