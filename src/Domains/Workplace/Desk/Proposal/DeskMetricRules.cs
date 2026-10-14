namespace Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>A rule a desk metric must pass. Check returns its problems, or none.</summary>
public interface IDeskMetricRule
{
    List<string> Check(DeskMetric item);
}

public sealed record DeskMetricRequiredCodeRule(string Message) : IDeskMetricRule
{
    public List<string> Check(DeskMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskMetricCodePrefixRule(string Prefix) : IDeskMetricRule
{
    public List<string> Check(DeskMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeskMetricNameLengthRule(int MaxLength) : IDeskMetricRule
{
    public List<string> Check(DeskMetric item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskMetricRatioRangeRule(double Min, double Max) : IDeskMetricRule
{
    public List<string> Check(DeskMetric item)
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

public sealed record DeskMetricRequiredEmployeeIdRule(string Message) : IDeskMetricRule
{
    public List<string> Check(DeskMetric item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskMetricVisibleGuardRule(int Limit) : IDeskMetricRule
{
    public List<string> Check(DeskMetric item)
    {
        if (item.Visible && item.StartsAt > Limit)
        {
            return new List<string> { $"starts at {item.StartsAt} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk metric.</summary>
public sealed record DeskMetricValidator(List<IDeskMetricRule> Rules)
{
    public List<string> Validate(DeskMetric item)
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

    public void RequireValid(DeskMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskMetric> items)
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

/// <summary>Functions of desk metric rules.</summary>
public static class DeskMetricRules
{
    public static DeskMetricValidator DefaultDeskMetricValidator()
    {
        return new DeskMetricValidator(
            new List<IDeskMetricRule>
            {
                new DeskMetricRequiredCodeRule("desk metric needs this field"),
                new DeskMetricCodePrefixRule("X"),
                new DeskMetricNameLengthRule(32),
                new DeskMetricRatioRangeRule(2.6, 1350.4),
                new DeskMetricRequiredEmployeeIdRule("desk metric needs this field"),
                new DeskMetricVisibleGuardRule(32),
            });
    }
}
