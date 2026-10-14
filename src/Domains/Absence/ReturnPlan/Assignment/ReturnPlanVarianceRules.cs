namespace Acme.Workforce.Absence.ReturnPlan.Assignment;

/// <summary>A rule a return plan variance must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanVarianceRule
{
    List<string> Check(ReturnPlanVariance item);
}

public sealed record ReturnPlanVarianceCodePrefixRule(string Prefix) : IReturnPlanVarianceRule
{
    public List<string> Check(ReturnPlanVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanVarianceRequiredCodeRule(string Message) : IReturnPlanVarianceRule
{
    public List<string> Check(ReturnPlanVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanVarianceMaximumRangeRule(double Min, double Max) : IReturnPlanVarianceRule
{
    public List<string> Check(ReturnPlanVariance item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a return plan variance.</summary>
public sealed record ReturnPlanVarianceValidator(List<IReturnPlanVarianceRule> Rules)
{
    public List<string> Validate(ReturnPlanVariance item)
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

    public void RequireValid(ReturnPlanVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanVariance> items)
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

/// <summary>Functions of return plan variance rules.</summary>
public static class ReturnPlanVarianceRules
{
    public static ReturnPlanVarianceValidator DefaultReturnPlanVarianceValidator()
    {
        return new ReturnPlanVarianceValidator(
            new List<IReturnPlanVarianceRule>
            {
                new ReturnPlanVarianceCodePrefixRule("OPS"),
                new ReturnPlanVarianceRequiredCodeRule("return plan variance needs a value"),
                new ReturnPlanVarianceMaximumRangeRule(9.3, 2157.8),
            });
    }
}
