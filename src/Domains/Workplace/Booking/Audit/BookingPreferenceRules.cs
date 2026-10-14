namespace Acme.Workforce.Workplace.Booking.Audit;

/// <summary>A rule a booking preference must pass. Check returns its problems, or none.</summary>
public interface IBookingPreferenceRule
{
    List<string> Check(BookingPreference item);
}

public sealed record BookingPreferenceRequiredGradeRule(string Message) : IBookingPreferenceRule
{
    public List<string> Check(BookingPreference item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingPreferenceScoreRangeRule(double Min, double Max) : IBookingPreferenceRule
{
    public List<string> Check(BookingPreference item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BookingPreferenceCodePrefixRule(string Prefix) : IBookingPreferenceRule
{
    public List<string> Check(BookingPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking preference.</summary>
public sealed record BookingPreferenceValidator(List<IBookingPreferenceRule> Rules)
{
    public List<string> Validate(BookingPreference item)
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

    public void RequireValid(BookingPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingPreference> items)
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

/// <summary>Functions of booking preference rules.</summary>
public static class BookingPreferenceRules
{
    public static BookingPreferenceValidator DefaultBookingPreferenceValidator()
    {
        return new BookingPreferenceValidator(
            new List<IBookingPreferenceRule>
            {
                new BookingPreferenceRequiredGradeRule("booking preference needs an entry"),
                new BookingPreferenceScoreRangeRule(6.1, 2132.1),
                new BookingPreferenceCodePrefixRule("X"),
            });
    }
}
