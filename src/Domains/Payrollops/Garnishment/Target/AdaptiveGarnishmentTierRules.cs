namespace Acme.Workforce.Payrollops.Garnishment.Target;

/// <summary>A rule a adaptive garnishment tier must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveGarnishmentTierRule
{
    List<string> Check(AdaptiveGarnishmentTier item);
}

public sealed record AdaptiveGarnishmentTierCodePrefixRule(string Prefix) : IAdaptiveGarnishmentTierRule
{
    public List<string> Check(AdaptiveGarnishmentTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveGarnishmentTierTimezoneLengthRule(int MaxLength) : IAdaptiveGarnishmentTierRule
{
    public List<string> Check(AdaptiveGarnishmentTier item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveGarnishmentTierChecksumLengthRule(int MaxLength) : IAdaptiveGarnishmentTierRule
{
    public List<string> Check(AdaptiveGarnishmentTier item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive garnishment tier.</summary>
public sealed record AdaptiveGarnishmentTierValidator(List<IAdaptiveGarnishmentTierRule> Rules)
{
    public List<string> Validate(AdaptiveGarnishmentTier item)
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

    public void RequireValid(AdaptiveGarnishmentTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveGarnishmentTier> items)
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

/// <summary>Functions of adaptive garnishment tier rules.</summary>
public static class AdaptiveGarnishmentTierRules
{
    public static AdaptiveGarnishmentTierValidator DefaultAdaptiveGarnishmentTierValidator()
    {
        return new AdaptiveGarnishmentTierValidator(
            new List<IAdaptiveGarnishmentTierRule>
            {
                new AdaptiveGarnishmentTierCodePrefixRule("REF"),
                new AdaptiveGarnishmentTierTimezoneLengthRule(255),
                new AdaptiveGarnishmentTierChecksumLengthRule(80),
            });
    }
}
