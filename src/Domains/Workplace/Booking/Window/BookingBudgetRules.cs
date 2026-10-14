namespace Acme.Workforce.Workplace.Booking.Window;

/// <summary>A rule a booking budget must pass. Check returns its problems, or none.</summary>
public interface IBookingBudgetRule
{
    List<string> Check(BookingBudget item);
}

public sealed record BookingBudgetCodeLengthRule(int MaxLength) : IBookingBudgetRule
{
    public List<string> Check(BookingBudget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingBudgetActiveGuardRule(int Limit) : IBookingBudgetRule
{
    public List<string> Check(BookingBudget item)
    {
        if (item.Active && item.BatchSize > Limit)
        {
            return new List<string> { $"batch size {item.BatchSize} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record BookingBudgetBatchSizeRangeRule(int Min, int Max) : IBookingBudgetRule
{
    public List<string> Check(BookingBudget item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking budget.</summary>
public sealed record BookingBudgetValidator(List<IBookingBudgetRule> Rules)
{
    public List<string> Validate(BookingBudget item)
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

    public void RequireValid(BookingBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingBudget> items)
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

/// <summary>Functions of booking budget rules.</summary>
public static class BookingBudgetRules
{
    public static BookingBudgetValidator DefaultBookingBudgetValidator()
    {
        return new BookingBudgetValidator(
            new List<IBookingBudgetRule>
            {
                new BookingBudgetCodeLengthRule(80),
                new BookingBudgetActiveGuardRule(16),
                new BookingBudgetBatchSizeRangeRule(255, 3600),
            });
    }
}
