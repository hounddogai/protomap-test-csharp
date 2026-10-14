namespace Acme.Workforce.Compliance.Attestation.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending attestation rule must pass. Check returns its problems, or none.</summary>
public interface IPendingAttestationRuleRule
{
    List<string> Check(PendingAttestationRule item);
}

public sealed record PendingAttestationRuleRequiredCodeRule(string Message) : IPendingAttestationRuleRule
{
    public List<string> Check(PendingAttestationRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingAttestationRuleVisibleGuardRule(double Limit) : IPendingAttestationRuleRule
{
    public List<string> Check(PendingAttestationRule item)
    {
        if (item.Visible && item.Rate > Limit)
        {
            return new List<string> { $"rate {item.Rate} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record PendingAttestationRuleCodeLengthRule(int MaxLength) : IPendingAttestationRuleRule
{
    public List<string> Check(PendingAttestationRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingAttestationRuleAllowedTitleRule(List<string> Allowed) : IPendingAttestationRuleRule
{
    public List<string> Check(PendingAttestationRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record PendingAttestationRuleLabelLengthRule(int MaxLength) : IPendingAttestationRuleRule
{
    public List<string> Check(PendingAttestationRule item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending attestation rule.</summary>
public sealed record PendingAttestationRuleValidator(List<IPendingAttestationRuleRule> Rules)
{
    public List<string> Validate(PendingAttestationRule item)
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

    public void RequireValid(PendingAttestationRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingAttestationRule> items)
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

/// <summary>Functions of pending attestation rule rules.</summary>
public static class PendingAttestationRuleRules
{
    public static PendingAttestationRuleValidator DefaultPendingAttestationRuleValidator()
    {
        return new PendingAttestationRuleValidator(
            new List<IPendingAttestationRuleRule>
            {
                new PendingAttestationRuleRequiredCodeRule("pending attestation rule needs an entry"),
                new PendingAttestationRuleVisibleGuardRule(7.6),
                new PendingAttestationRuleCodeLengthRule(16),
                new PendingAttestationRuleAllowedTitleRule(new List<string> { "pending", "primary" }),
                new PendingAttestationRuleLabelLengthRule(80),
            });
    }
}
