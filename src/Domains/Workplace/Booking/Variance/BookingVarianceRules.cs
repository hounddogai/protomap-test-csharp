namespace Acme.Workforce.Workplace.Booking.Variance;

/// <summary>A rule a booking variance must pass. Check returns its problems, or none.</summary>
public interface IBookingVarianceRule
{
    List<string> Check(BookingVariance item);
}

public sealed record BookingVarianceGradeLengthRule(int MaxLength) : IBookingVarianceRule
{
    public List<string> Check(BookingVariance item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingVarianceTenureMonthsRangeRule(int Min, int Max) : IBookingVarianceRule
{
    public List<string> Check(BookingVariance item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BookingVarianceRevisionRangeRule(int Min, int Max) : IBookingVarianceRule
{
    public List<string> Check(BookingVariance item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BookingVarianceCostRangeRule(double Min, double Max) : IBookingVarianceRule
{
    public List<string> Check(BookingVariance item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking variance.</summary>
public sealed record BookingVarianceValidator(List<IBookingVarianceRule> Rules)
{
    public List<string> Validate(BookingVariance item)
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

    public void RequireValid(BookingVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingVariance> items)
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

/// <summary>Functions of booking variance rules.</summary>
public static class BookingVarianceRules
{
    public static BookingVarianceValidator DefaultBookingVarianceValidator()
    {
        return new BookingVarianceValidator(
            new List<IBookingVarianceRule>
            {
                new BookingVarianceGradeLengthRule(120),
                new BookingVarianceTenureMonthsRangeRule(32, 80),
                new BookingVarianceRevisionRangeRule(16, 3600),
                new BookingVarianceCostRangeRule(9.4, 263.7),
            });
    }
}
