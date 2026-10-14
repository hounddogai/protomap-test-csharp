namespace Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

/// <summary>A rule a timesheet digest must pass. Check returns its problems, or none.</summary>
public interface ITimesheetDigestRule
{
    List<string> Check(TimesheetDigest item);
}

public sealed record TimesheetDigestTenureMonthsRangeRule(int Min, int Max) : ITimesheetDigestRule
{
    public List<string> Check(TimesheetDigest item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetDigestCodeLengthRule(int MaxLength) : ITimesheetDigestRule
{
    public List<string> Check(TimesheetDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetDigestCodePrefixRule(string Prefix) : ITimesheetDigestRule
{
    public List<string> Check(TimesheetDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet digest.</summary>
public sealed record TimesheetDigestValidator(List<ITimesheetDigestRule> Rules)
{
    public List<string> Validate(TimesheetDigest item)
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

    public void RequireValid(TimesheetDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetDigest> items)
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

/// <summary>Functions of timesheet digest rules.</summary>
public static class TimesheetDigestRules
{
    public static TimesheetDigestValidator DefaultTimesheetDigestValidator()
    {
        return new TimesheetDigestValidator(
            new List<ITimesheetDigestRule>
            {
                new TimesheetDigestTenureMonthsRangeRule(32, 64),
                new TimesheetDigestCodeLengthRule(120),
                new TimesheetDigestCodePrefixRule("REF"),
            });
    }
}
