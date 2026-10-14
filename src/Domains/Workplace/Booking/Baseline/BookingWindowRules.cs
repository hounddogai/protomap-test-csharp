namespace Acme.Workforce.Workplace.Booking.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a booking window must pass. Check returns its problems, or none.</summary>
public interface IBookingWindowRule
{
    List<string> Check(BookingWindow item);
}

public sealed record BookingWindowTitleLengthRule(int MaxLength) : IBookingWindowRule
{
    public List<string> Check(BookingWindow item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingWindowRequiredNameRule(string Message) : IBookingWindowRule
{
    public List<string> Check(BookingWindow item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingWindowCodeLengthRule(int MaxLength) : IBookingWindowRule
{
    public List<string> Check(BookingWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingWindowRateRangeRule(double Min, double Max) : IBookingWindowRule
{
    public List<string> Check(BookingWindow item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BookingWindowPhoneLengthRule(int MaxLength) : IBookingWindowRule
{
    public List<string> Check(BookingWindow item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingWindowAllowedNameRule(List<string> Allowed) : IBookingWindowRule
{
    public List<string> Check(BookingWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is above maximum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking window.</summary>
public sealed record BookingWindowValidator(List<IBookingWindowRule> Rules)
{
    public List<string> Validate(BookingWindow item)
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

    public void RequireValid(BookingWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingWindow> items)
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

/// <summary>Functions of booking window rules.</summary>
public static class BookingWindowRules
{
    public static BookingWindowValidator DefaultBookingWindowValidator()
    {
        return new BookingWindowValidator(
            new List<IBookingWindowRule>
            {
                new BookingWindowTitleLengthRule(64),
                new BookingWindowRequiredNameRule("booking window needs a value"),
                new BookingWindowCodeLengthRule(64),
                new BookingWindowRateRangeRule(3.1, 1645.6),
                new BookingWindowPhoneLengthRule(86400),
                new BookingWindowAllowedNameRule(new List<string> { "global" }),
            });
    }
}
