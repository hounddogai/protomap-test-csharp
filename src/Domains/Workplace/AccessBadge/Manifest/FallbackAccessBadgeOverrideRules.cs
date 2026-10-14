namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

/// <summary>A rule a fallback access badge override must pass. Check returns its problems, or none.</summary>
public interface IFallbackAccessBadgeOverrideRule
{
    List<string> Check(FallbackAccessBadgeOverride item);
}

public sealed record FallbackAccessBadgeOverrideCodePrefixRule(string Prefix) : IFallbackAccessBadgeOverrideRule
{
    public List<string> Check(FallbackAccessBadgeOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAccessBadgeOverrideRequiredChecksumRule(string Message) : IFallbackAccessBadgeOverrideRule
{
    public List<string> Check(FallbackAccessBadgeOverride item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackAccessBadgeOverrideRetriesRangeRule(int Min, int Max) : IFallbackAccessBadgeOverrideRule
{
    public List<string> Check(FallbackAccessBadgeOverride item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackAccessBadgeOverrideChecksumLengthRule(int MaxLength) : IFallbackAccessBadgeOverrideRule
{
    public List<string> Check(FallbackAccessBadgeOverride item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAccessBadgeOverrideRequiredCodeRule(string Message) : IFallbackAccessBadgeOverrideRule
{
    public List<string> Check(FallbackAccessBadgeOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback access badge override.</summary>
public sealed record FallbackAccessBadgeOverrideValidator(List<IFallbackAccessBadgeOverrideRule> Rules)
{
    public List<string> Validate(FallbackAccessBadgeOverride item)
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

    public void RequireValid(FallbackAccessBadgeOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackAccessBadgeOverride> items)
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

/// <summary>Functions of fallback access badge override rules.</summary>
public static class FallbackAccessBadgeOverrideRules
{
    public static FallbackAccessBadgeOverrideValidator DefaultFallbackAccessBadgeOverrideValidator()
    {
        return new FallbackAccessBadgeOverrideValidator(
            new List<IFallbackAccessBadgeOverrideRule>
            {
                new FallbackAccessBadgeOverrideCodePrefixRule("ACM"),
                new FallbackAccessBadgeOverrideRequiredChecksumRule("fallback access badge override needs a value"),
                new FallbackAccessBadgeOverrideRetriesRangeRule(64, 80),
                new FallbackAccessBadgeOverrideChecksumLengthRule(32),
                new FallbackAccessBadgeOverrideRequiredCodeRule("fallback access badge override needs this field"),
            });
    }
}
