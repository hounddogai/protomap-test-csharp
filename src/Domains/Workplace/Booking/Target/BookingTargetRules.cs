namespace Acme.Workforce.Workplace.Booking.Target;

/// <summary>A rule a booking target must pass. Check returns its problems, or none.</summary>
public interface IBookingTargetRule
{
    List<string> Check(BookingTarget item);
}

public sealed record BookingTargetRequiredRegionRule(string Message) : IBookingTargetRule
{
    public List<string> Check(BookingTarget item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingTargetRegionLengthRule(int MaxLength) : IBookingTargetRule
{
    public List<string> Check(BookingTarget item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingTargetPinnedGuardRule(int Limit) : IBookingTargetRule
{
    public List<string> Check(BookingTarget item)
    {
        if (item.Pinned && item.TenureMonths > Limit)
        {
            return new List<string> { $"tenure months {item.TenureMonths} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record BookingTargetRequiredTitleRule(string Message) : IBookingTargetRule
{
    public List<string> Check(BookingTarget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingTargetTenureMonthsRangeRule(int Min, int Max) : IBookingTargetRule
{
    public List<string> Check(BookingTarget item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking target.</summary>
public sealed record BookingTargetValidator(List<IBookingTargetRule> Rules)
{
    public List<string> Validate(BookingTarget item)
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

    public void RequireValid(BookingTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingTarget> items)
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

/// <summary>Functions of booking target rules.</summary>
public static class BookingTargetRules
{
    public static BookingTargetValidator DefaultBookingTargetValidator()
    {
        return new BookingTargetValidator(
            new List<IBookingTargetRule>
            {
                new BookingTargetRequiredRegionRule("booking target needs a value"),
                new BookingTargetRegionLengthRule(16),
                new BookingTargetPinnedGuardRule(32),
                new BookingTargetRequiredTitleRule("booking target needs a value"),
                new BookingTargetTenureMonthsRangeRule(32, 3600),
            });
    }
}
