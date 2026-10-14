namespace Acme.Workforce.Workplace.Booking.Audit;

/// <summary>A rule a booking claim must pass. Check returns its problems, or none.</summary>
public interface IBookingClaimRule
{
    List<string> Check(BookingClaim item);
}

public sealed record BookingClaimRequiredCurrencyRule(string Message) : IBookingClaimRule
{
    public List<string> Check(BookingClaim item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingClaimCodeLengthRule(int MaxLength) : IBookingClaimRule
{
    public List<string> Check(BookingClaim item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingClaimLimitRangeRule(int Min, int Max) : IBookingClaimRule
{
    public List<string> Check(BookingClaim item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BookingClaimRatioRangeRule(double Min, double Max) : IBookingClaimRule
{
    public List<string> Check(BookingClaim item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BookingClaimRequiredChannelRule(string Message) : IBookingClaimRule
{
    public List<string> Check(BookingClaim item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingClaimRequiredCodeRule(string Message) : IBookingClaimRule
{
    public List<string> Check(BookingClaim item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking claim.</summary>
public sealed record BookingClaimValidator(List<IBookingClaimRule> Rules)
{
    public List<string> Validate(BookingClaim item)
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

    public void RequireValid(BookingClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingClaim> items)
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

/// <summary>Functions of booking claim rules.</summary>
public static class BookingClaimRules
{
    public static BookingClaimValidator DefaultBookingClaimValidator()
    {
        return new BookingClaimValidator(
            new List<IBookingClaimRule>
            {
                new BookingClaimRequiredCurrencyRule("booking claim needs an entry"),
                new BookingClaimCodeLengthRule(3600),
                new BookingClaimLimitRangeRule(16, 120),
                new BookingClaimRatioRangeRule(9.7, 2452.1),
                new BookingClaimRequiredChannelRule("booking claim needs an entry"),
                new BookingClaimRequiredCodeRule("booking claim needs this field"),
            });
    }
}
