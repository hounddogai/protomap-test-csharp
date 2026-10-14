namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback nomination bundle must pass. Check returns its problems, or none.</summary>
public interface IFallbackNominationBundleRule
{
    List<string> Check(FallbackNominationBundle item);
}

public sealed record FallbackNominationBundleUpdatedAtRangeRule(int Min, int Max) : IFallbackNominationBundleRule
{
    public List<string> Check(FallbackNominationBundle item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackNominationBundleTitleLengthRule(int MaxLength) : IFallbackNominationBundleRule
{
    public List<string> Check(FallbackNominationBundle item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackNominationBundleAllowedCurrencyRule(List<string> Allowed) : IFallbackNominationBundleRule
{
    public List<string> Check(FallbackNominationBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Currency)))
        {
            return new List<string> { $"currency {item.Currency} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record FallbackNominationBundleRequiredTitleRule(string Message) : IFallbackNominationBundleRule
{
    public List<string> Check(FallbackNominationBundle item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackNominationBundleCodePrefixRule(string Prefix) : IFallbackNominationBundleRule
{
    public List<string> Check(FallbackNominationBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackNominationBundleRatioRangeRule(double Min, double Max) : IFallbackNominationBundleRule
{
    public List<string> Check(FallbackNominationBundle item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback nomination bundle.</summary>
public sealed record FallbackNominationBundleValidator(List<IFallbackNominationBundleRule> Rules)
{
    public List<string> Validate(FallbackNominationBundle item)
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

    public void RequireValid(FallbackNominationBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackNominationBundle> items)
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

/// <summary>Functions of fallback nomination bundle rules.</summary>
public static class FallbackNominationBundleRules
{
    public static FallbackNominationBundleValidator DefaultFallbackNominationBundleValidator()
    {
        return new FallbackNominationBundleValidator(
            new List<IFallbackNominationBundleRule>
            {
                new FallbackNominationBundleUpdatedAtRangeRule(120, 255),
                new FallbackNominationBundleTitleLengthRule(86400),
                new FallbackNominationBundleAllowedCurrencyRule(new List<string> { "scheduled", "global", "capped" }),
                new FallbackNominationBundleRequiredTitleRule("fallback nomination bundle needs an entry"),
                new FallbackNominationBundleCodePrefixRule("Q"),
                new FallbackNominationBundleRatioRangeRule(6.4, 4637.4),
            });
    }
}
