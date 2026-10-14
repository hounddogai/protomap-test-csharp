namespace Acme.Workforce.Workplace.Booking.Assignment;

/// <summary>A rule a booking assignment must pass. Check returns its problems, or none.</summary>
public interface IBookingAssignmentRule
{
    List<string> Check(BookingAssignment item);
}

public sealed record BookingAssignmentNotesLengthRule(int MaxLength) : IBookingAssignmentRule
{
    public List<string> Check(BookingAssignment item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingAssignmentRequiredCurrencyRule(string Message) : IBookingAssignmentRule
{
    public List<string> Check(BookingAssignment item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingAssignmentCostRangeRule(double Min, double Max) : IBookingAssignmentRule
{
    public List<string> Check(BookingAssignment item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BookingAssignmentEndsAtRangeRule(int Min, int Max) : IBookingAssignmentRule
{
    public List<string> Check(BookingAssignment item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a booking assignment.</summary>
public sealed record BookingAssignmentValidator(List<IBookingAssignmentRule> Rules)
{
    public List<string> Validate(BookingAssignment item)
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

    public void RequireValid(BookingAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingAssignment> items)
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

/// <summary>Functions of booking assignment rules.</summary>
public static class BookingAssignmentRules
{
    public static BookingAssignmentValidator DefaultBookingAssignmentValidator()
    {
        return new BookingAssignmentValidator(
            new List<IBookingAssignmentRule>
            {
                new BookingAssignmentNotesLengthRule(120),
                new BookingAssignmentRequiredCurrencyRule("booking assignment needs this field"),
                new BookingAssignmentCostRangeRule(2.3, 2883.3),
                new BookingAssignmentEndsAtRangeRule(32, 3600),
            });
    }
}
