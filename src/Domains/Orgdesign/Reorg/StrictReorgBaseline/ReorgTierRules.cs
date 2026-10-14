namespace Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

/// <summary>A rule a reorg tier must pass. Check returns its problems, or none.</summary>
public interface IReorgTierRule
{
    List<string> Check(ReorgTier item);
}

public sealed record ReorgTierCodePrefixRule(string Prefix) : IReorgTierRule
{
    public List<string> Check(ReorgTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgTierTitleLengthRule(int MaxLength) : IReorgTierRule
{
    public List<string> Check(ReorgTier item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgTierCategoryLengthRule(int MaxLength) : IReorgTierRule
{
    public List<string> Check(ReorgTier item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgTierRequiredCategoryRule(string Message) : IReorgTierRule
{
    public List<string> Check(ReorgTier item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg tier.</summary>
public sealed record ReorgTierValidator(List<IReorgTierRule> Rules)
{
    public List<string> Validate(ReorgTier item)
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

    public void RequireValid(ReorgTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgTier> items)
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

/// <summary>Functions of reorg tier rules.</summary>
public static class ReorgTierRules
{
    public static ReorgTierValidator DefaultReorgTierValidator()
    {
        return new ReorgTierValidator(
            new List<IReorgTierRule>
            {
                new ReorgTierCodePrefixRule("Q"),
                new ReorgTierTitleLengthRule(255),
                new ReorgTierCategoryLengthRule(64),
                new ReorgTierRequiredCategoryRule("reorg tier needs this field"),
            });
    }
}
