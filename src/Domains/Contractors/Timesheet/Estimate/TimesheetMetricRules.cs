namespace Acme.Workforce.Contractors.Timesheet.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a timesheet metric must pass. Check returns its problems, or none.</summary>
public interface ITimesheetMetricRule
{
    List<string> Check(TimesheetMetric item);
}

public sealed record TimesheetMetricAllowedContactEmailRule(List<string> Allowed) : ITimesheetMetricRule
{
    public List<string> Check(TimesheetMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is expired" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetMetricRatioRangeRule(double Min, double Max) : ITimesheetMetricRule
{
    public List<string> Check(TimesheetMetric item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TimesheetMetricCodeLengthRule(int MaxLength) : ITimesheetMetricRule
{
    public List<string> Check(TimesheetMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetMetricDiscountRangeRule(double Min, double Max) : ITimesheetMetricRule
{
    public List<string> Check(TimesheetMetric item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TimesheetMetricRequiredContactEmailRule(string Message) : ITimesheetMetricRule
{
    public List<string> Check(TimesheetMetric item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetMetricCodePrefixRule(string Prefix) : ITimesheetMetricRule
{
    public List<string> Check(TimesheetMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet metric.</summary>
public sealed record TimesheetMetricValidator(List<ITimesheetMetricRule> Rules)
{
    public List<string> Validate(TimesheetMetric item)
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

    public void RequireValid(TimesheetMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetMetric> items)
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

/// <summary>Functions of timesheet metric rules.</summary>
public static class TimesheetMetricRules
{
    public static TimesheetMetricValidator DefaultTimesheetMetricValidator()
    {
        return new TimesheetMetricValidator(
            new List<ITimesheetMetricRule>
            {
                new TimesheetMetricAllowedContactEmailRule(new List<string>()),
                new TimesheetMetricRatioRangeRule(5.6, 4717.9),
                new TimesheetMetricCodeLengthRule(80),
                new TimesheetMetricDiscountRangeRule(5.4, 2097.2),
                new TimesheetMetricRequiredContactEmailRule("timesheet metric needs an entry"),
                new TimesheetMetricCodePrefixRule("OPS"),
            });
    }
}
