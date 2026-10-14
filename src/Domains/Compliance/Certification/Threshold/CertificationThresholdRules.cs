namespace Acme.Workforce.Compliance.Certification.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a certification threshold must pass. Check returns its problems, or none.</summary>
public interface ICertificationThresholdRule
{
    List<string> Check(CertificationThreshold item);
}

public sealed record CertificationThresholdApprovedGuardRule(double Limit) : ICertificationThresholdRule
{
    public List<string> Check(CertificationThreshold item)
    {
        if (item.Approved && item.Fte > Limit)
        {
            return new List<string> { $"fte {item.Fte} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record CertificationThresholdRequiredCostCenterRule(string Message) : ICertificationThresholdRule
{
    public List<string> Check(CertificationThreshold item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationThresholdCodePrefixRule(string Prefix) : ICertificationThresholdRule
{
    public List<string> Check(CertificationThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationThresholdRequiredPhoneRule(string Message) : ICertificationThresholdRule
{
    public List<string> Check(CertificationThreshold item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationThresholdAllowedCodeRule(List<string> Allowed) : ICertificationThresholdRule
{
    public List<string> Check(CertificationThreshold item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification threshold.</summary>
public sealed record CertificationThresholdValidator(List<ICertificationThresholdRule> Rules)
{
    public List<string> Validate(CertificationThreshold item)
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

    public void RequireValid(CertificationThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationThreshold> items)
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

/// <summary>Functions of certification threshold rules.</summary>
public static class CertificationThresholdRules
{
    public static CertificationThresholdValidator DefaultCertificationThresholdValidator()
    {
        return new CertificationThresholdValidator(
            new List<ICertificationThresholdRule>
            {
                new CertificationThresholdApprovedGuardRule(1.6),
                new CertificationThresholdRequiredCostCenterRule("certification threshold needs a value"),
                new CertificationThresholdCodePrefixRule("ACM"),
                new CertificationThresholdRequiredPhoneRule("certification threshold needs an entry"),
                new CertificationThresholdAllowedCodeRule(new List<string> { "batched" }),
            });
    }
}
