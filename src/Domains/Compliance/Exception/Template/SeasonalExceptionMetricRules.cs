namespace Acme.Workforce.Compliance.Exception.Template;

/// <summary>A rule a seasonal exception metric must pass. Check returns its problems, or none.</summary>
public interface ISeasonalExceptionMetricRule
{
    List<string> Check(SeasonalExceptionMetric item);
}

public sealed record SeasonalExceptionMetricRatioRangeRule(double Min, double Max) : ISeasonalExceptionMetricRule
{
    public List<string> Check(SeasonalExceptionMetric item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalExceptionMetricRequiredCodeRule(string Message) : ISeasonalExceptionMetricRule
{
    public List<string> Check(SeasonalExceptionMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalExceptionMetricPinnedGuardRule(int Limit) : ISeasonalExceptionMetricRule
{
    public List<string> Check(SeasonalExceptionMetric item)
    {
        if (item.Pinned && item.Attempts > Limit)
        {
            return new List<string> { $"attempts {item.Attempts} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal exception metric.</summary>
public sealed record SeasonalExceptionMetricValidator(List<ISeasonalExceptionMetricRule> Rules)
{
    public List<string> Validate(SeasonalExceptionMetric item)
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

    public void RequireValid(SeasonalExceptionMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalExceptionMetric> items)
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

/// <summary>Functions of seasonal exception metric rules.</summary>
public static class SeasonalExceptionMetricRules
{
    public static SeasonalExceptionMetricValidator DefaultSeasonalExceptionMetricValidator()
    {
        return new SeasonalExceptionMetricValidator(
            new List<ISeasonalExceptionMetricRule>
            {
                new SeasonalExceptionMetricRatioRangeRule(0.4, 4451.1),
                new SeasonalExceptionMetricRequiredCodeRule("seasonal exception metric needs an entry"),
                new SeasonalExceptionMetricPinnedGuardRule(80),
            });
    }
}
