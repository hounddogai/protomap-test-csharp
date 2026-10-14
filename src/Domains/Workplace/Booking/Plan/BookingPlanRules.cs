namespace Acme.Workforce.Workplace.Booking.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a booking plan must pass. Check returns its problems, or none.</summary>
public interface IBookingPlanRule
{
    List<string> Check(BookingPlan item);
}

public sealed record BookingPlanWeightRangeRule(double Min, double Max) : IBookingPlanRule
{
    public List<string> Check(BookingPlan item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BookingPlanArchivedGuardRule(int Limit) : IBookingPlanRule
{
    public List<string> Check(BookingPlan item)
    {
        if (item.Archived && item.Priority > Limit)
        {
            return new List<string> { $"priority {item.Priority} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record BookingPlanRequiredNameRule(string Message) : IBookingPlanRule
{
    public List<string> Check(BookingPlan item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingPlanAllowedNameRule(List<string> Allowed) : IBookingPlanRule
{
    public List<string> Check(BookingPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is expired" };
        }
        return new List<string>();
    }
}

public sealed record BookingPlanNameLengthRule(int MaxLength) : IBookingPlanRule
{
    public List<string> Check(BookingPlan item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingPlanRequiredCodeRule(string Message) : IBookingPlanRule
{
    public List<string> Check(BookingPlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking plan.</summary>
public sealed record BookingPlanValidator(List<IBookingPlanRule> Rules)
{
    public List<string> Validate(BookingPlan item)
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

    public void RequireValid(BookingPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingPlan> items)
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

/// <summary>Functions of booking plan rules.</summary>
public static class BookingPlanRules
{
    public static BookingPlanValidator DefaultBookingPlanValidator()
    {
        return new BookingPlanValidator(
            new List<IBookingPlanRule>
            {
                new BookingPlanWeightRangeRule(8.4, 1567.4),
                new BookingPlanArchivedGuardRule(3600),
                new BookingPlanRequiredNameRule("booking plan needs an entry"),
                new BookingPlanAllowedNameRule(new List<string> { "pending" }),
                new BookingPlanNameLengthRule(64),
                new BookingPlanRequiredCodeRule("booking plan needs a value"),
            });
    }
}
