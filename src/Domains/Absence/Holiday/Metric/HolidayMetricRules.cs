namespace Acme.Workforce.Absence.Holiday.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a holiday metric must pass. Check returns its problems, or none.</summary>
public interface IHolidayMetricRule
{
    List<string> Check(HolidayMetric item);
}

public sealed record HolidayMetricRequiredNameRule(string Message) : IHolidayMetricRule
{
    public List<string> Check(HolidayMetric item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayMetricDepartmentLengthRule(int MaxLength) : IHolidayMetricRule
{
    public List<string> Check(HolidayMetric item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayMetricHoursRangeRule(double Min, double Max) : IHolidayMetricRule
{
    public List<string> Check(HolidayMetric item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record HolidayMetricAllowedCodeRule(List<string> Allowed) : IHolidayMetricRule
{
    public List<string> Check(HolidayMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday metric.</summary>
public sealed record HolidayMetricValidator(List<IHolidayMetricRule> Rules)
{
    public List<string> Validate(HolidayMetric item)
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

    public void RequireValid(HolidayMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayMetric> items)
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

/// <summary>Functions of holiday metric rules.</summary>
public static class HolidayMetricRules
{
    public static HolidayMetricValidator DefaultHolidayMetricValidator()
    {
        return new HolidayMetricValidator(
            new List<IHolidayMetricRule>
            {
                new HolidayMetricRequiredNameRule("holiday metric needs this field"),
                new HolidayMetricDepartmentLengthRule(64),
                new HolidayMetricHoursRangeRule(1.5, 4818.4),
                new HolidayMetricAllowedCodeRule(new List<string> { "legacy", "strict" }),
            });
    }
}
