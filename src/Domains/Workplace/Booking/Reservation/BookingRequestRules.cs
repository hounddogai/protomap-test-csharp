namespace Acme.Workforce.Workplace.Booking.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a booking request must pass. Check returns its problems, or none.</summary>
public interface IBookingRequestRule
{
    List<string> Check(BookingRequest item);
}

public sealed record BookingRequestRequiredTitleRule(string Message) : IBookingRequestRule
{
    public List<string> Check(BookingRequest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingRequestCodeLengthRule(int MaxLength) : IBookingRequestRule
{
    public List<string> Check(BookingRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingRequestTitleLengthRule(int MaxLength) : IBookingRequestRule
{
    public List<string> Check(BookingRequest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingRequestAllowedTitleRule(List<string> Allowed) : IBookingRequestRule
{
    public List<string> Check(BookingRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is expired" };
        }
        return new List<string>();
    }
}

public sealed record BookingRequestRequiredNameRule(string Message) : IBookingRequestRule
{
    public List<string> Check(BookingRequest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingRequestRevisionRangeRule(int Min, int Max) : IBookingRequestRule
{
    public List<string> Check(BookingRequest item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking request.</summary>
public sealed record BookingRequestValidator(List<IBookingRequestRule> Rules)
{
    public List<string> Validate(BookingRequest item)
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

    public void RequireValid(BookingRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingRequest> items)
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

/// <summary>Functions of booking request rules.</summary>
public static class BookingRequestRules
{
    public static BookingRequestValidator DefaultBookingRequestValidator()
    {
        return new BookingRequestValidator(
            new List<IBookingRequestRule>
            {
                new BookingRequestRequiredTitleRule("booking request needs this field"),
                new BookingRequestCodeLengthRule(16),
                new BookingRequestTitleLengthRule(32),
                new BookingRequestAllowedTitleRule(new List<string> { "seasonal", "scheduled" }),
                new BookingRequestRequiredNameRule("booking request needs this field"),
                new BookingRequestRevisionRangeRule(16, 120),
            });
    }
}
