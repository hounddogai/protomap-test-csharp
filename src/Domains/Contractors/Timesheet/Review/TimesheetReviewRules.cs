namespace Acme.Workforce.Contractors.Timesheet.Review;

/// <summary>A rule a timesheet review must pass. Check returns its problems, or none.</summary>
public interface ITimesheetReviewRule
{
    List<string> Check(TimesheetReview item);
}

public sealed record TimesheetReviewRequiredLabelRule(string Message) : ITimesheetReviewRule
{
    public List<string> Check(TimesheetReview item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetReviewApprovedGuardRule(double Limit) : ITimesheetReviewRule
{
    public List<string> Check(TimesheetReview item)
    {
        if (item.Approved && item.Minimum > Limit)
        {
            return new List<string> { $"minimum {item.Minimum} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetReviewRequiredCodeRule(string Message) : ITimesheetReviewRule
{
    public List<string> Check(TimesheetReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetReviewExpiresAtRangeRule(int Min, int Max) : ITimesheetReviewRule
{
    public List<string> Check(TimesheetReview item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetReviewEmployeeIdLengthRule(int MaxLength) : ITimesheetReviewRule
{
    public List<string> Check(TimesheetReview item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetReviewCodePrefixRule(string Prefix) : ITimesheetReviewRule
{
    public List<string> Check(TimesheetReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet review.</summary>
public sealed record TimesheetReviewValidator(List<ITimesheetReviewRule> Rules)
{
    public List<string> Validate(TimesheetReview item)
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

    public void RequireValid(TimesheetReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetReview> items)
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

/// <summary>Functions of timesheet review rules.</summary>
public static class TimesheetReviewRules
{
    public static TimesheetReviewValidator DefaultTimesheetReviewValidator()
    {
        return new TimesheetReviewValidator(
            new List<ITimesheetReviewRule>
            {
                new TimesheetReviewRequiredLabelRule("timesheet review needs an entry"),
                new TimesheetReviewApprovedGuardRule(3.5),
                new TimesheetReviewRequiredCodeRule("timesheet review needs a value"),
                new TimesheetReviewExpiresAtRangeRule(16, 80),
                new TimesheetReviewEmployeeIdLengthRule(32),
                new TimesheetReviewCodePrefixRule("Q"),
            });
    }
}
