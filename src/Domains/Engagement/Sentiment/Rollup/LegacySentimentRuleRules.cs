namespace Acme.Workforce.Engagement.Sentiment.Rollup;

/// <summary>A rule a legacy sentiment rule must pass. Check returns its problems, or none.</summary>
public interface ILegacySentimentRuleRule
{
    List<string> Check(LegacySentimentRule item);
}

public sealed record LegacySentimentRuleCodePrefixRule(string Prefix) : ILegacySentimentRuleRule
{
    public List<string> Check(LegacySentimentRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacySentimentRuleLocaleLengthRule(int MaxLength) : ILegacySentimentRuleRule
{
    public List<string> Check(LegacySentimentRule item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacySentimentRuleActiveGuardRule(int Limit) : ILegacySentimentRuleRule
{
    public List<string> Check(LegacySentimentRule item)
    {
        if (item.Active && item.BatchSize > Limit)
        {
            return new List<string> { $"batch size {item.BatchSize} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy sentiment rule.</summary>
public sealed record LegacySentimentRuleValidator(List<ILegacySentimentRuleRule> Rules)
{
    public List<string> Validate(LegacySentimentRule item)
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

    public void RequireValid(LegacySentimentRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacySentimentRule> items)
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

/// <summary>Functions of legacy sentiment rule rules.</summary>
public static class LegacySentimentRuleRules
{
    public static LegacySentimentRuleValidator DefaultLegacySentimentRuleValidator()
    {
        return new LegacySentimentRuleValidator(
            new List<ILegacySentimentRuleRule>
            {
                new LegacySentimentRuleCodePrefixRule("ACM"),
                new LegacySentimentRuleLocaleLengthRule(16),
                new LegacySentimentRuleActiveGuardRule(255),
            });
    }
}
