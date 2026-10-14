namespace Acme.Workforce.Workplace.Booking.Variance;

/// <summary>A rule a booking checkpoint must pass. Check returns its problems, or none.</summary>
public interface IBookingCheckpointRule
{
    List<string> Check(BookingCheckpoint item);
}

public sealed record BookingCheckpointRequiredCodeRule(string Message) : IBookingCheckpointRule
{
    public List<string> Check(BookingCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingCheckpointRequiredLabelRule(string Message) : IBookingCheckpointRule
{
    public List<string> Check(BookingCheckpoint item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingCheckpointCodePrefixRule(string Prefix) : IBookingCheckpointRule
{
    public List<string> Check(BookingCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BookingCheckpointPinnedGuardRule(double Limit) : IBookingCheckpointRule
{
    public List<string> Check(BookingCheckpoint item)
    {
        if (item.Pinned && item.Hours > Limit)
        {
            return new List<string> { $"hours {item.Hours} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking checkpoint.</summary>
public sealed record BookingCheckpointValidator(List<IBookingCheckpointRule> Rules)
{
    public List<string> Validate(BookingCheckpoint item)
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

    public void RequireValid(BookingCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingCheckpoint> items)
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

/// <summary>Functions of booking checkpoint rules.</summary>
public static class BookingCheckpointRules
{
    public static BookingCheckpointValidator DefaultBookingCheckpointValidator()
    {
        return new BookingCheckpointValidator(
            new List<IBookingCheckpointRule>
            {
                new BookingCheckpointRequiredCodeRule("booking checkpoint needs an entry"),
                new BookingCheckpointRequiredLabelRule("booking checkpoint needs an entry"),
                new BookingCheckpointCodePrefixRule("ACM"),
                new BookingCheckpointPinnedGuardRule(5.1),
            });
    }
}
