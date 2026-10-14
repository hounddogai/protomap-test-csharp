namespace Acme.Workforce.Workplace.Booking.Variance;

/// <summary>A rule a booking allocation must pass. Check returns its problems, or none.</summary>
public interface IBookingAllocationRule
{
    List<string> Check(BookingAllocation item);
}

public sealed record BookingAllocationHoursRangeRule(double Min, double Max) : IBookingAllocationRule
{
    public List<string> Check(BookingAllocation item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BookingAllocationLabelLengthRule(int MaxLength) : IBookingAllocationRule
{
    public List<string> Check(BookingAllocation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingAllocationCodePrefixRule(string Prefix) : IBookingAllocationRule
{
    public List<string> Check(BookingAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking allocation.</summary>
public sealed record BookingAllocationValidator(List<IBookingAllocationRule> Rules)
{
    public List<string> Validate(BookingAllocation item)
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

    public void RequireValid(BookingAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingAllocation> items)
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

/// <summary>Functions of booking allocation rules.</summary>
public static class BookingAllocationRules
{
    public static BookingAllocationValidator DefaultBookingAllocationValidator()
    {
        return new BookingAllocationValidator(
            new List<IBookingAllocationRule>
            {
                new BookingAllocationHoursRangeRule(7.7, 2541.0),
                new BookingAllocationLabelLengthRule(255),
                new BookingAllocationCodePrefixRule("ACM"),
            });
    }
}
