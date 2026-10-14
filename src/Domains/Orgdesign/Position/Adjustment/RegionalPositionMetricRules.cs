namespace Acme.Workforce.Orgdesign.Position.Adjustment;

/// <summary>A rule a regional position metric must pass. Check returns its problems, or none.</summary>
public interface IRegionalPositionMetricRule
{
    List<string> Check(RegionalPositionMetric item);
}

public sealed record RegionalPositionMetricRequiredTimezoneRule(string Message) : IRegionalPositionMetricRule
{
    public List<string> Check(RegionalPositionMetric item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalPositionMetricRequiredCodeRule(string Message) : IRegionalPositionMetricRule
{
    public List<string> Check(RegionalPositionMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalPositionMetricGraceDaysRangeRule(int Min, int Max) : IRegionalPositionMetricRule
{
    public List<string> Check(RegionalPositionMetric item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalPositionMetricCodeLengthRule(int MaxLength) : IRegionalPositionMetricRule
{
    public List<string> Check(RegionalPositionMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional position metric.</summary>
public sealed record RegionalPositionMetricValidator(List<IRegionalPositionMetricRule> Rules)
{
    public List<string> Validate(RegionalPositionMetric item)
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

    public void RequireValid(RegionalPositionMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalPositionMetric> items)
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

/// <summary>Functions of regional position metric rules.</summary>
public static class RegionalPositionMetricRules
{
    public static RegionalPositionMetricValidator DefaultRegionalPositionMetricValidator()
    {
        return new RegionalPositionMetricValidator(
            new List<IRegionalPositionMetricRule>
            {
                new RegionalPositionMetricRequiredTimezoneRule("regional position metric needs an entry"),
                new RegionalPositionMetricRequiredCodeRule("regional position metric needs a value"),
                new RegionalPositionMetricGraceDaysRangeRule(1000, 86400),
                new RegionalPositionMetricCodeLengthRule(255),
            });
    }
}
