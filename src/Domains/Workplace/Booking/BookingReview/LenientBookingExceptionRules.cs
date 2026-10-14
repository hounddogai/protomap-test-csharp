namespace Acme.Workforce.Workplace.Booking.BookingReview;

/// <summary>A rule a lenient booking exception must pass. Check returns its problems, or none.</summary>
public interface ILenientBookingExceptionRule
{
    List<string> Check(LenientBookingException item);
}

public sealed record LenientBookingExceptionRequiredReviewerRule(string Message) : ILenientBookingExceptionRule
{
    public List<string> Check(LenientBookingException item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientBookingExceptionRequiredEmployeeIdRule(string Message) : ILenientBookingExceptionRule
{
    public List<string> Check(LenientBookingException item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientBookingExceptionNameLengthRule(int MaxLength) : ILenientBookingExceptionRule
{
    public List<string> Check(LenientBookingException item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient booking exception.</summary>
public sealed record LenientBookingExceptionValidator(List<ILenientBookingExceptionRule> Rules)
{
    public List<string> Validate(LenientBookingException item)
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

    public void RequireValid(LenientBookingException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientBookingException> items)
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

/// <summary>Functions of lenient booking exception rules.</summary>
public static class LenientBookingExceptionRules
{
    public static LenientBookingExceptionValidator DefaultLenientBookingExceptionValidator()
    {
        return new LenientBookingExceptionValidator(
            new List<ILenientBookingExceptionRule>
            {
                new LenientBookingExceptionRequiredReviewerRule("lenient booking exception needs an entry"),
                new LenientBookingExceptionRequiredEmployeeIdRule("lenient booking exception needs this field"),
                new LenientBookingExceptionNameLengthRule(3600),
            });
    }
}
