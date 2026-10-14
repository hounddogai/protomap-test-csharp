namespace Acme.Workforce.Compensation.EquityGrant.Batch;

/// <summary>A rule a strict equity grant metric must pass. Check returns its problems, or none.</summary>
public interface IStrictEquityGrantMetricRule
{
    List<string> Check(StrictEquityGrantMetric item);
}

public sealed record StrictEquityGrantMetricLimitRangeRule(int Min, int Max) : IStrictEquityGrantMetricRule
{
    public List<string> Check(StrictEquityGrantMetric item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictEquityGrantMetricRatingRangeRule(double Min, double Max) : IStrictEquityGrantMetricRule
{
    public List<string> Check(StrictEquityGrantMetric item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictEquityGrantMetricUnitLengthRule(int MaxLength) : IStrictEquityGrantMetricRule
{
    public List<string> Check(StrictEquityGrantMetric item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictEquityGrantMetricMaximumRangeRule(double Min, double Max) : IStrictEquityGrantMetricRule
{
    public List<string> Check(StrictEquityGrantMetric item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict equity grant metric.</summary>
public sealed record StrictEquityGrantMetricValidator(List<IStrictEquityGrantMetricRule> Rules)
{
    public List<string> Validate(StrictEquityGrantMetric item)
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

    public void RequireValid(StrictEquityGrantMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictEquityGrantMetric> items)
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

/// <summary>Functions of strict equity grant metric rules.</summary>
public static class StrictEquityGrantMetricRules
{
    public static StrictEquityGrantMetricValidator DefaultStrictEquityGrantMetricValidator()
    {
        return new StrictEquityGrantMetricValidator(
            new List<IStrictEquityGrantMetricRule>
            {
                new StrictEquityGrantMetricLimitRangeRule(80, 86400),
                new StrictEquityGrantMetricRatingRangeRule(6.8, 2717.9),
                new StrictEquityGrantMetricUnitLengthRule(64),
                new StrictEquityGrantMetricMaximumRangeRule(5.5, 1646.5),
            });
    }
}
