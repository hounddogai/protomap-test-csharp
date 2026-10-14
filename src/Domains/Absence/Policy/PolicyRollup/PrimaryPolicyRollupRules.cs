namespace Acme.Workforce.Absence.Policy.PolicyRollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary policy rollup must pass. Check returns its problems, or none.</summary>
public interface IPrimaryPolicyRollupRule
{
    List<string> Check(PrimaryPolicyRollup item);
}

public sealed record PrimaryPolicyRollupCodePrefixRule(string Prefix) : IPrimaryPolicyRollupRule
{
    public List<string> Check(PrimaryPolicyRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyRollupAllowedSkillRule(List<string> Allowed) : IPrimaryPolicyRollupRule
{
    public List<string> Check(PrimaryPolicyRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyRollupRequiredCodeRule(string Message) : IPrimaryPolicyRollupRule
{
    public List<string> Check(PrimaryPolicyRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyRollupLabelLengthRule(int MaxLength) : IPrimaryPolicyRollupRule
{
    public List<string> Check(PrimaryPolicyRollup item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyRollupRequiredLabelRule(string Message) : IPrimaryPolicyRollupRule
{
    public List<string> Check(PrimaryPolicyRollup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary policy rollup.</summary>
public sealed record PrimaryPolicyRollupValidator(List<IPrimaryPolicyRollupRule> Rules)
{
    public List<string> Validate(PrimaryPolicyRollup item)
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

    public void RequireValid(PrimaryPolicyRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryPolicyRollup> items)
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

/// <summary>Functions of primary policy rollup rules.</summary>
public static class PrimaryPolicyRollupRules
{
    public static PrimaryPolicyRollupValidator DefaultPrimaryPolicyRollupValidator()
    {
        return new PrimaryPolicyRollupValidator(
            new List<IPrimaryPolicyRollupRule>
            {
                new PrimaryPolicyRollupCodePrefixRule("OPS"),
                new PrimaryPolicyRollupAllowedSkillRule(new List<string> { "lenient", "fallback", "scheduled" }),
                new PrimaryPolicyRollupRequiredCodeRule("primary policy rollup needs a value"),
                new PrimaryPolicyRollupLabelLengthRule(32),
                new PrimaryPolicyRollupRequiredLabelRule("primary policy rollup needs an entry"),
            });
    }
}
