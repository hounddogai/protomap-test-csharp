namespace Acme.Workforce.Workplace.Booking.Digest;

/// <summary>A rule a pending booking digest must pass. Check returns its problems, or none.</summary>
public interface IPendingBookingDigestRule
{
    List<string> Check(PendingBookingDigest item);
}

public sealed record PendingBookingDigestRequiredCodeRule(string Message) : IPendingBookingDigestRule
{
    public List<string> Check(PendingBookingDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingBookingDigestThresholdRangeRule(double Min, double Max) : IPendingBookingDigestRule
{
    public List<string> Check(PendingBookingDigest item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingBookingDigestSummaryLengthRule(int MaxLength) : IPendingBookingDigestRule
{
    public List<string> Check(PendingBookingDigest item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending booking digest.</summary>
public sealed record PendingBookingDigestValidator(List<IPendingBookingDigestRule> Rules)
{
    public List<string> Validate(PendingBookingDigest item)
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

    public void RequireValid(PendingBookingDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingBookingDigest> items)
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

/// <summary>Functions of pending booking digest rules.</summary>
public static class PendingBookingDigestRules
{
    public static PendingBookingDigestValidator DefaultPendingBookingDigestValidator()
    {
        return new PendingBookingDigestValidator(
            new List<IPendingBookingDigestRule>
            {
                new PendingBookingDigestRequiredCodeRule("pending booking digest needs this field"),
                new PendingBookingDigestThresholdRangeRule(2.0, 4750.3),
                new PendingBookingDigestSummaryLengthRule(255),
            });
    }
}
