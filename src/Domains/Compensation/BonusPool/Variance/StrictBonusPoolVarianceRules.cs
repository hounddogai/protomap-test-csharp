namespace Acme.Workforce.Compensation.BonusPool.Variance;

/// <summary>A rule a strict bonus pool variance must pass. Check returns its problems, or none.</summary>
public interface IStrictBonusPoolVarianceRule
{
    List<string> Check(StrictBonusPoolVariance item);
}

public sealed record StrictBonusPoolVarianceTagsLimitRule(int MaxCount, List<string> Banned) : IStrictBonusPoolVarianceRule
{
    public List<string> Check(StrictBonusPoolVariance item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record StrictBonusPoolVarianceCurrencyLengthRule(int MaxLength) : IStrictBonusPoolVarianceRule
{
    public List<string> Check(StrictBonusPoolVariance item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictBonusPoolVarianceManagerIdLengthRule(int MaxLength) : IStrictBonusPoolVarianceRule
{
    public List<string> Check(StrictBonusPoolVariance item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictBonusPoolVarianceCodePrefixRule(string Prefix) : IStrictBonusPoolVarianceRule
{
    public List<string> Check(StrictBonusPoolVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StrictBonusPoolVarianceRequiredCodeRule(string Message) : IStrictBonusPoolVarianceRule
{
    public List<string> Check(StrictBonusPoolVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict bonus pool variance.</summary>
public sealed record StrictBonusPoolVarianceValidator(List<IStrictBonusPoolVarianceRule> Rules)
{
    public List<string> Validate(StrictBonusPoolVariance item)
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

    public void RequireValid(StrictBonusPoolVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictBonusPoolVariance> items)
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

/// <summary>Functions of strict bonus pool variance rules.</summary>
public static class StrictBonusPoolVarianceRules
{
    public static StrictBonusPoolVarianceValidator DefaultStrictBonusPoolVarianceValidator()
    {
        return new StrictBonusPoolVarianceValidator(
            new List<IStrictBonusPoolVarianceRule>
            {
                new StrictBonusPoolVarianceTagsLimitRule(32, new List<string> { "primary" }),
                new StrictBonusPoolVarianceCurrencyLengthRule(255),
                new StrictBonusPoolVarianceManagerIdLengthRule(16),
                new StrictBonusPoolVarianceCodePrefixRule("X"),
                new StrictBonusPoolVarianceRequiredCodeRule("strict bonus pool variance needs this field"),
            });
    }
}
