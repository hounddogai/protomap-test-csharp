namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

/// <summary>A rule a pending timesheet policy must pass. Check returns its problems, or none.</summary>
public interface IPendingTimesheetPolicyRule
{
    List<string> Check(PendingTimesheetPolicy item);
}

public sealed record PendingTimesheetPolicyCategoryLengthRule(int MaxLength) : IPendingTimesheetPolicyRule
{
    public List<string> Check(PendingTimesheetPolicy item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingTimesheetPolicyTitleLengthRule(int MaxLength) : IPendingTimesheetPolicyRule
{
    public List<string> Check(PendingTimesheetPolicy item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingTimesheetPolicyLeaveDaysRangeRule(int Min, int Max) : IPendingTimesheetPolicyRule
{
    public List<string> Check(PendingTimesheetPolicy item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingTimesheetPolicyRequiredChecksumRule(string Message) : IPendingTimesheetPolicyRule
{
    public List<string> Check(PendingTimesheetPolicy item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingTimesheetPolicyCodePrefixRule(string Prefix) : IPendingTimesheetPolicyRule
{
    public List<string> Check(PendingTimesheetPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending timesheet policy.</summary>
public sealed record PendingTimesheetPolicyValidator(List<IPendingTimesheetPolicyRule> Rules)
{
    public List<string> Validate(PendingTimesheetPolicy item)
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

    public void RequireValid(PendingTimesheetPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingTimesheetPolicy> items)
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

/// <summary>Functions of pending timesheet policy rules.</summary>
public static class PendingTimesheetPolicyRules
{
    public static PendingTimesheetPolicyValidator DefaultPendingTimesheetPolicyValidator()
    {
        return new PendingTimesheetPolicyValidator(
            new List<IPendingTimesheetPolicyRule>
            {
                new PendingTimesheetPolicyCategoryLengthRule(80),
                new PendingTimesheetPolicyTitleLengthRule(3600),
                new PendingTimesheetPolicyLeaveDaysRangeRule(32, 255),
                new PendingTimesheetPolicyRequiredChecksumRule("pending timesheet policy needs this field"),
                new PendingTimesheetPolicyCodePrefixRule("REF"),
            });
    }
}
