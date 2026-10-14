namespace Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

/// <summary>A rule a tiered reorg variance must pass. Check returns its problems, or none.</summary>
public interface ITieredReorgVarianceRule
{
    List<string> Check(TieredReorgVariance item);
}

public sealed record TieredReorgVarianceCodePrefixRule(string Prefix) : ITieredReorgVarianceRule
{
    public List<string> Check(TieredReorgVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredReorgVarianceRequiredTimezoneRule(string Message) : ITieredReorgVarianceRule
{
    public List<string> Check(TieredReorgVariance item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredReorgVarianceUpdatedAtRangeRule(int Min, int Max) : ITieredReorgVarianceRule
{
    public List<string> Check(TieredReorgVariance item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredReorgVarianceRequiredSegmentRule(string Message) : ITieredReorgVarianceRule
{
    public List<string> Check(TieredReorgVariance item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredReorgVarianceSegmentLengthRule(int MaxLength) : ITieredReorgVarianceRule
{
    public List<string> Check(TieredReorgVariance item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered reorg variance.</summary>
public sealed record TieredReorgVarianceValidator(List<ITieredReorgVarianceRule> Rules)
{
    public List<string> Validate(TieredReorgVariance item)
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

    public void RequireValid(TieredReorgVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredReorgVariance> items)
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

/// <summary>Functions of tiered reorg variance rules.</summary>
public static class TieredReorgVarianceRules
{
    public static TieredReorgVarianceValidator DefaultTieredReorgVarianceValidator()
    {
        return new TieredReorgVarianceValidator(
            new List<ITieredReorgVarianceRule>
            {
                new TieredReorgVarianceCodePrefixRule("X"),
                new TieredReorgVarianceRequiredTimezoneRule("tiered reorg variance needs a value"),
                new TieredReorgVarianceUpdatedAtRangeRule(64, 1000),
                new TieredReorgVarianceRequiredSegmentRule("tiered reorg variance needs a value"),
                new TieredReorgVarianceSegmentLengthRule(16),
            });
    }
}
