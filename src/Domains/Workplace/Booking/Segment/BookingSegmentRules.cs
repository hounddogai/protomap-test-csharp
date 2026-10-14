namespace Acme.Workforce.Workplace.Booking.Segment;

/// <summary>A rule a booking segment must pass. Check returns its problems, or none.</summary>
public interface IBookingSegmentRule
{
    List<string> Check(BookingSegment item);
}

public sealed record BookingSegmentCodeLengthRule(int MaxLength) : IBookingSegmentRule
{
    public List<string> Check(BookingSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingSegmentLeaveDaysRangeRule(int Min, int Max) : IBookingSegmentRule
{
    public List<string> Check(BookingSegment item)
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

public sealed record BookingSegmentCodePrefixRule(string Prefix) : IBookingSegmentRule
{
    public List<string> Check(BookingSegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BookingSegmentCreatedAtRangeRule(int Min, int Max) : IBookingSegmentRule
{
    public List<string> Check(BookingSegment item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BookingSegmentRequiredNameRule(string Message) : IBookingSegmentRule
{
    public List<string> Check(BookingSegment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingSegmentRequiredCodeRule(string Message) : IBookingSegmentRule
{
    public List<string> Check(BookingSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking segment.</summary>
public sealed record BookingSegmentValidator(List<IBookingSegmentRule> Rules)
{
    public List<string> Validate(BookingSegment item)
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

    public void RequireValid(BookingSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingSegment> items)
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

/// <summary>Functions of booking segment rules.</summary>
public static class BookingSegmentRules
{
    public static BookingSegmentValidator DefaultBookingSegmentValidator()
    {
        return new BookingSegmentValidator(
            new List<IBookingSegmentRule>
            {
                new BookingSegmentCodeLengthRule(1000),
                new BookingSegmentLeaveDaysRangeRule(1000, 1000),
                new BookingSegmentCodePrefixRule("REF"),
                new BookingSegmentCreatedAtRangeRule(120, 1000),
                new BookingSegmentRequiredNameRule("booking segment needs this field"),
                new BookingSegmentRequiredCodeRule("booking segment needs an entry"),
            });
    }
}
