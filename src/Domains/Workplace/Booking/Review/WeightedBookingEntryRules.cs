namespace Acme.Workforce.Workplace.Booking.Review;

/// <summary>A rule a weighted booking entry must pass. Check returns its problems, or none.</summary>
public interface IWeightedBookingEntryRule
{
    List<string> Check(WeightedBookingEntry item);
}

public sealed record WeightedBookingEntryCodePrefixRule(string Prefix) : IWeightedBookingEntryRule
{
    public List<string> Check(WeightedBookingEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedBookingEntryCodeLengthRule(int MaxLength) : IWeightedBookingEntryRule
{
    public List<string> Check(WeightedBookingEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedBookingEntryChannelLengthRule(int MaxLength) : IWeightedBookingEntryRule
{
    public List<string> Check(WeightedBookingEntry item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted booking entry.</summary>
public sealed record WeightedBookingEntryValidator(List<IWeightedBookingEntryRule> Rules)
{
    public List<string> Validate(WeightedBookingEntry item)
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

    public void RequireValid(WeightedBookingEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedBookingEntry> items)
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

/// <summary>Functions of weighted booking entry rules.</summary>
public static class WeightedBookingEntryRules
{
    public static WeightedBookingEntryValidator DefaultWeightedBookingEntryValidator()
    {
        return new WeightedBookingEntryValidator(
            new List<IWeightedBookingEntryRule>
            {
                new WeightedBookingEntryCodePrefixRule("OPS"),
                new WeightedBookingEntryCodeLengthRule(1000),
                new WeightedBookingEntryChannelLengthRule(16),
            });
    }
}
