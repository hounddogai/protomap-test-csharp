namespace Acme.Workforce.Absence.Policy.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy rollup must pass. Check returns its problems, or none.</summary>
public interface IPolicyRollupRule
{
    List<string> Check(PolicyRollup item);
}

public sealed record PolicyRollupPhoneLengthRule(int MaxLength) : IPolicyRollupRule
{
    public List<string> Check(PolicyRollup item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyRollupAllowedCodeRule(List<string> Allowed) : IPolicyRollupRule
{
    public List<string> Check(PolicyRollup item)
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

public sealed record PolicyRollupRequiredCodeRule(string Message) : IPolicyRollupRule
{
    public List<string> Check(PolicyRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy rollup.</summary>
public sealed record PolicyRollupValidator(List<IPolicyRollupRule> Rules)
{
    public List<string> Validate(PolicyRollup item)
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

    public void RequireValid(PolicyRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyRollup> items)
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

/// <summary>Functions of policy rollup rules.</summary>
public static class PolicyRollupRules
{
    public static PolicyRollupValidator DefaultPolicyRollupValidator()
    {
        return new PolicyRollupValidator(
            new List<IPolicyRollupRule>
            {
                new PolicyRollupPhoneLengthRule(86400),
                new PolicyRollupAllowedCodeRule(new List<string> { "primary", "fallback", "lenient" }),
                new PolicyRollupRequiredCodeRule("policy rollup needs a value"),
            });
    }
}
