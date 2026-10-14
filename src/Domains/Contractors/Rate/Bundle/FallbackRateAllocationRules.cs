namespace Acme.Workforce.Contractors.Rate.Bundle;

/// <summary>A rule a fallback rate allocation must pass. Check returns its problems, or none.</summary>
public interface IFallbackRateAllocationRule
{
    List<string> Check(FallbackRateAllocation item);
}

public sealed record FallbackRateAllocationExpiresAtRangeRule(int Min, int Max) : IFallbackRateAllocationRule
{
    public List<string> Check(FallbackRateAllocation item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackRateAllocationRequiredCodeRule(string Message) : IFallbackRateAllocationRule
{
    public List<string> Check(FallbackRateAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackRateAllocationCodePrefixRule(string Prefix) : IFallbackRateAllocationRule
{
    public List<string> Check(FallbackRateAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackRateAllocationRequiredNameRule(string Message) : IFallbackRateAllocationRule
{
    public List<string> Check(FallbackRateAllocation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackRateAllocationChannelLengthRule(int MaxLength) : IFallbackRateAllocationRule
{
    public List<string> Check(FallbackRateAllocation item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackRateAllocationNameLengthRule(int MaxLength) : IFallbackRateAllocationRule
{
    public List<string> Check(FallbackRateAllocation item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback rate allocation.</summary>
public sealed record FallbackRateAllocationValidator(List<IFallbackRateAllocationRule> Rules)
{
    public List<string> Validate(FallbackRateAllocation item)
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

    public void RequireValid(FallbackRateAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackRateAllocation> items)
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

/// <summary>Functions of fallback rate allocation rules.</summary>
public static class FallbackRateAllocationRules
{
    public static FallbackRateAllocationValidator DefaultFallbackRateAllocationValidator()
    {
        return new FallbackRateAllocationValidator(
            new List<IFallbackRateAllocationRule>
            {
                new FallbackRateAllocationExpiresAtRangeRule(32, 86400),
                new FallbackRateAllocationRequiredCodeRule("fallback rate allocation needs this field"),
                new FallbackRateAllocationCodePrefixRule("X"),
                new FallbackRateAllocationRequiredNameRule("fallback rate allocation needs an entry"),
                new FallbackRateAllocationChannelLengthRule(1000),
                new FallbackRateAllocationNameLengthRule(120),
            });
    }
}
