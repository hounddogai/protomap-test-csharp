namespace Acme.Workforce.Workplace.Booking.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a booking ledger must pass. Check returns its problems, or none.</summary>
public interface IBookingLedgerRule
{
    List<string> Check(BookingLedger item);
}

public sealed record BookingLedgerRequiredSkillRule(string Message) : IBookingLedgerRule
{
    public List<string> Check(BookingLedger item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingLedgerAllowedSkillRule(List<string> Allowed) : IBookingLedgerRule
{
    public List<string> Check(BookingLedger item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record BookingLedgerNameLengthRule(int MaxLength) : IBookingLedgerRule
{
    public List<string> Check(BookingLedger item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingLedgerCapacityRangeRule(int Min, int Max) : IBookingLedgerRule
{
    public List<string> Check(BookingLedger item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a booking ledger.</summary>
public sealed record BookingLedgerValidator(List<IBookingLedgerRule> Rules)
{
    public List<string> Validate(BookingLedger item)
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

    public void RequireValid(BookingLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingLedger> items)
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

/// <summary>Functions of booking ledger rules.</summary>
public static class BookingLedgerRules
{
    public static BookingLedgerValidator DefaultBookingLedgerValidator()
    {
        return new BookingLedgerValidator(
            new List<IBookingLedgerRule>
            {
                new BookingLedgerRequiredSkillRule("booking ledger needs this field"),
                new BookingLedgerAllowedSkillRule(new List<string> { "tiered", "batched" }),
                new BookingLedgerNameLengthRule(3600),
                new BookingLedgerCapacityRangeRule(120, 1000),
            });
    }
}
