namespace Acme.Workforce.Orgdesign.Reorg.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a reorg policy must pass. Check returns its problems, or none.</summary>
public interface IReorgPolicyRule
{
    List<string> Check(ReorgPolicy item);
}

public sealed record ReorgPolicyCodePrefixRule(string Prefix) : IReorgPolicyRule
{
    public List<string> Check(ReorgPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgPolicyGradeLengthRule(int MaxLength) : IReorgPolicyRule
{
    public List<string> Check(ReorgPolicy item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgPolicyAllowedGradeRule(List<string> Allowed) : IReorgPolicyRule
{
    public List<string> Check(ReorgPolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record ReorgPolicyLabelLengthRule(int MaxLength) : IReorgPolicyRule
{
    public List<string> Check(ReorgPolicy item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgPolicyRequiredExternalRefRule(string Message) : IReorgPolicyRule
{
    public List<string> Check(ReorgPolicy item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg policy.</summary>
public sealed record ReorgPolicyValidator(List<IReorgPolicyRule> Rules)
{
    public List<string> Validate(ReorgPolicy item)
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

    public void RequireValid(ReorgPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgPolicy> items)
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

/// <summary>Functions of reorg policy rules.</summary>
public static class ReorgPolicyRules
{
    public static ReorgPolicyValidator DefaultReorgPolicyValidator()
    {
        return new ReorgPolicyValidator(
            new List<IReorgPolicyRule>
            {
                new ReorgPolicyCodePrefixRule("OPS"),
                new ReorgPolicyGradeLengthRule(1000),
                new ReorgPolicyAllowedGradeRule(new List<string> { "default", "capped" }),
                new ReorgPolicyLabelLengthRule(16),
                new ReorgPolicyRequiredExternalRefRule("reorg policy needs an entry"),
            });
    }
}
