namespace Acme.Workforce.Engagement.ActionPlan.Metric;

/// <summary>A rule a default action plan metric must pass. Check returns its problems, or none.</summary>
public interface IDefaultActionPlanMetricRule
{
    List<string> Check(DefaultActionPlanMetric item);
}

public sealed record DefaultActionPlanMetricCodePrefixRule(string Prefix) : IDefaultActionPlanMetricRule
{
    public List<string> Check(DefaultActionPlanMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultActionPlanMetricRequiredGradeRule(string Message) : IDefaultActionPlanMetricRule
{
    public List<string> Check(DefaultActionPlanMetric item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultActionPlanMetricRequiredCodeRule(string Message) : IDefaultActionPlanMetricRule
{
    public List<string> Check(DefaultActionPlanMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultActionPlanMetricGradeLengthRule(int MaxLength) : IDefaultActionPlanMetricRule
{
    public List<string> Check(DefaultActionPlanMetric item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultActionPlanMetricTitleLengthRule(int MaxLength) : IDefaultActionPlanMetricRule
{
    public List<string> Check(DefaultActionPlanMetric item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default action plan metric.</summary>
public sealed record DefaultActionPlanMetricValidator(List<IDefaultActionPlanMetricRule> Rules)
{
    public List<string> Validate(DefaultActionPlanMetric item)
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

    public void RequireValid(DefaultActionPlanMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultActionPlanMetric> items)
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

/// <summary>Functions of default action plan metric rules.</summary>
public static class DefaultActionPlanMetricRules
{
    public static DefaultActionPlanMetricValidator DefaultDefaultActionPlanMetricValidator()
    {
        return new DefaultActionPlanMetricValidator(
            new List<IDefaultActionPlanMetricRule>
            {
                new DefaultActionPlanMetricCodePrefixRule("Q"),
                new DefaultActionPlanMetricRequiredGradeRule("default action plan metric needs this field"),
                new DefaultActionPlanMetricRequiredCodeRule("default action plan metric needs this field"),
                new DefaultActionPlanMetricGradeLengthRule(120),
                new DefaultActionPlanMetricTitleLengthRule(64),
            });
    }
}
