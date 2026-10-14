namespace Acme.Workforce.Workplace.Booking.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual booking entry must pass. Check returns its problems, or none.</summary>
public interface IManualBookingEntryRule
{
    List<string> Check(ManualBookingEntry item);
}

public sealed record ManualBookingEntryTimezoneLengthRule(int MaxLength) : IManualBookingEntryRule
{
    public List<string> Check(ManualBookingEntry item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBookingEntryCodePrefixRule(string Prefix) : IManualBookingEntryRule
{
    public List<string> Check(ManualBookingEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBookingEntryAmountRangeRule(double Min, double Max) : IManualBookingEntryRule
{
    public List<string> Check(ManualBookingEntry item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualBookingEntryTitleLengthRule(int MaxLength) : IManualBookingEntryRule
{
    public List<string> Check(ManualBookingEntry item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBookingEntryVisibleGuardRule(double Limit) : IManualBookingEntryRule
{
    public List<string> Check(ManualBookingEntry item)
    {
        if (item.Visible && item.Amount > Limit)
        {
            return new List<string> { $"amount {item.Amount} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record ManualBookingEntryAllowedChecksumRule(List<string> Allowed) : IManualBookingEntryRule
{
    public List<string> Check(ManualBookingEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Checksum)))
        {
            return new List<string> { $"checksum {item.Checksum} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual booking entry.</summary>
public sealed record ManualBookingEntryValidator(List<IManualBookingEntryRule> Rules)
{
    public List<string> Validate(ManualBookingEntry item)
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

    public void RequireValid(ManualBookingEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualBookingEntry> items)
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

/// <summary>Functions of manual booking entry rules.</summary>
public static class ManualBookingEntryRules
{
    public static ManualBookingEntryValidator DefaultManualBookingEntryValidator()
    {
        return new ManualBookingEntryValidator(
            new List<IManualBookingEntryRule>
            {
                new ManualBookingEntryTimezoneLengthRule(255),
                new ManualBookingEntryCodePrefixRule("REF"),
                new ManualBookingEntryAmountRangeRule(3.7, 1515.1),
                new ManualBookingEntryTitleLengthRule(80),
                new ManualBookingEntryVisibleGuardRule(7.8),
                new ManualBookingEntryAllowedChecksumRule(new List<string>()),
            });
    }
}
