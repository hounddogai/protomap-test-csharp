namespace Acme.Workforce.Engagement.Sentiment.Rollup;

/// <summary>A rule a primary sentiment rollup must pass. Check returns its problems, or none.</summary>
public interface IPrimarySentimentRollupRule
{
    List<string> Check(PrimarySentimentRollup item);
}

public sealed record PrimarySentimentRollupCodePrefixRule(string Prefix) : IPrimarySentimentRollupRule
{
    public List<string> Check(PrimarySentimentRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimarySentimentRollupRequiredLabelRule(string Message) : IPrimarySentimentRollupRule
{
    public List<string> Check(PrimarySentimentRollup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimarySentimentRollupRequiredTeamRule(string Message) : IPrimarySentimentRollupRule
{
    public List<string> Check(PrimarySentimentRollup item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary sentiment rollup.</summary>
public sealed record PrimarySentimentRollupValidator(List<IPrimarySentimentRollupRule> Rules)
{
    public List<string> Validate(PrimarySentimentRollup item)
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

    public void RequireValid(PrimarySentimentRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimarySentimentRollup> items)
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

/// <summary>Functions of primary sentiment rollup rules.</summary>
public static class PrimarySentimentRollupRules
{
    public static PrimarySentimentRollupValidator DefaultPrimarySentimentRollupValidator()
    {
        return new PrimarySentimentRollupValidator(
            new List<IPrimarySentimentRollupRule>
            {
                new PrimarySentimentRollupCodePrefixRule("REF"),
                new PrimarySentimentRollupRequiredLabelRule("primary sentiment rollup needs an entry"),
                new PrimarySentimentRollupRequiredTeamRule("primary sentiment rollup needs a value"),
            });
    }
}
