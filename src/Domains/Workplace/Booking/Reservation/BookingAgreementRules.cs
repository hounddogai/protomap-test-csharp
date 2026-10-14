namespace Acme.Workforce.Workplace.Booking.Reservation;

/// <summary>A rule a booking agreement must pass. Check returns its problems, or none.</summary>
public interface IBookingAgreementRule
{
    List<string> Check(BookingAgreement item);
}

public sealed record BookingAgreementCodeLengthRule(int MaxLength) : IBookingAgreementRule
{
    public List<string> Check(BookingAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingAgreementNameLengthRule(int MaxLength) : IBookingAgreementRule
{
    public List<string> Check(BookingAgreement item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingAgreementCountryLengthRule(int MaxLength) : IBookingAgreementRule
{
    public List<string> Check(BookingAgreement item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingAgreementRequiredCategoryRule(string Message) : IBookingAgreementRule
{
    public List<string> Check(BookingAgreement item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingAgreementCategoryLengthRule(int MaxLength) : IBookingAgreementRule
{
    public List<string> Check(BookingAgreement item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingAgreementRequiredNameRule(string Message) : IBookingAgreementRule
{
    public List<string> Check(BookingAgreement item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking agreement.</summary>
public sealed record BookingAgreementValidator(List<IBookingAgreementRule> Rules)
{
    public List<string> Validate(BookingAgreement item)
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

    public void RequireValid(BookingAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingAgreement> items)
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

/// <summary>Functions of booking agreement rules.</summary>
public static class BookingAgreementRules
{
    public static BookingAgreementValidator DefaultBookingAgreementValidator()
    {
        return new BookingAgreementValidator(
            new List<IBookingAgreementRule>
            {
                new BookingAgreementCodeLengthRule(32),
                new BookingAgreementNameLengthRule(80),
                new BookingAgreementCountryLengthRule(255),
                new BookingAgreementRequiredCategoryRule("booking agreement needs this field"),
                new BookingAgreementCategoryLengthRule(255),
                new BookingAgreementRequiredNameRule("booking agreement needs an entry"),
            });
    }
}
