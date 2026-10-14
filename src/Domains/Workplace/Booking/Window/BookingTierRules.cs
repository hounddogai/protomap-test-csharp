namespace Acme.Workforce.Workplace.Booking.Window;

/// <summary>A rule a booking tier must pass. Check returns its problems, or none.</summary>
public interface IBookingTierRule
{
    List<string> Check(BookingTier item);
}

public sealed record BookingTierRequiredCodeRule(string Message) : IBookingTierRule
{
    public List<string> Check(BookingTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingTierCodeLengthRule(int MaxLength) : IBookingTierRule
{
    public List<string> Check(BookingTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingTierMarginRangeRule(double Min, double Max) : IBookingTierRule
{
    public List<string> Check(BookingTier item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BookingTierVersionRangeRule(int Min, int Max) : IBookingTierRule
{
    public List<string> Check(BookingTier item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BookingTierRequiredTimezoneRule(string Message) : IBookingTierRule
{
    public List<string> Check(BookingTier item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking tier.</summary>
public sealed record BookingTierValidator(List<IBookingTierRule> Rules)
{
    public List<string> Validate(BookingTier item)
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

    public void RequireValid(BookingTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingTier> items)
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

/// <summary>Functions of booking tier rules.</summary>
public static class BookingTierRules
{
    public static BookingTierValidator DefaultBookingTierValidator()
    {
        return new BookingTierValidator(
            new List<IBookingTierRule>
            {
                new BookingTierRequiredCodeRule("booking tier needs an entry"),
                new BookingTierCodeLengthRule(86400),
                new BookingTierMarginRangeRule(0.2, 4935.2),
                new BookingTierVersionRangeRule(32, 3600),
                new BookingTierRequiredTimezoneRule("booking tier needs a value"),
            });
    }
}
