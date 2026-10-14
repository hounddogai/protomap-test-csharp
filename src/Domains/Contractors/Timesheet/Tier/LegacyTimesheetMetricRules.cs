namespace Acme.Workforce.Contractors.Timesheet.Tier;

/// <summary>A rule a legacy timesheet metric must pass. Check returns its problems, or none.</summary>
public interface ILegacyTimesheetMetricRule
{
    List<string> Check(LegacyTimesheetMetric item);
}

public sealed record LegacyTimesheetMetricCodePrefixRule(string Prefix) : ILegacyTimesheetMetricRule
{
    public List<string> Check(LegacyTimesheetMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTimesheetMetricRequiredPhoneRule(string Message) : ILegacyTimesheetMetricRule
{
    public List<string> Check(LegacyTimesheetMetric item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyTimesheetMetricRequiredTitleRule(string Message) : ILegacyTimesheetMetricRule
{
    public List<string> Check(LegacyTimesheetMetric item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy timesheet metric.</summary>
public sealed record LegacyTimesheetMetricValidator(List<ILegacyTimesheetMetricRule> Rules)
{
    public List<string> Validate(LegacyTimesheetMetric item)
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

    public void RequireValid(LegacyTimesheetMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyTimesheetMetric> items)
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

/// <summary>Functions of legacy timesheet metric rules.</summary>
public static class LegacyTimesheetMetricRules
{
    public static LegacyTimesheetMetricValidator DefaultLegacyTimesheetMetricValidator()
    {
        return new LegacyTimesheetMetricValidator(
            new List<ILegacyTimesheetMetricRule>
            {
                new LegacyTimesheetMetricCodePrefixRule("OPS"),
                new LegacyTimesheetMetricRequiredPhoneRule("legacy timesheet metric needs a value"),
                new LegacyTimesheetMetricRequiredTitleRule("legacy timesheet metric needs a value"),
            });
    }
}
