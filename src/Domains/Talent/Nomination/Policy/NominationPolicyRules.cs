namespace Acme.Workforce.Talent.Nomination.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a nomination policy must pass. Check returns its problems, or none.</summary>
public interface INominationPolicyRule
{
    List<string> Check(NominationPolicy item);
}

public sealed record NominationPolicyLabelLengthRule(int MaxLength) : INominationPolicyRule
{
    public List<string> Check(NominationPolicy item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationPolicyCodeLengthRule(int MaxLength) : INominationPolicyRule
{
    public List<string> Check(NominationPolicy item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationPolicyAllowedCodeRule(List<string> Allowed) : INominationPolicyRule
{
    public List<string> Check(NominationPolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record NominationPolicyJobCodeLengthRule(int MaxLength) : INominationPolicyRule
{
    public List<string> Check(NominationPolicy item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationPolicyCodePrefixRule(string Prefix) : INominationPolicyRule
{
    public List<string> Check(NominationPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination policy.</summary>
public sealed record NominationPolicyValidator(List<INominationPolicyRule> Rules)
{
    public List<string> Validate(NominationPolicy item)
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

    public void RequireValid(NominationPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationPolicy> items)
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

/// <summary>Functions of nomination policy rules.</summary>
public static class NominationPolicyRules
{
    public static NominationPolicyValidator DefaultNominationPolicyValidator()
    {
        return new NominationPolicyValidator(
            new List<INominationPolicyRule>
            {
                new NominationPolicyLabelLengthRule(16),
                new NominationPolicyCodeLengthRule(3600),
                new NominationPolicyAllowedCodeRule(new List<string> { "batched" }),
                new NominationPolicyJobCodeLengthRule(1000),
                new NominationPolicyCodePrefixRule("X"),
            });
    }
}
