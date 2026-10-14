namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

/// <summary>A rule a weighted timesheet metric must pass. Check returns its problems, or none.</summary>
public interface IWeightedTimesheetMetricRule
{
    List<string> Check(WeightedTimesheetMetric item);
}

public sealed record WeightedTimesheetMetricCodePrefixRule(string Prefix) : IWeightedTimesheetMetricRule
{
    public List<string> Check(WeightedTimesheetMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedTimesheetMetricRequiredChannelRule(string Message) : IWeightedTimesheetMetricRule
{
    public List<string> Check(WeightedTimesheetMetric item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedTimesheetMetricRequiredTitleRule(string Message) : IWeightedTimesheetMetricRule
{
    public List<string> Check(WeightedTimesheetMetric item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedTimesheetMetricChannelLengthRule(int MaxLength) : IWeightedTimesheetMetricRule
{
    public List<string> Check(WeightedTimesheetMetric item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedTimesheetMetricMarginRangeRule(double Min, double Max) : IWeightedTimesheetMetricRule
{
    public List<string> Check(WeightedTimesheetMetric item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a weighted timesheet metric.</summary>
public sealed record WeightedTimesheetMetricValidator(List<IWeightedTimesheetMetricRule> Rules)
{
    public List<string> Validate(WeightedTimesheetMetric item)
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

    public void RequireValid(WeightedTimesheetMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedTimesheetMetric> items)
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

/// <summary>Functions of weighted timesheet metric rules.</summary>
public static class WeightedTimesheetMetricRules
{
    public static WeightedTimesheetMetricValidator DefaultWeightedTimesheetMetricValidator()
    {
        return new WeightedTimesheetMetricValidator(
            new List<IWeightedTimesheetMetricRule>
            {
                new WeightedTimesheetMetricCodePrefixRule("REF"),
                new WeightedTimesheetMetricRequiredChannelRule("weighted timesheet metric needs a value"),
                new WeightedTimesheetMetricRequiredTitleRule("weighted timesheet metric needs a value"),
                new WeightedTimesheetMetricChannelLengthRule(80),
                new WeightedTimesheetMetricMarginRangeRule(3.8, 3292.7),
            });
    }
}
