namespace Acme.Workforce.Compensation.EquityGrant.GrantMetric;

/// <summary>A rule a rolling equity grant metric must pass. Check returns its problems, or none.</summary>
public interface IRollingEquityGrantMetricRule
{
    List<string> Check(RollingEquityGrantMetric item);
}

public sealed record RollingEquityGrantMetricNameLengthRule(int MaxLength) : IRollingEquityGrantMetricRule
{
    public List<string> Check(RollingEquityGrantMetric item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingEquityGrantMetricTimezoneLengthRule(int MaxLength) : IRollingEquityGrantMetricRule
{
    public List<string> Check(RollingEquityGrantMetric item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingEquityGrantMetricRequiredTitleRule(string Message) : IRollingEquityGrantMetricRule
{
    public List<string> Check(RollingEquityGrantMetric item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling equity grant metric.</summary>
public sealed record RollingEquityGrantMetricValidator(List<IRollingEquityGrantMetricRule> Rules)
{
    public List<string> Validate(RollingEquityGrantMetric item)
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

    public void RequireValid(RollingEquityGrantMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingEquityGrantMetric> items)
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

/// <summary>Functions of rolling equity grant metric rules.</summary>
public static class RollingEquityGrantMetricRules
{
    public static RollingEquityGrantMetricValidator DefaultRollingEquityGrantMetricValidator()
    {
        return new RollingEquityGrantMetricValidator(
            new List<IRollingEquityGrantMetricRule>
            {
                new RollingEquityGrantMetricNameLengthRule(80),
                new RollingEquityGrantMetricTimezoneLengthRule(86400),
                new RollingEquityGrantMetricRequiredTitleRule("rolling equity grant metric needs this field"),
            });
    }
}
