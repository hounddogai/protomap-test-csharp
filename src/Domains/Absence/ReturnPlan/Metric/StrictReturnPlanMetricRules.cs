namespace Acme.Workforce.Absence.ReturnPlan.Metric;

/// <summary>A rule a strict return plan metric must pass. Check returns its problems, or none.</summary>
public interface IStrictReturnPlanMetricRule
{
    List<string> Check(StrictReturnPlanMetric item);
}

public sealed record StrictReturnPlanMetricRequiredReviewerRule(string Message) : IStrictReturnPlanMetricRule
{
    public List<string> Check(StrictReturnPlanMetric item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictReturnPlanMetricRequiredCodeRule(string Message) : IStrictReturnPlanMetricRule
{
    public List<string> Check(StrictReturnPlanMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictReturnPlanMetricRequiredManagerIdRule(string Message) : IStrictReturnPlanMetricRule
{
    public List<string> Check(StrictReturnPlanMetric item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict return plan metric.</summary>
public sealed record StrictReturnPlanMetricValidator(List<IStrictReturnPlanMetricRule> Rules)
{
    public List<string> Validate(StrictReturnPlanMetric item)
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

    public void RequireValid(StrictReturnPlanMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictReturnPlanMetric> items)
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

/// <summary>Functions of strict return plan metric rules.</summary>
public static class StrictReturnPlanMetricRules
{
    public static StrictReturnPlanMetricValidator DefaultStrictReturnPlanMetricValidator()
    {
        return new StrictReturnPlanMetricValidator(
            new List<IStrictReturnPlanMetricRule>
            {
                new StrictReturnPlanMetricRequiredReviewerRule("strict return plan metric needs this field"),
                new StrictReturnPlanMetricRequiredCodeRule("strict return plan metric needs an entry"),
                new StrictReturnPlanMetricRequiredManagerIdRule("strict return plan metric needs this field"),
            });
    }
}
