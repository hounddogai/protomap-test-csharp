namespace Acme.Workforce.Workplace.Booking.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a booking entry must pass. Check returns its problems, or none.</summary>
public interface IBookingEntryRule
{
    List<string> Check(BookingEntry item);
}

public sealed record BookingEntryAllowedCodeRule(List<string> Allowed) : IBookingEntryRule
{
    public List<string> Check(BookingEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record BookingEntryTitleLengthRule(int MaxLength) : IBookingEntryRule
{
    public List<string> Check(BookingEntry item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingEntryRequiredSummaryRule(string Message) : IBookingEntryRule
{
    public List<string> Check(BookingEntry item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingEntryDurationMinutesRangeRule(int Min, int Max) : IBookingEntryRule
{
    public List<string> Check(BookingEntry item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BookingEntrySummaryLengthRule(int MaxLength) : IBookingEntryRule
{
    public List<string> Check(BookingEntry item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking entry.</summary>
public sealed record BookingEntryValidator(List<IBookingEntryRule> Rules)
{
    public List<string> Validate(BookingEntry item)
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

    public void RequireValid(BookingEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingEntry> items)
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

/// <summary>Functions of booking entry rules.</summary>
public static class BookingEntryRules
{
    public static BookingEntryValidator DefaultBookingEntryValidator()
    {
        return new BookingEntryValidator(
            new List<IBookingEntryRule>
            {
                new BookingEntryAllowedCodeRule(new List<string> { "legacy", "default" }),
                new BookingEntryTitleLengthRule(1000),
                new BookingEntryRequiredSummaryRule("booking entry needs an entry"),
                new BookingEntryDurationMinutesRangeRule(64, 1000),
                new BookingEntrySummaryLengthRule(255),
            });
    }
}
