namespace Acme.Workforce.Absence.Policy.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy quota must pass. Check returns its problems, or none.</summary>
public interface IPolicyQuotaRule
{
    List<string> Check(PolicyQuota item);
}

public sealed record PolicyQuotaRequiredSummaryRule(string Message) : IPolicyQuotaRule
{
    public List<string> Check(PolicyQuota item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyQuotaCodeLengthRule(int MaxLength) : IPolicyQuotaRule
{
    public List<string> Check(PolicyQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyQuotaAllowedCodeRule(List<string> Allowed) : IPolicyQuotaRule
{
    public List<string> Check(PolicyQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy quota.</summary>
public sealed record PolicyQuotaValidator(List<IPolicyQuotaRule> Rules)
{
    public List<string> Validate(PolicyQuota item)
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

    public void RequireValid(PolicyQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyQuota> items)
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

/// <summary>Functions of policy quota rules.</summary>
public static class PolicyQuotaRules
{
    public static PolicyQuotaValidator DefaultPolicyQuotaValidator()
    {
        return new PolicyQuotaValidator(
            new List<IPolicyQuotaRule>
            {
                new PolicyQuotaRequiredSummaryRule("policy quota needs this field"),
                new PolicyQuotaCodeLengthRule(1000),
                new PolicyQuotaAllowedCodeRule(new List<string> { "fallback", "manual", "rolling" }),
            });
    }
}
