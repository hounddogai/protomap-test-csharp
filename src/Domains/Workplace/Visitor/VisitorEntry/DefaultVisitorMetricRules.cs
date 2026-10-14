namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

/// <summary>A rule a default visitor metric must pass. Check returns its problems, or none.</summary>
public interface IDefaultVisitorMetricRule
{
    List<string> Check(DefaultVisitorMetric item);
}

public sealed record DefaultVisitorMetricLabelLengthRule(int MaxLength) : IDefaultVisitorMetricRule
{
    public List<string> Check(DefaultVisitorMetric item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultVisitorMetricRequiredChannelRule(string Message) : IDefaultVisitorMetricRule
{
    public List<string> Check(DefaultVisitorMetric item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultVisitorMetricRetriesRangeRule(int Min, int Max) : IDefaultVisitorMetricRule
{
    public List<string> Check(DefaultVisitorMetric item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default visitor metric.</summary>
public sealed record DefaultVisitorMetricValidator(List<IDefaultVisitorMetricRule> Rules)
{
    public List<string> Validate(DefaultVisitorMetric item)
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

    public void RequireValid(DefaultVisitorMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultVisitorMetric> items)
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

/// <summary>Functions of default visitor metric rules.</summary>
public static class DefaultVisitorMetricRules
{
    public static DefaultVisitorMetricValidator DefaultDefaultVisitorMetricValidator()
    {
        return new DefaultVisitorMetricValidator(
            new List<IDefaultVisitorMetricRule>
            {
                new DefaultVisitorMetricLabelLengthRule(32),
                new DefaultVisitorMetricRequiredChannelRule("default visitor metric needs this field"),
                new DefaultVisitorMetricRetriesRangeRule(32, 1000),
            });
    }
}
