namespace Acme.Workforce.Workplace.Visitor.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive visitor metric must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveVisitorMetricRule
{
    List<string> Check(AdaptiveVisitorMetric item);
}

public sealed record AdaptiveVisitorMetricAllowedTitleRule(List<string> Allowed) : IAdaptiveVisitorMetricRule
{
    public List<string> Check(AdaptiveVisitorMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is stale" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveVisitorMetricVersionRangeRule(int Min, int Max) : IAdaptiveVisitorMetricRule
{
    public List<string> Check(AdaptiveVisitorMetric item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveVisitorMetricTitleLengthRule(int MaxLength) : IAdaptiveVisitorMetricRule
{
    public List<string> Check(AdaptiveVisitorMetric item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveVisitorMetricRequiredTitleRule(string Message) : IAdaptiveVisitorMetricRule
{
    public List<string> Check(AdaptiveVisitorMetric item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveVisitorMetricOwnerIdLengthRule(int MaxLength) : IAdaptiveVisitorMetricRule
{
    public List<string> Check(AdaptiveVisitorMetric item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive visitor metric.</summary>
public sealed record AdaptiveVisitorMetricValidator(List<IAdaptiveVisitorMetricRule> Rules)
{
    public List<string> Validate(AdaptiveVisitorMetric item)
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

    public void RequireValid(AdaptiveVisitorMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveVisitorMetric> items)
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

/// <summary>Functions of adaptive visitor metric rules.</summary>
public static class AdaptiveVisitorMetricRules
{
    public static AdaptiveVisitorMetricValidator DefaultAdaptiveVisitorMetricValidator()
    {
        return new AdaptiveVisitorMetricValidator(
            new List<IAdaptiveVisitorMetricRule>
            {
                new AdaptiveVisitorMetricAllowedTitleRule(new List<string>()),
                new AdaptiveVisitorMetricVersionRangeRule(16, 32),
                new AdaptiveVisitorMetricTitleLengthRule(64),
                new AdaptiveVisitorMetricRequiredTitleRule("adaptive visitor metric needs an entry"),
                new AdaptiveVisitorMetricOwnerIdLengthRule(16),
            });
    }
}
