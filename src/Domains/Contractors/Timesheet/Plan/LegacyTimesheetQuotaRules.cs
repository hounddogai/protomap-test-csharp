namespace Acme.Workforce.Contractors.Timesheet.Plan;

/// <summary>A rule a legacy timesheet quota must pass. Check returns its problems, or none.</summary>
public interface ILegacyTimesheetQuotaRule
{
    List<string> Check(LegacyTimesheetQuota item);
}

public sealed record LegacyTimesheetQuotaCostRangeRule(double Min, double Max) : ILegacyTimesheetQuotaRule
{
    public List<string> Check(LegacyTimesheetQuota item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTimesheetQuotaAttemptsRangeRule(int Min, int Max) : ILegacyTimesheetQuotaRule
{
    public List<string> Check(LegacyTimesheetQuota item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTimesheetQuotaRequiredTitleRule(string Message) : ILegacyTimesheetQuotaRule
{
    public List<string> Check(LegacyTimesheetQuota item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyTimesheetQuotaCodePrefixRule(string Prefix) : ILegacyTimesheetQuotaRule
{
    public List<string> Check(LegacyTimesheetQuota item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTimesheetQuotaCodeLengthRule(int MaxLength) : ILegacyTimesheetQuotaRule
{
    public List<string> Check(LegacyTimesheetQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy timesheet quota.</summary>
public sealed record LegacyTimesheetQuotaValidator(List<ILegacyTimesheetQuotaRule> Rules)
{
    public List<string> Validate(LegacyTimesheetQuota item)
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

    public void RequireValid(LegacyTimesheetQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyTimesheetQuota> items)
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

/// <summary>Functions of legacy timesheet quota rules.</summary>
public static class LegacyTimesheetQuotaRules
{
    public static LegacyTimesheetQuotaValidator DefaultLegacyTimesheetQuotaValidator()
    {
        return new LegacyTimesheetQuotaValidator(
            new List<ILegacyTimesheetQuotaRule>
            {
                new LegacyTimesheetQuotaCostRangeRule(4.4, 3946.7),
                new LegacyTimesheetQuotaAttemptsRangeRule(1000, 86400),
                new LegacyTimesheetQuotaRequiredTitleRule("legacy timesheet quota needs this field"),
                new LegacyTimesheetQuotaCodePrefixRule("X"),
                new LegacyTimesheetQuotaCodeLengthRule(64),
            });
    }
}
