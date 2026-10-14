namespace Acme.Workforce.Contractors.Rate.Plan;

/// <summary>A rule a rate mapping must pass. Check returns its problems, or none.</summary>
public interface IRateMappingRule
{
    List<string> Check(RateMapping item);
}

public sealed record RateMappingRequiredTitleRule(string Message) : IRateMappingRule
{
    public List<string> Check(RateMapping item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateMappingVersionRangeRule(int Min, int Max) : IRateMappingRule
{
    public List<string> Check(RateMapping item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RateMappingCodePrefixRule(string Prefix) : IRateMappingRule
{
    public List<string> Check(RateMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RateMappingTitleLengthRule(int MaxLength) : IRateMappingRule
{
    public List<string> Check(RateMapping item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateMappingRequiredPhoneRule(string Message) : IRateMappingRule
{
    public List<string> Check(RateMapping item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate mapping.</summary>
public sealed record RateMappingValidator(List<IRateMappingRule> Rules)
{
    public List<string> Validate(RateMapping item)
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

    public void RequireValid(RateMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateMapping> items)
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

/// <summary>Functions of rate mapping rules.</summary>
public static class RateMappingRules
{
    public static RateMappingValidator DefaultRateMappingValidator()
    {
        return new RateMappingValidator(
            new List<IRateMappingRule>
            {
                new RateMappingRequiredTitleRule("rate mapping needs a value"),
                new RateMappingVersionRangeRule(64, 3600),
                new RateMappingCodePrefixRule("ACM"),
                new RateMappingTitleLengthRule(1000),
                new RateMappingRequiredPhoneRule("rate mapping needs a value"),
            });
    }
}
