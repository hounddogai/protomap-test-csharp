namespace Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>A rule a scheduled visitor metric must pass. Check returns its problems, or none.</summary>
public interface IScheduledVisitorMetricRule
{
    List<string> Check(ScheduledVisitorMetric item);
}

public sealed record ScheduledVisitorMetricCostCenterLengthRule(int MaxLength) : IScheduledVisitorMetricRule
{
    public List<string> Check(ScheduledVisitorMetric item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledVisitorMetricCodePrefixRule(string Prefix) : IScheduledVisitorMetricRule
{
    public List<string> Check(ScheduledVisitorMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledVisitorMetricRequiredCostCenterRule(string Message) : IScheduledVisitorMetricRule
{
    public List<string> Check(ScheduledVisitorMetric item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled visitor metric.</summary>
public sealed record ScheduledVisitorMetricValidator(List<IScheduledVisitorMetricRule> Rules)
{
    public List<string> Validate(ScheduledVisitorMetric item)
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

    public void RequireValid(ScheduledVisitorMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledVisitorMetric> items)
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

/// <summary>Functions of scheduled visitor metric rules.</summary>
public static class ScheduledVisitorMetricRules
{
    public static ScheduledVisitorMetricValidator DefaultScheduledVisitorMetricValidator()
    {
        return new ScheduledVisitorMetricValidator(
            new List<IScheduledVisitorMetricRule>
            {
                new ScheduledVisitorMetricCostCenterLengthRule(32),
                new ScheduledVisitorMetricCodePrefixRule("REF"),
                new ScheduledVisitorMetricRequiredCostCenterRule("scheduled visitor metric needs an entry"),
            });
    }
}
