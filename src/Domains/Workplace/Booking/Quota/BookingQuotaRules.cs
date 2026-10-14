namespace Acme.Workforce.Workplace.Booking.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a booking quota must pass. Check returns its problems, or none.</summary>
public interface IBookingQuotaRule
{
    List<string> Check(BookingQuota item);
}

public sealed record BookingQuotaExpiresAtRangeRule(int Min, int Max) : IBookingQuotaRule
{
    public List<string> Check(BookingQuota item)
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

public sealed record BookingQuotaRequiredCategoryRule(string Message) : IBookingQuotaRule
{
    public List<string> Check(BookingQuota item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingQuotaCodePrefixRule(string Prefix) : IBookingQuotaRule
{
    public List<string> Check(BookingQuota item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BookingQuotaCapacityRangeRule(int Min, int Max) : IBookingQuotaRule
{
    public List<string> Check(BookingQuota item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BookingQuotaAllowedCodeRule(List<string> Allowed) : IBookingQuotaRule
{
    public List<string> Check(BookingQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record BookingQuotaCategoryLengthRule(int MaxLength) : IBookingQuotaRule
{
    public List<string> Check(BookingQuota item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking quota.</summary>
public sealed record BookingQuotaValidator(List<IBookingQuotaRule> Rules)
{
    public List<string> Validate(BookingQuota item)
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

    public void RequireValid(BookingQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingQuota> items)
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

/// <summary>Functions of booking quota rules.</summary>
public static class BookingQuotaRules
{
    public static BookingQuotaValidator DefaultBookingQuotaValidator()
    {
        return new BookingQuotaValidator(
            new List<IBookingQuotaRule>
            {
                new BookingQuotaExpiresAtRangeRule(32, 64),
                new BookingQuotaRequiredCategoryRule("booking quota needs an entry"),
                new BookingQuotaCodePrefixRule("OPS"),
                new BookingQuotaCapacityRangeRule(1000, 86400),
                new BookingQuotaAllowedCodeRule(new List<string>()),
                new BookingQuotaCategoryLengthRule(86400),
            });
    }
}
