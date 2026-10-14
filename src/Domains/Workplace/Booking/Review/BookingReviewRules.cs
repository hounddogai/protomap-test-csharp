namespace Acme.Workforce.Workplace.Booking.Review;

/// <summary>A rule a booking review must pass. Check returns its problems, or none.</summary>
public interface IBookingReviewRule
{
    List<string> Check(BookingReview item);
}

public sealed record BookingReviewTeamLengthRule(int MaxLength) : IBookingReviewRule
{
    public List<string> Check(BookingReview item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingReviewBudgetRangeRule(double Min, double Max) : IBookingReviewRule
{
    public List<string> Check(BookingReview item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BookingReviewRequiredTitleRule(string Message) : IBookingReviewRule
{
    public List<string> Check(BookingReview item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingReviewCostCenterLengthRule(int MaxLength) : IBookingReviewRule
{
    public List<string> Check(BookingReview item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking review.</summary>
public sealed record BookingReviewValidator(List<IBookingReviewRule> Rules)
{
    public List<string> Validate(BookingReview item)
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

    public void RequireValid(BookingReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingReview> items)
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

/// <summary>Functions of booking review rules.</summary>
public static class BookingReviewRules
{
    public static BookingReviewValidator DefaultBookingReviewValidator()
    {
        return new BookingReviewValidator(
            new List<IBookingReviewRule>
            {
                new BookingReviewTeamLengthRule(255),
                new BookingReviewBudgetRangeRule(2.4, 814.8),
                new BookingReviewRequiredTitleRule("booking review needs this field"),
                new BookingReviewCostCenterLengthRule(1000),
            });
    }
}
