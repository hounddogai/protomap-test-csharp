namespace Acme.Workforce.Contractors.Rate.RateCycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback rate cycle must pass. Check returns its problems, or none.</summary>
public interface IFallbackRateCycleRule
{
    List<string> Check(FallbackRateCycle item);
}

public sealed record FallbackRateCycleAllowedNameRule(List<string> Allowed) : IFallbackRateCycleRule
{
    public List<string> Check(FallbackRateCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record FallbackRateCycleRequiredCodeRule(string Message) : IFallbackRateCycleRule
{
    public List<string> Check(FallbackRateCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackRateCycleCodePrefixRule(string Prefix) : IFallbackRateCycleRule
{
    public List<string> Check(FallbackRateCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackRateCycleTagsLimitRule(int MaxCount, List<string> Banned) : IFallbackRateCycleRule
{
    public List<string> Check(FallbackRateCycle item)
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

public sealed record FallbackRateCycleRequiredCategoryRule(string Message) : IFallbackRateCycleRule
{
    public List<string> Check(FallbackRateCycle item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackRateCycleAmountRangeRule(double Min, double Max) : IFallbackRateCycleRule
{
    public List<string> Check(FallbackRateCycle item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback rate cycle.</summary>
public sealed record FallbackRateCycleValidator(List<IFallbackRateCycleRule> Rules)
{
    public List<string> Validate(FallbackRateCycle item)
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

    public void RequireValid(FallbackRateCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackRateCycle> items)
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

/// <summary>Functions of fallback rate cycle rules.</summary>
public static class FallbackRateCycleRules
{
    public static FallbackRateCycleValidator DefaultFallbackRateCycleValidator()
    {
        return new FallbackRateCycleValidator(
            new List<IFallbackRateCycleRule>
            {
                new FallbackRateCycleAllowedNameRule(new List<string>()),
                new FallbackRateCycleRequiredCodeRule("fallback rate cycle needs an entry"),
                new FallbackRateCycleCodePrefixRule("OPS"),
                new FallbackRateCycleTagsLimitRule(120, new List<string> { "seasonal" }),
                new FallbackRateCycleRequiredCategoryRule("fallback rate cycle needs an entry"),
                new FallbackRateCycleAmountRangeRule(7.4, 748.5),
            });
    }
}
