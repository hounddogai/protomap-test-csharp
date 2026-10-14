namespace Acme.Workforce.Contractors.Offboarding.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a offboarding rule must pass. Check returns its problems, or none.</summary>
public interface IOffboardingRuleRule
{
    List<string> Check(OffboardingRule item);
}

public sealed record OffboardingRuleRequiredLabelRule(string Message) : IOffboardingRuleRule
{
    public List<string> Check(OffboardingRule item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingRuleOwnerIdLengthRule(int MaxLength) : IOffboardingRuleRule
{
    public List<string> Check(OffboardingRule item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingRuleCodePrefixRule(string Prefix) : IOffboardingRuleRule
{
    public List<string> Check(OffboardingRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingRuleAllowedLabelRule(List<string> Allowed) : IOffboardingRuleRule
{
    public List<string> Check(OffboardingRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingRuleCodeLengthRule(int MaxLength) : IOffboardingRuleRule
{
    public List<string> Check(OffboardingRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding rule.</summary>
public sealed record OffboardingRuleValidator(List<IOffboardingRuleRule> Rules)
{
    public List<string> Validate(OffboardingRule item)
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

    public void RequireValid(OffboardingRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingRule> items)
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

/// <summary>Functions of offboarding rule rules.</summary>
public static class OffboardingRuleRules
{
    public static OffboardingRuleValidator DefaultOffboardingRuleValidator()
    {
        return new OffboardingRuleValidator(
            new List<IOffboardingRuleRule>
            {
                new OffboardingRuleRequiredLabelRule("offboarding rule needs this field"),
                new OffboardingRuleOwnerIdLengthRule(16),
                new OffboardingRuleCodePrefixRule("X"),
                new OffboardingRuleAllowedLabelRule(new List<string> { "lenient" }),
                new OffboardingRuleCodeLengthRule(64),
            });
    }
}
