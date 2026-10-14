namespace Acme.Workforce.Orgdesign.Span.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a span tier must pass. Check returns its problems, or none.</summary>
public interface ISpanTierRule
{
    List<string> Check(SpanTier item);
}

public sealed record SpanTierAllowedDepartmentRule(List<string> Allowed) : ISpanTierRule
{
    public List<string> Check(SpanTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is stale" };
        }
        return new List<string>();
    }
}

public sealed record SpanTierCodePrefixRule(string Prefix) : ISpanTierRule
{
    public List<string> Check(SpanTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SpanTierRequiredExternalRefRule(string Message) : ISpanTierRule
{
    public List<string> Check(SpanTier item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanTierRequiredNameRule(string Message) : ISpanTierRule
{
    public List<string> Check(SpanTier item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanTierQuantityRangeRule(int Min, int Max) : ISpanTierRule
{
    public List<string> Check(SpanTier item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span tier.</summary>
public sealed record SpanTierValidator(List<ISpanTierRule> Rules)
{
    public List<string> Validate(SpanTier item)
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

    public void RequireValid(SpanTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanTier> items)
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

/// <summary>Functions of span tier rules.</summary>
public static class SpanTierRules
{
    public static SpanTierValidator DefaultSpanTierValidator()
    {
        return new SpanTierValidator(
            new List<ISpanTierRule>
            {
                new SpanTierAllowedDepartmentRule(new List<string> { "adaptive", "pending", "batched" }),
                new SpanTierCodePrefixRule("OPS"),
                new SpanTierRequiredExternalRefRule("span tier needs this field"),
                new SpanTierRequiredNameRule("span tier needs this field"),
                new SpanTierQuantityRangeRule(3600, 86400),
            });
    }
}
