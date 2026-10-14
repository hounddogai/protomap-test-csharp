namespace Acme.Workforce.Workplace.Booking.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global booking tier must pass. Check returns its problems, or none.</summary>
public interface IGlobalBookingTierRule
{
    List<string> Check(GlobalBookingTier item);
}

public sealed record GlobalBookingTierCodePrefixRule(string Prefix) : IGlobalBookingTierRule
{
    public List<string> Check(GlobalBookingTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalBookingTierRequiredCodeRule(string Message) : IGlobalBookingTierRule
{
    public List<string> Check(GlobalBookingTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalBookingTierTitleLengthRule(int MaxLength) : IGlobalBookingTierRule
{
    public List<string> Check(GlobalBookingTier item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalBookingTierAllowedGradeRule(List<string> Allowed) : IGlobalBookingTierRule
{
    public List<string> Check(GlobalBookingTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record GlobalBookingTierPriorityRangeRule(int Min, int Max) : IGlobalBookingTierRule
{
    public List<string> Check(GlobalBookingTier item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global booking tier.</summary>
public sealed record GlobalBookingTierValidator(List<IGlobalBookingTierRule> Rules)
{
    public List<string> Validate(GlobalBookingTier item)
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

    public void RequireValid(GlobalBookingTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalBookingTier> items)
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

/// <summary>Functions of global booking tier rules.</summary>
public static class GlobalBookingTierRules
{
    public static GlobalBookingTierValidator DefaultGlobalBookingTierValidator()
    {
        return new GlobalBookingTierValidator(
            new List<IGlobalBookingTierRule>
            {
                new GlobalBookingTierCodePrefixRule("ACM"),
                new GlobalBookingTierRequiredCodeRule("global booking tier needs a value"),
                new GlobalBookingTierTitleLengthRule(32),
                new GlobalBookingTierAllowedGradeRule(new List<string> { "capped", "legacy", "lenient" }),
                new GlobalBookingTierPriorityRangeRule(64, 86400),
            });
    }
}
