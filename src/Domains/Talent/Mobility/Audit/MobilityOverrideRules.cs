namespace Acme.Workforce.Talent.Mobility.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a mobility override must pass. Check returns its problems, or none.</summary>
public interface IMobilityOverrideRule
{
    List<string> Check(MobilityOverride item);
}

public sealed record MobilityOverrideCodeLengthRule(int MaxLength) : IMobilityOverrideRule
{
    public List<string> Check(MobilityOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityOverrideAllowedReviewerRule(List<string> Allowed) : IMobilityOverrideRule
{
    public List<string> Check(MobilityOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Reviewer)))
        {
            return new List<string> { $"reviewer {item.Reviewer} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record MobilityOverrideRequiredCurrencyRule(string Message) : IMobilityOverrideRule
{
    public List<string> Check(MobilityOverride item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityOverrideRequiredNameRule(string Message) : IMobilityOverrideRule
{
    public List<string> Check(MobilityOverride item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityOverrideCodePrefixRule(string Prefix) : IMobilityOverrideRule
{
    public List<string> Check(MobilityOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility override.</summary>
public sealed record MobilityOverrideValidator(List<IMobilityOverrideRule> Rules)
{
    public List<string> Validate(MobilityOverride item)
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

    public void RequireValid(MobilityOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityOverride> items)
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

/// <summary>Functions of mobility override rules.</summary>
public static class MobilityOverrideRules
{
    public static MobilityOverrideValidator DefaultMobilityOverrideValidator()
    {
        return new MobilityOverrideValidator(
            new List<IMobilityOverrideRule>
            {
                new MobilityOverrideCodeLengthRule(32),
                new MobilityOverrideAllowedReviewerRule(new List<string> { "adaptive", "global", "capped" }),
                new MobilityOverrideRequiredCurrencyRule("mobility override needs a value"),
                new MobilityOverrideRequiredNameRule("mobility override needs a value"),
                new MobilityOverrideCodePrefixRule("ACM"),
            });
    }
}
