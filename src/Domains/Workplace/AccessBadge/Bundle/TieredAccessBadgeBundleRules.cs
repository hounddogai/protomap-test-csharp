namespace Acme.Workforce.Workplace.AccessBadge.Bundle;

/// <summary>A rule a tiered access badge bundle must pass. Check returns its problems, or none.</summary>
public interface ITieredAccessBadgeBundleRule
{
    List<string> Check(TieredAccessBadgeBundle item);
}

public sealed record TieredAccessBadgeBundleJobCodeLengthRule(int MaxLength) : ITieredAccessBadgeBundleRule
{
    public List<string> Check(TieredAccessBadgeBundle item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredAccessBadgeBundleTitleLengthRule(int MaxLength) : ITieredAccessBadgeBundleRule
{
    public List<string> Check(TieredAccessBadgeBundle item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredAccessBadgeBundleCodePrefixRule(string Prefix) : ITieredAccessBadgeBundleRule
{
    public List<string> Check(TieredAccessBadgeBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredAccessBadgeBundleStartsAtRangeRule(int Min, int Max) : ITieredAccessBadgeBundleRule
{
    public List<string> Check(TieredAccessBadgeBundle item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredAccessBadgeBundleCategoryLengthRule(int MaxLength) : ITieredAccessBadgeBundleRule
{
    public List<string> Check(TieredAccessBadgeBundle item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredAccessBadgeBundleRequiredCategoryRule(string Message) : ITieredAccessBadgeBundleRule
{
    public List<string> Check(TieredAccessBadgeBundle item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered access badge bundle.</summary>
public sealed record TieredAccessBadgeBundleValidator(List<ITieredAccessBadgeBundleRule> Rules)
{
    public List<string> Validate(TieredAccessBadgeBundle item)
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

    public void RequireValid(TieredAccessBadgeBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredAccessBadgeBundle> items)
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

/// <summary>Functions of tiered access badge bundle rules.</summary>
public static class TieredAccessBadgeBundleRules
{
    public static TieredAccessBadgeBundleValidator DefaultTieredAccessBadgeBundleValidator()
    {
        return new TieredAccessBadgeBundleValidator(
            new List<ITieredAccessBadgeBundleRule>
            {
                new TieredAccessBadgeBundleJobCodeLengthRule(32),
                new TieredAccessBadgeBundleTitleLengthRule(64),
                new TieredAccessBadgeBundleCodePrefixRule("ACM"),
                new TieredAccessBadgeBundleStartsAtRangeRule(120, 86400),
                new TieredAccessBadgeBundleCategoryLengthRule(120),
                new TieredAccessBadgeBundleRequiredCategoryRule("tiered access badge bundle needs a value"),
            });
    }
}
