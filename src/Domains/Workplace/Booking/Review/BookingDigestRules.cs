namespace Acme.Workforce.Workplace.Booking.Review;

/// <summary>A rule a booking digest must pass. Check returns its problems, or none.</summary>
public interface IBookingDigestRule
{
    List<string> Check(BookingDigest item);
}

public sealed record BookingDigestRequiredEmployeeIdRule(string Message) : IBookingDigestRule
{
    public List<string> Check(BookingDigest item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingDigestEmployeeIdLengthRule(int MaxLength) : IBookingDigestRule
{
    public List<string> Check(BookingDigest item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingDigestRequiredCodeRule(string Message) : IBookingDigestRule
{
    public List<string> Check(BookingDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingDigestCodePrefixRule(string Prefix) : IBookingDigestRule
{
    public List<string> Check(BookingDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking digest.</summary>
public sealed record BookingDigestValidator(List<IBookingDigestRule> Rules)
{
    public List<string> Validate(BookingDigest item)
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

    public void RequireValid(BookingDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingDigest> items)
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

/// <summary>Functions of booking digest rules.</summary>
public static class BookingDigestRules
{
    public static BookingDigestValidator DefaultBookingDigestValidator()
    {
        return new BookingDigestValidator(
            new List<IBookingDigestRule>
            {
                new BookingDigestRequiredEmployeeIdRule("booking digest needs this field"),
                new BookingDigestEmployeeIdLengthRule(255),
                new BookingDigestRequiredCodeRule("booking digest needs a value"),
                new BookingDigestCodePrefixRule("OPS"),
            });
    }
}
