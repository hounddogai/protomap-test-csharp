namespace Acme.Workforce.Workplace.Booking.Audit;

/// <summary>A rule a booking profile must pass. Check returns its problems, or none.</summary>
public interface IBookingProfileRule
{
    List<string> Check(BookingProfile item);
}

public sealed record BookingProfileOwnerIdLengthRule(int MaxLength) : IBookingProfileRule
{
    public List<string> Check(BookingProfile item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingProfileVisibleGuardRule(int Limit) : IBookingProfileRule
{
    public List<string> Check(BookingProfile item)
    {
        if (item.Visible && item.Revision > Limit)
        {
            return new List<string> { $"revision {item.Revision} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record BookingProfileUnitLengthRule(int MaxLength) : IBookingProfileRule
{
    public List<string> Check(BookingProfile item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingProfileRequiredLabelRule(string Message) : IBookingProfileRule
{
    public List<string> Check(BookingProfile item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingProfileExpiresAtRangeRule(int Min, int Max) : IBookingProfileRule
{
    public List<string> Check(BookingProfile item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BookingProfileRequiredCodeRule(string Message) : IBookingProfileRule
{
    public List<string> Check(BookingProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking profile.</summary>
public sealed record BookingProfileValidator(List<IBookingProfileRule> Rules)
{
    public List<string> Validate(BookingProfile item)
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

    public void RequireValid(BookingProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingProfile> items)
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

/// <summary>Functions of booking profile rules.</summary>
public static class BookingProfileRules
{
    public static BookingProfileValidator DefaultBookingProfileValidator()
    {
        return new BookingProfileValidator(
            new List<IBookingProfileRule>
            {
                new BookingProfileOwnerIdLengthRule(80),
                new BookingProfileVisibleGuardRule(120),
                new BookingProfileUnitLengthRule(3600),
                new BookingProfileRequiredLabelRule("booking profile needs a value"),
                new BookingProfileExpiresAtRangeRule(64, 86400),
                new BookingProfileRequiredCodeRule("booking profile needs a value"),
            });
    }
}
