namespace Acme.Workforce.Compensation.PayBand.Reservation;

/// <summary>A rule a lenient pay band tier must pass. Check returns its problems, or none.</summary>
public interface ILenientPayBandTierRule
{
    List<string> Check(LenientPayBandTier item);
}

public sealed record LenientPayBandTierSummaryLengthRule(int MaxLength) : ILenientPayBandTierRule
{
    public List<string> Check(LenientPayBandTier item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPayBandTierActiveGuardRule(int Limit) : ILenientPayBandTierRule
{
    public List<string> Check(LenientPayBandTier item)
    {
        if (item.Active && item.ExpiresAt > Limit)
        {
            return new List<string> { $"expires at {item.ExpiresAt} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record LenientPayBandTierCodePrefixRule(string Prefix) : ILenientPayBandTierRule
{
    public List<string> Check(LenientPayBandTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient pay band tier.</summary>
public sealed record LenientPayBandTierValidator(List<ILenientPayBandTierRule> Rules)
{
    public List<string> Validate(LenientPayBandTier item)
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

    public void RequireValid(LenientPayBandTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientPayBandTier> items)
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

/// <summary>Functions of lenient pay band tier rules.</summary>
public static class LenientPayBandTierRules
{
    public static LenientPayBandTierValidator DefaultLenientPayBandTierValidator()
    {
        return new LenientPayBandTierValidator(
            new List<ILenientPayBandTierRule>
            {
                new LenientPayBandTierSummaryLengthRule(120),
                new LenientPayBandTierActiveGuardRule(32),
                new LenientPayBandTierCodePrefixRule("ACM"),
            });
    }
}
