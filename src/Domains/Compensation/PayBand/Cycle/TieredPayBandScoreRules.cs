namespace Acme.Workforce.Compensation.PayBand.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered pay band score must pass. Check returns its problems, or none.</summary>
public interface ITieredPayBandScoreRule
{
    List<string> Check(TieredPayBandScore item);
}

public sealed record TieredPayBandScoreRequiredCostCenterRule(string Message) : ITieredPayBandScoreRule
{
    public List<string> Check(TieredPayBandScore item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredPayBandScoreCodeLengthRule(int MaxLength) : ITieredPayBandScoreRule
{
    public List<string> Check(TieredPayBandScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPayBandScoreRequiredTitleRule(string Message) : ITieredPayBandScoreRule
{
    public List<string> Check(TieredPayBandScore item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredPayBandScoreAllowedUnitRule(List<string> Allowed) : ITieredPayBandScoreRule
{
    public List<string> Check(TieredPayBandScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Unit)))
        {
            return new List<string> { $"unit {item.Unit} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record TieredPayBandScoreCodePrefixRule(string Prefix) : ITieredPayBandScoreRule
{
    public List<string> Check(TieredPayBandScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered pay band score.</summary>
public sealed record TieredPayBandScoreValidator(List<ITieredPayBandScoreRule> Rules)
{
    public List<string> Validate(TieredPayBandScore item)
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

    public void RequireValid(TieredPayBandScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredPayBandScore> items)
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

/// <summary>Functions of tiered pay band score rules.</summary>
public static class TieredPayBandScoreRules
{
    public static TieredPayBandScoreValidator DefaultTieredPayBandScoreValidator()
    {
        return new TieredPayBandScoreValidator(
            new List<ITieredPayBandScoreRule>
            {
                new TieredPayBandScoreRequiredCostCenterRule("tiered pay band score needs this field"),
                new TieredPayBandScoreCodeLengthRule(120),
                new TieredPayBandScoreRequiredTitleRule("tiered pay band score needs an entry"),
                new TieredPayBandScoreAllowedUnitRule(new List<string> { "legacy", "regional" }),
                new TieredPayBandScoreCodePrefixRule("OPS"),
            });
    }
}
