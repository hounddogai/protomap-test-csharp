namespace Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>A rule a seasonal span digest must pass. Check returns its problems, or none.</summary>
public interface ISeasonalSpanDigestRule
{
    List<string> Check(SeasonalSpanDigest item);
}

public sealed record SeasonalSpanDigestRequiredSourceRule(string Message) : ISeasonalSpanDigestRule
{
    public List<string> Check(SeasonalSpanDigest item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalSpanDigestNameLengthRule(int MaxLength) : ISeasonalSpanDigestRule
{
    public List<string> Check(SeasonalSpanDigest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalSpanDigestCodePrefixRule(string Prefix) : ISeasonalSpanDigestRule
{
    public List<string> Check(SeasonalSpanDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal span digest.</summary>
public sealed record SeasonalSpanDigestValidator(List<ISeasonalSpanDigestRule> Rules)
{
    public List<string> Validate(SeasonalSpanDigest item)
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

    public void RequireValid(SeasonalSpanDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalSpanDigest> items)
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

/// <summary>Functions of seasonal span digest rules.</summary>
public static class SeasonalSpanDigestRules
{
    public static SeasonalSpanDigestValidator DefaultSeasonalSpanDigestValidator()
    {
        return new SeasonalSpanDigestValidator(
            new List<ISeasonalSpanDigestRule>
            {
                new SeasonalSpanDigestRequiredSourceRule("seasonal span digest needs this field"),
                new SeasonalSpanDigestNameLengthRule(1000),
                new SeasonalSpanDigestCodePrefixRule("X"),
            });
    }
}
