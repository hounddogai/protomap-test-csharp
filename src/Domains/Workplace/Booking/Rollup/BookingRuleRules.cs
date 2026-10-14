namespace Acme.Workforce.Workplace.Booking.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a booking rule must pass. Check returns its problems, or none.</summary>
public interface IBookingRuleRule
{
    List<string> Check(BookingRule item);
}

public sealed record BookingRuleRequiredLocaleRule(string Message) : IBookingRuleRule
{
    public List<string> Check(BookingRule item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingRuleRequiredCodeRule(string Message) : IBookingRuleRule
{
    public List<string> Check(BookingRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingRuleAllowedRegionRule(List<string> Allowed) : IBookingRuleRule
{
    public List<string> Check(BookingRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record BookingRuleCodeLengthRule(int MaxLength) : IBookingRuleRule
{
    public List<string> Check(BookingRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingRuleLocaleLengthRule(int MaxLength) : IBookingRuleRule
{
    public List<string> Check(BookingRule item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingRuleRateRangeRule(double Min, double Max) : IBookingRuleRule
{
    public List<string> Check(BookingRule item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a booking rule.</summary>
public sealed record BookingRuleValidator(List<IBookingRuleRule> Rules)
{
    public List<string> Validate(BookingRule item)
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

    public void RequireValid(BookingRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingRule> items)
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

/// <summary>Functions of booking rule rules.</summary>
public static class BookingRuleRules
{
    public static BookingRuleValidator DefaultBookingRuleValidator()
    {
        return new BookingRuleValidator(
            new List<IBookingRuleRule>
            {
                new BookingRuleRequiredLocaleRule("booking rule needs a value"),
                new BookingRuleRequiredCodeRule("booking rule needs this field"),
                new BookingRuleAllowedRegionRule(new List<string> { "lenient", "regional" }),
                new BookingRuleCodeLengthRule(1000),
                new BookingRuleLocaleLengthRule(86400),
                new BookingRuleRateRangeRule(7.0, 4567.3),
            });
    }
}
