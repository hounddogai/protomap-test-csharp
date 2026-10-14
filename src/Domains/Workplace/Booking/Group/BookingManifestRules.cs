namespace Acme.Workforce.Workplace.Booking.Group;

/// <summary>A rule a booking manifest must pass. Check returns its problems, or none.</summary>
public interface IBookingManifestRule
{
    List<string> Check(BookingManifest item);
}

public sealed record BookingManifestDepartmentLengthRule(int MaxLength) : IBookingManifestRule
{
    public List<string> Check(BookingManifest item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingManifestRequiredNameRule(string Message) : IBookingManifestRule
{
    public List<string> Check(BookingManifest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingManifestCodePrefixRule(string Prefix) : IBookingManifestRule
{
    public List<string> Check(BookingManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BookingManifestEndsAtRangeRule(int Min, int Max) : IBookingManifestRule
{
    public List<string> Check(BookingManifest item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BookingManifestJobCodeLengthRule(int MaxLength) : IBookingManifestRule
{
    public List<string> Check(BookingManifest item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingManifestArchivedGuardRule(int Limit) : IBookingManifestRule
{
    public List<string> Check(BookingManifest item)
    {
        if (item.Archived && item.EndsAt > Limit)
        {
            return new List<string> { $"ends at {item.EndsAt} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking manifest.</summary>
public sealed record BookingManifestValidator(List<IBookingManifestRule> Rules)
{
    public List<string> Validate(BookingManifest item)
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

    public void RequireValid(BookingManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingManifest> items)
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

/// <summary>Functions of booking manifest rules.</summary>
public static class BookingManifestRules
{
    public static BookingManifestValidator DefaultBookingManifestValidator()
    {
        return new BookingManifestValidator(
            new List<IBookingManifestRule>
            {
                new BookingManifestDepartmentLengthRule(32),
                new BookingManifestRequiredNameRule("booking manifest needs a value"),
                new BookingManifestCodePrefixRule("OPS"),
                new BookingManifestEndsAtRangeRule(80, 80),
                new BookingManifestJobCodeLengthRule(64),
                new BookingManifestArchivedGuardRule(3600),
            });
    }
}
