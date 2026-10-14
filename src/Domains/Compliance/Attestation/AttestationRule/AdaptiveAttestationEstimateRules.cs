namespace Acme.Workforce.Compliance.Attestation.AttestationRule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive attestation estimate must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveAttestationEstimateRule
{
    List<string> Check(AdaptiveAttestationEstimate item);
}

public sealed record AdaptiveAttestationEstimateRequiredCodeRule(string Message) : IAdaptiveAttestationEstimateRule
{
    public List<string> Check(AdaptiveAttestationEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAttestationEstimateRequiredUnitRule(string Message) : IAdaptiveAttestationEstimateRule
{
    public List<string> Check(AdaptiveAttestationEstimate item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAttestationEstimateAllowedManagerIdRule(List<string> Allowed) : IAdaptiveAttestationEstimateRule
{
    public List<string> Check(AdaptiveAttestationEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ManagerId)))
        {
            return new List<string> { $"manager id {item.ManagerId} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAttestationEstimateVisibleGuardRule(double Limit) : IAdaptiveAttestationEstimateRule
{
    public List<string> Check(AdaptiveAttestationEstimate item)
    {
        if (item.Visible && item.Threshold > Limit)
        {
            return new List<string> { $"threshold {item.Threshold} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAttestationEstimateTitleLengthRule(int MaxLength) : IAdaptiveAttestationEstimateRule
{
    public List<string> Check(AdaptiveAttestationEstimate item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAttestationEstimateCodeLengthRule(int MaxLength) : IAdaptiveAttestationEstimateRule
{
    public List<string> Check(AdaptiveAttestationEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive attestation estimate.</summary>
public sealed record AdaptiveAttestationEstimateValidator(List<IAdaptiveAttestationEstimateRule> Rules)
{
    public List<string> Validate(AdaptiveAttestationEstimate item)
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

    public void RequireValid(AdaptiveAttestationEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveAttestationEstimate> items)
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

/// <summary>Functions of adaptive attestation estimate rules.</summary>
public static class AdaptiveAttestationEstimateRules
{
    public static AdaptiveAttestationEstimateValidator DefaultAdaptiveAttestationEstimateValidator()
    {
        return new AdaptiveAttestationEstimateValidator(
            new List<IAdaptiveAttestationEstimateRule>
            {
                new AdaptiveAttestationEstimateRequiredCodeRule("adaptive attestation estimate needs a value"),
                new AdaptiveAttestationEstimateRequiredUnitRule("adaptive attestation estimate needs an entry"),
                new AdaptiveAttestationEstimateAllowedManagerIdRule(new List<string>()),
                new AdaptiveAttestationEstimateVisibleGuardRule(2.6),
                new AdaptiveAttestationEstimateTitleLengthRule(80),
                new AdaptiveAttestationEstimateCodeLengthRule(80),
            });
    }
}
