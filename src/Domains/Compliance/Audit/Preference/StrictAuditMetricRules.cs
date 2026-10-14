namespace Acme.Workforce.Compliance.Audit.Preference;

/// <summary>A rule a strict audit metric must pass. Check returns its problems, or none.</summary>
public interface IStrictAuditMetricRule
{
    List<string> Check(StrictAuditMetric item);
}

public sealed record StrictAuditMetricPriorityRangeRule(int Min, int Max) : IStrictAuditMetricRule
{
    public List<string> Check(StrictAuditMetric item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAuditMetricRequiredLabelRule(string Message) : IStrictAuditMetricRule
{
    public List<string> Check(StrictAuditMetric item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictAuditMetricCostRangeRule(double Min, double Max) : IStrictAuditMetricRule
{
    public List<string> Check(StrictAuditMetric item)
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

/// <summary>Runs every rule of a strict audit metric.</summary>
public sealed record StrictAuditMetricValidator(List<IStrictAuditMetricRule> Rules)
{
    public List<string> Validate(StrictAuditMetric item)
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

    public void RequireValid(StrictAuditMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictAuditMetric> items)
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

/// <summary>Functions of strict audit metric rules.</summary>
public static class StrictAuditMetricRules
{
    public static StrictAuditMetricValidator DefaultStrictAuditMetricValidator()
    {
        return new StrictAuditMetricValidator(
            new List<IStrictAuditMetricRule>
            {
                new StrictAuditMetricPriorityRangeRule(32, 3600),
                new StrictAuditMetricRequiredLabelRule("strict audit metric needs an entry"),
                new StrictAuditMetricCostRangeRule(6.5, 1577.5),
            });
    }
}
