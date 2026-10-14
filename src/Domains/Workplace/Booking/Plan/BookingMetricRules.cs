namespace Acme.Workforce.Workplace.Booking.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a booking metric must pass. Check returns its problems, or none.</summary>
public interface IBookingMetricRule
{
    List<string> Check(BookingMetric item);
}

public sealed record BookingMetricAllowedNotesRule(List<string> Allowed) : IBookingMetricRule
{
    public List<string> Check(BookingMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Notes)))
        {
            return new List<string> { $"notes {item.Notes} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record BookingMetricRetriesRangeRule(int Min, int Max) : IBookingMetricRule
{
    public List<string> Check(BookingMetric item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BookingMetricRequiredNameRule(string Message) : IBookingMetricRule
{
    public List<string> Check(BookingMetric item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking metric.</summary>
public sealed record BookingMetricValidator(List<IBookingMetricRule> Rules)
{
    public List<string> Validate(BookingMetric item)
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

    public void RequireValid(BookingMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingMetric> items)
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

/// <summary>Functions of booking metric rules.</summary>
public static class BookingMetricRules
{
    public static BookingMetricValidator DefaultBookingMetricValidator()
    {
        return new BookingMetricValidator(
            new List<IBookingMetricRule>
            {
                new BookingMetricAllowedNotesRule(new List<string> { "strict", "primary" }),
                new BookingMetricRetriesRangeRule(120, 3600),
                new BookingMetricRequiredNameRule("booking metric needs this field"),
            });
    }
}
