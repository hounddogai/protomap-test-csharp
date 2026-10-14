namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive equity grant override must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveEquityGrantOverrideRule
{
    List<string> Check(AdaptiveEquityGrantOverride item);
}

public sealed record AdaptiveEquityGrantOverrideAllowedCodeRule(List<string> Allowed) : IAdaptiveEquityGrantOverrideRule
{
    public List<string> Check(AdaptiveEquityGrantOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveEquityGrantOverrideContactEmailLengthRule(int MaxLength) : IAdaptiveEquityGrantOverrideRule
{
    public List<string> Check(AdaptiveEquityGrantOverride item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveEquityGrantOverrideSequenceRangeRule(int Min, int Max) : IAdaptiveEquityGrantOverrideRule
{
    public List<string> Check(AdaptiveEquityGrantOverride item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveEquityGrantOverrideCodeLengthRule(int MaxLength) : IAdaptiveEquityGrantOverrideRule
{
    public List<string> Check(AdaptiveEquityGrantOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveEquityGrantOverrideCodePrefixRule(string Prefix) : IAdaptiveEquityGrantOverrideRule
{
    public List<string> Check(AdaptiveEquityGrantOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveEquityGrantOverrideRequiredSegmentRule(string Message) : IAdaptiveEquityGrantOverrideRule
{
    public List<string> Check(AdaptiveEquityGrantOverride item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive equity grant override.</summary>
public sealed record AdaptiveEquityGrantOverrideValidator(List<IAdaptiveEquityGrantOverrideRule> Rules)
{
    public List<string> Validate(AdaptiveEquityGrantOverride item)
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

    public void RequireValid(AdaptiveEquityGrantOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveEquityGrantOverride> items)
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

/// <summary>Functions of adaptive equity grant override rules.</summary>
public static class AdaptiveEquityGrantOverrideRules
{
    public static AdaptiveEquityGrantOverrideValidator DefaultAdaptiveEquityGrantOverrideValidator()
    {
        return new AdaptiveEquityGrantOverrideValidator(
            new List<IAdaptiveEquityGrantOverrideRule>
            {
                new AdaptiveEquityGrantOverrideAllowedCodeRule(
                    new List<string> { "fallback", "tiered", "scheduled" }),
                new AdaptiveEquityGrantOverrideContactEmailLengthRule(80),
                new AdaptiveEquityGrantOverrideSequenceRangeRule(255, 3600),
                new AdaptiveEquityGrantOverrideCodeLengthRule(255),
                new AdaptiveEquityGrantOverrideCodePrefixRule("ACM"),
                new AdaptiveEquityGrantOverrideRequiredSegmentRule("adaptive equity grant override needs an entry"),
            });
    }
}
