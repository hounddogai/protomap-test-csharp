namespace Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

/// <summary>A rule a tiered policy baseline must pass. Check returns its problems, or none.</summary>
public interface ITieredPolicyBaselineRule
{
    List<string> Check(TieredPolicyBaseline item);
}

public sealed record TieredPolicyBaselineTitleLengthRule(int MaxLength) : ITieredPolicyBaselineRule
{
    public List<string> Check(TieredPolicyBaseline item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPolicyBaselineCodeLengthRule(int MaxLength) : ITieredPolicyBaselineRule
{
    public List<string> Check(TieredPolicyBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPolicyBaselineRequiredLocationRule(string Message) : ITieredPolicyBaselineRule
{
    public List<string> Check(TieredPolicyBaseline item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredPolicyBaselineRequiredCodeRule(string Message) : ITieredPolicyBaselineRule
{
    public List<string> Check(TieredPolicyBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredPolicyBaselineCodePrefixRule(string Prefix) : ITieredPolicyBaselineRule
{
    public List<string> Check(TieredPolicyBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPolicyBaselineLocationLengthRule(int MaxLength) : ITieredPolicyBaselineRule
{
    public List<string> Check(TieredPolicyBaseline item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered policy baseline.</summary>
public sealed record TieredPolicyBaselineValidator(List<ITieredPolicyBaselineRule> Rules)
{
    public List<string> Validate(TieredPolicyBaseline item)
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

    public void RequireValid(TieredPolicyBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredPolicyBaseline> items)
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

/// <summary>Functions of tiered policy baseline rules.</summary>
public static class TieredPolicyBaselineRules
{
    public static TieredPolicyBaselineValidator DefaultTieredPolicyBaselineValidator()
    {
        return new TieredPolicyBaselineValidator(
            new List<ITieredPolicyBaselineRule>
            {
                new TieredPolicyBaselineTitleLengthRule(120),
                new TieredPolicyBaselineCodeLengthRule(16),
                new TieredPolicyBaselineRequiredLocationRule("tiered policy baseline needs a value"),
                new TieredPolicyBaselineRequiredCodeRule("tiered policy baseline needs a value"),
                new TieredPolicyBaselineCodePrefixRule("ACM"),
                new TieredPolicyBaselineLocationLengthRule(32),
            });
    }
}
