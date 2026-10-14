namespace Acme.Workforce.Workplace.Booking.StrictBookingPlan;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a booking slot must pass. Check returns its problems, or none.</summary>
public interface IBookingSlotRule
{
    List<string> Check(BookingSlot item);
}

public sealed record BookingSlotRequiredContactEmailRule(string Message) : IBookingSlotRule
{
    public List<string> Check(BookingSlot item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingSlotVisibleGuardRule(double Limit) : IBookingSlotRule
{
    public List<string> Check(BookingSlot item)
    {
        if (item.Visible && item.Rating > Limit)
        {
            return new List<string> { $"rating {item.Rating} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record BookingSlotRequiredCodeRule(string Message) : IBookingSlotRule
{
    public List<string> Check(BookingSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingSlotAllowedChannelRule(List<string> Allowed) : IBookingSlotRule
{
    public List<string> Check(BookingSlot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record BookingSlotTenureMonthsRangeRule(int Min, int Max) : IBookingSlotRule
{
    public List<string> Check(BookingSlot item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking slot.</summary>
public sealed record BookingSlotValidator(List<IBookingSlotRule> Rules)
{
    public List<string> Validate(BookingSlot item)
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

    public void RequireValid(BookingSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingSlot> items)
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

/// <summary>Functions of booking slot rules.</summary>
public static class BookingSlotRules
{
    public static BookingSlotValidator DefaultBookingSlotValidator()
    {
        return new BookingSlotValidator(
            new List<IBookingSlotRule>
            {
                new BookingSlotRequiredContactEmailRule("booking slot needs an entry"),
                new BookingSlotVisibleGuardRule(9.4),
                new BookingSlotRequiredCodeRule("booking slot needs an entry"),
                new BookingSlotAllowedChannelRule(new List<string> { "lenient", "fallback" }),
                new BookingSlotTenureMonthsRangeRule(32, 120),
            });
    }
}
