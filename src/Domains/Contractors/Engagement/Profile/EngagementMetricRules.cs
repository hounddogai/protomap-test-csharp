namespace Acme.Workforce.Contractors.Engagement.Profile;

/// <summary>A rule a engagement metric must pass. Check returns its problems, or none.</summary>
public interface IEngagementMetricRule
{
    List<string> Check(EngagementMetric item);
}

public sealed record EngagementMetricLocationLengthRule(int MaxLength) : IEngagementMetricRule
{
    public List<string> Check(EngagementMetric item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementMetricNameLengthRule(int MaxLength) : IEngagementMetricRule
{
    public List<string> Check(EngagementMetric item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementMetricRequiredPhoneRule(string Message) : IEngagementMetricRule
{
    public List<string> Check(EngagementMetric item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement metric.</summary>
public sealed record EngagementMetricValidator(List<IEngagementMetricRule> Rules)
{
    public List<string> Validate(EngagementMetric item)
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

    public void RequireValid(EngagementMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementMetric> items)
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

/// <summary>Functions of engagement metric rules.</summary>
public static class EngagementMetricRules
{
    public static EngagementMetricValidator DefaultEngagementMetricValidator()
    {
        return new EngagementMetricValidator(
            new List<IEngagementMetricRule>
            {
                new EngagementMetricLocationLengthRule(120),
                new EngagementMetricNameLengthRule(3600),
                new EngagementMetricRequiredPhoneRule("engagement metric needs this field"),
            });
    }
}
