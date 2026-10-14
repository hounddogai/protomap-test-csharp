namespace Acme.Workforce.Workplace.Booking.BookingAudit;

/// <summary>A rule a booking snapshot must pass. Check returns its problems, or none.</summary>
public interface IBookingSnapshotRule
{
    List<string> Check(BookingSnapshot item);
}

public sealed record BookingSnapshotRequiredNotesRule(string Message) : IBookingSnapshotRule
{
    public List<string> Check(BookingSnapshot item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingSnapshotApprovedGuardRule(int Limit) : IBookingSnapshotRule
{
    public List<string> Check(BookingSnapshot item)
    {
        if (item.Approved && item.DurationMinutes > Limit)
        {
            return new List<string> { $"duration minutes {item.DurationMinutes} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record BookingSnapshotCodePrefixRule(string Prefix) : IBookingSnapshotRule
{
    public List<string> Check(BookingSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BookingSnapshotNotesLengthRule(int MaxLength) : IBookingSnapshotRule
{
    public List<string> Check(BookingSnapshot item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingSnapshotDescriptionLengthRule(int MaxLength) : IBookingSnapshotRule
{
    public List<string> Check(BookingSnapshot item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking snapshot.</summary>
public sealed record BookingSnapshotValidator(List<IBookingSnapshotRule> Rules)
{
    public List<string> Validate(BookingSnapshot item)
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

    public void RequireValid(BookingSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingSnapshot> items)
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

/// <summary>Functions of booking snapshot rules.</summary>
public static class BookingSnapshotRules
{
    public static BookingSnapshotValidator DefaultBookingSnapshotValidator()
    {
        return new BookingSnapshotValidator(
            new List<IBookingSnapshotRule>
            {
                new BookingSnapshotRequiredNotesRule("booking snapshot needs a value"),
                new BookingSnapshotApprovedGuardRule(3600),
                new BookingSnapshotCodePrefixRule("ACM"),
                new BookingSnapshotNotesLengthRule(120),
                new BookingSnapshotDescriptionLengthRule(255),
            });
    }
}
