namespace Acme.Workforce.Workplace.Booking.BookingReservation;

/// <summary>A rule a capped booking threshold must pass. Check returns its problems, or none.</summary>
public interface ICappedBookingThresholdRule
{
    List<string> Check(CappedBookingThreshold item);
}

public sealed record CappedBookingThresholdPriorityRangeRule(int Min, int Max) : ICappedBookingThresholdRule
{
    public List<string> Check(CappedBookingThreshold item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedBookingThresholdRequiredLocationRule(string Message) : ICappedBookingThresholdRule
{
    public List<string> Check(CappedBookingThreshold item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedBookingThresholdCodePrefixRule(string Prefix) : ICappedBookingThresholdRule
{
    public List<string> Check(CappedBookingThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedBookingThresholdSequenceRangeRule(int Min, int Max) : ICappedBookingThresholdRule
{
    public List<string> Check(CappedBookingThreshold item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a capped booking threshold.</summary>
public sealed record CappedBookingThresholdValidator(List<ICappedBookingThresholdRule> Rules)
{
    public List<string> Validate(CappedBookingThreshold item)
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

    public void RequireValid(CappedBookingThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedBookingThreshold> items)
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

/// <summary>Functions of capped booking threshold rules.</summary>
public static class CappedBookingThresholdRules
{
    public static CappedBookingThresholdValidator DefaultCappedBookingThresholdValidator()
    {
        return new CappedBookingThresholdValidator(
            new List<ICappedBookingThresholdRule>
            {
                new CappedBookingThresholdPriorityRangeRule(32, 255),
                new CappedBookingThresholdRequiredLocationRule("capped booking threshold needs this field"),
                new CappedBookingThresholdCodePrefixRule("X"),
                new CappedBookingThresholdSequenceRangeRule(80, 80),
            });
    }
}
