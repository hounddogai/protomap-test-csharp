namespace Acme.Workforce.Talent.Mobility.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy mobility rule must pass. Check returns its problems, or none.</summary>
public interface ILegacyMobilityRuleRule
{
    List<string> Check(LegacyMobilityRule item);
}

public sealed record LegacyMobilityRuleAllowedCodeRule(List<string> Allowed) : ILegacyMobilityRuleRule
{
    public List<string> Check(LegacyMobilityRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is stale" };
        }
        return new List<string>();
    }
}

public sealed record LegacyMobilityRuleContactEmailLengthRule(int MaxLength) : ILegacyMobilityRuleRule
{
    public List<string> Check(LegacyMobilityRule item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyMobilityRuleTitleLengthRule(int MaxLength) : ILegacyMobilityRuleRule
{
    public List<string> Check(LegacyMobilityRule item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyMobilityRuleCodePrefixRule(string Prefix) : ILegacyMobilityRuleRule
{
    public List<string> Check(LegacyMobilityRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy mobility rule.</summary>
public sealed record LegacyMobilityRuleValidator(List<ILegacyMobilityRuleRule> Rules)
{
    public List<string> Validate(LegacyMobilityRule item)
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

    public void RequireValid(LegacyMobilityRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyMobilityRule> items)
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

/// <summary>Functions of legacy mobility rule rules.</summary>
public static class LegacyMobilityRuleRules
{
    public static LegacyMobilityRuleValidator DefaultLegacyMobilityRuleValidator()
    {
        return new LegacyMobilityRuleValidator(
            new List<ILegacyMobilityRuleRule>
            {
                new LegacyMobilityRuleAllowedCodeRule(new List<string> { "global" }),
                new LegacyMobilityRuleContactEmailLengthRule(255),
                new LegacyMobilityRuleTitleLengthRule(1000),
                new LegacyMobilityRuleCodePrefixRule("Q"),
            });
    }
}
