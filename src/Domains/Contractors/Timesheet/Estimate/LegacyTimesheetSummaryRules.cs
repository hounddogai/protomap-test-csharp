namespace Acme.Workforce.Contractors.Timesheet.Estimate;

/// <summary>A rule a legacy timesheet summary must pass. Check returns its problems, or none.</summary>
public interface ILegacyTimesheetSummaryRule
{
    List<string> Check(LegacyTimesheetSummary item);
}

public sealed record LegacyTimesheetSummaryRequiredBadgeRule(string Message) : ILegacyTimesheetSummaryRule
{
    public List<string> Check(LegacyTimesheetSummary item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyTimesheetSummaryRequiredCodeRule(string Message) : ILegacyTimesheetSummaryRule
{
    public List<string> Check(LegacyTimesheetSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyTimesheetSummaryRateRangeRule(double Min, double Max) : ILegacyTimesheetSummaryRule
{
    public List<string> Check(LegacyTimesheetSummary item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyTimesheetSummaryRequiredCostCenterRule(string Message) : ILegacyTimesheetSummaryRule
{
    public List<string> Check(LegacyTimesheetSummary item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyTimesheetSummaryBadgeLengthRule(int MaxLength) : ILegacyTimesheetSummaryRule
{
    public List<string> Check(LegacyTimesheetSummary item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy timesheet summary.</summary>
public sealed record LegacyTimesheetSummaryValidator(List<ILegacyTimesheetSummaryRule> Rules)
{
    public List<string> Validate(LegacyTimesheetSummary item)
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

    public void RequireValid(LegacyTimesheetSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyTimesheetSummary> items)
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

/// <summary>Functions of legacy timesheet summary rules.</summary>
public static class LegacyTimesheetSummaryRules
{
    public static LegacyTimesheetSummaryValidator DefaultLegacyTimesheetSummaryValidator()
    {
        return new LegacyTimesheetSummaryValidator(
            new List<ILegacyTimesheetSummaryRule>
            {
                new LegacyTimesheetSummaryRequiredBadgeRule("legacy timesheet summary needs this field"),
                new LegacyTimesheetSummaryRequiredCodeRule("legacy timesheet summary needs an entry"),
                new LegacyTimesheetSummaryRateRangeRule(7.2, 689.0),
                new LegacyTimesheetSummaryRequiredCostCenterRule("legacy timesheet summary needs this field"),
                new LegacyTimesheetSummaryBadgeLengthRule(86400),
            });
    }
}
