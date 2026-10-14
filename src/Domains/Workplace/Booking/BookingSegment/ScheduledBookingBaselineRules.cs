namespace Acme.Workforce.Workplace.Booking.BookingSegment;

/// <summary>A rule a scheduled booking baseline must pass. Check returns its problems, or none.</summary>
public interface IScheduledBookingBaselineRule
{
    List<string> Check(ScheduledBookingBaseline item);
}

public sealed record ScheduledBookingBaselineArchivedGuardRule(int Limit) : IScheduledBookingBaselineRule
{
    public List<string> Check(ScheduledBookingBaseline item)
    {
        if (item.Archived && item.Limit > Limit)
        {
            return new List<string> { $"limit {item.Limit} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBookingBaselineTitleLengthRule(int MaxLength) : IScheduledBookingBaselineRule
{
    public List<string> Check(ScheduledBookingBaseline item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBookingBaselineRequiredTitleRule(string Message) : IScheduledBookingBaselineRule
{
    public List<string> Check(ScheduledBookingBaseline item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled booking baseline.</summary>
public sealed record ScheduledBookingBaselineValidator(List<IScheduledBookingBaselineRule> Rules)
{
    public List<string> Validate(ScheduledBookingBaseline item)
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

    public void RequireValid(ScheduledBookingBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledBookingBaseline> items)
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

/// <summary>Functions of scheduled booking baseline rules.</summary>
public static class ScheduledBookingBaselineRules
{
    public static ScheduledBookingBaselineValidator DefaultScheduledBookingBaselineValidator()
    {
        return new ScheduledBookingBaselineValidator(
            new List<IScheduledBookingBaselineRule>
            {
                new ScheduledBookingBaselineArchivedGuardRule(1000),
                new ScheduledBookingBaselineTitleLengthRule(255),
                new ScheduledBookingBaselineRequiredTitleRule("scheduled booking baseline needs this field"),
            });
    }
}
