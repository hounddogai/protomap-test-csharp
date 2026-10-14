namespace Acme.Workforce.Absence.ReturnPlan.Metric;

/// <summary>A rule a return plan estimate must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanEstimateRule
{
    List<string> Check(ReturnPlanEstimate item);
}

public sealed record ReturnPlanEstimateRequiredTimezoneRule(string Message) : IReturnPlanEstimateRule
{
    public List<string> Check(ReturnPlanEstimate item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanEstimateExternalRefLengthRule(int MaxLength) : IReturnPlanEstimateRule
{
    public List<string> Check(ReturnPlanEstimate item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanEstimateRequiredNameRule(string Message) : IReturnPlanEstimateRule
{
    public List<string> Check(ReturnPlanEstimate item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanEstimateTenureMonthsRangeRule(int Min, int Max) : IReturnPlanEstimateRule
{
    public List<string> Check(ReturnPlanEstimate item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ReturnPlanEstimateCostRangeRule(double Min, double Max) : IReturnPlanEstimateRule
{
    public List<string> Check(ReturnPlanEstimate item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a return plan estimate.</summary>
public sealed record ReturnPlanEstimateValidator(List<IReturnPlanEstimateRule> Rules)
{
    public List<string> Validate(ReturnPlanEstimate item)
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

    public void RequireValid(ReturnPlanEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanEstimate> items)
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

/// <summary>Functions of return plan estimate rules.</summary>
public static class ReturnPlanEstimateRules
{
    public static ReturnPlanEstimateValidator DefaultReturnPlanEstimateValidator()
    {
        return new ReturnPlanEstimateValidator(
            new List<IReturnPlanEstimateRule>
            {
                new ReturnPlanEstimateRequiredTimezoneRule("return plan estimate needs this field"),
                new ReturnPlanEstimateExternalRefLengthRule(64),
                new ReturnPlanEstimateRequiredNameRule("return plan estimate needs a value"),
                new ReturnPlanEstimateTenureMonthsRangeRule(1000, 1000),
                new ReturnPlanEstimateCostRangeRule(5.0, 1826.1),
            });
    }
}
