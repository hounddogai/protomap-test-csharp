namespace Acme.Workforce.Workplace.Booking.BookingPlan;

/// <summary>A rule a lenient booking plan must pass. Check returns its problems, or none.</summary>
public interface ILenientBookingPlanRule
{
    List<string> Check(LenientBookingPlan item);
}

public sealed record LenientBookingPlanSegmentLengthRule(int MaxLength) : ILenientBookingPlanRule
{
    public List<string> Check(LenientBookingPlan item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientBookingPlanVisibleGuardRule(int Limit) : ILenientBookingPlanRule
{
    public List<string> Check(LenientBookingPlan item)
    {
        if (item.Visible && item.Attempts > Limit)
        {
            return new List<string> { $"attempts {item.Attempts} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record LenientBookingPlanRequiredSegmentRule(string Message) : ILenientBookingPlanRule
{
    public List<string> Check(LenientBookingPlan item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientBookingPlanCodeLengthRule(int MaxLength) : ILenientBookingPlanRule
{
    public List<string> Check(LenientBookingPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientBookingPlanRequiredCodeRule(string Message) : ILenientBookingPlanRule
{
    public List<string> Check(LenientBookingPlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient booking plan.</summary>
public sealed record LenientBookingPlanValidator(List<ILenientBookingPlanRule> Rules)
{
    public List<string> Validate(LenientBookingPlan item)
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

    public void RequireValid(LenientBookingPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientBookingPlan> items)
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

/// <summary>Functions of lenient booking plan rules.</summary>
public static class LenientBookingPlanRules
{
    public static LenientBookingPlanValidator DefaultLenientBookingPlanValidator()
    {
        return new LenientBookingPlanValidator(
            new List<ILenientBookingPlanRule>
            {
                new LenientBookingPlanSegmentLengthRule(32),
                new LenientBookingPlanVisibleGuardRule(255),
                new LenientBookingPlanRequiredSegmentRule("lenient booking plan needs a value"),
                new LenientBookingPlanCodeLengthRule(3600),
                new LenientBookingPlanRequiredCodeRule("lenient booking plan needs an entry"),
            });
    }
}
