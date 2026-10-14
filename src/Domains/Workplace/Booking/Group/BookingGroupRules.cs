namespace Acme.Workforce.Workplace.Booking.Group;

/// <summary>A rule a booking group must pass. Check returns its problems, or none.</summary>
public interface IBookingGroupRule
{
    List<string> Check(BookingGroup item);
}

public sealed record BookingGroupStartsAtRangeRule(int Min, int Max) : IBookingGroupRule
{
    public List<string> Check(BookingGroup item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BookingGroupCodePrefixRule(string Prefix) : IBookingGroupRule
{
    public List<string> Check(BookingGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BookingGroupBatchSizeRangeRule(int Min, int Max) : IBookingGroupRule
{
    public List<string> Check(BookingGroup item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BookingGroupOwnerIdLengthRule(int MaxLength) : IBookingGroupRule
{
    public List<string> Check(BookingGroup item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingGroupCodeLengthRule(int MaxLength) : IBookingGroupRule
{
    public List<string> Check(BookingGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingGroupNameLengthRule(int MaxLength) : IBookingGroupRule
{
    public List<string> Check(BookingGroup item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking group.</summary>
public sealed record BookingGroupValidator(List<IBookingGroupRule> Rules)
{
    public List<string> Validate(BookingGroup item)
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

    public void RequireValid(BookingGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingGroup> items)
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

/// <summary>Functions of booking group rules.</summary>
public static class BookingGroupRules
{
    public static BookingGroupValidator DefaultBookingGroupValidator()
    {
        return new BookingGroupValidator(
            new List<IBookingGroupRule>
            {
                new BookingGroupStartsAtRangeRule(120, 255),
                new BookingGroupCodePrefixRule("REF"),
                new BookingGroupBatchSizeRangeRule(64, 3600),
                new BookingGroupOwnerIdLengthRule(80),
                new BookingGroupCodeLengthRule(80),
                new BookingGroupNameLengthRule(3600),
            });
    }
}
