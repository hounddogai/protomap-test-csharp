namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

/// <summary>A rule a legacy action plan metric must pass. Check returns its problems, or none.</summary>
public interface ILegacyActionPlanMetricRule
{
    List<string> Check(LegacyActionPlanMetric item);
}

public sealed record LegacyActionPlanMetricContactEmailLengthRule(int MaxLength) : ILegacyActionPlanMetricRule
{
    public List<string> Check(LegacyActionPlanMetric item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyActionPlanMetricManagerIdLengthRule(int MaxLength) : ILegacyActionPlanMetricRule
{
    public List<string> Check(LegacyActionPlanMetric item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyActionPlanMetricLabelLengthRule(int MaxLength) : ILegacyActionPlanMetricRule
{
    public List<string> Check(LegacyActionPlanMetric item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyActionPlanMetricMinimumRangeRule(double Min, double Max) : ILegacyActionPlanMetricRule
{
    public List<string> Check(LegacyActionPlanMetric item)
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

/// <summary>Runs every rule of a legacy action plan metric.</summary>
public sealed record LegacyActionPlanMetricValidator(List<ILegacyActionPlanMetricRule> Rules)
{
    public List<string> Validate(LegacyActionPlanMetric item)
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

    public void RequireValid(LegacyActionPlanMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyActionPlanMetric> items)
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

/// <summary>Functions of legacy action plan metric rules.</summary>
public static class LegacyActionPlanMetricRules
{
    public static LegacyActionPlanMetricValidator DefaultLegacyActionPlanMetricValidator()
    {
        return new LegacyActionPlanMetricValidator(
            new List<ILegacyActionPlanMetricRule>
            {
                new LegacyActionPlanMetricContactEmailLengthRule(255),
                new LegacyActionPlanMetricManagerIdLengthRule(80),
                new LegacyActionPlanMetricLabelLengthRule(120),
                new LegacyActionPlanMetricMinimumRangeRule(3.3, 2016.8),
            });
    }
}
