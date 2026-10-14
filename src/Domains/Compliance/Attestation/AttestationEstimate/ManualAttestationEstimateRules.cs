namespace Acme.Workforce.Compliance.Attestation.AttestationEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual attestation estimate must pass. Check returns its problems, or none.</summary>
public interface IManualAttestationEstimateRule
{
    List<string> Check(ManualAttestationEstimate item);
}

public sealed record ManualAttestationEstimateAllowedEmployeeIdRule(List<string> Allowed) : IManualAttestationEstimateRule
{
    public List<string> Check(ManualAttestationEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.EmployeeId)))
        {
            return new List<string> { $"employee id {item.EmployeeId} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record ManualAttestationEstimateRequiredLabelRule(string Message) : IManualAttestationEstimateRule
{
    public List<string> Check(ManualAttestationEstimate item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualAttestationEstimateEmployeeIdLengthRule(int MaxLength) : IManualAttestationEstimateRule
{
    public List<string> Check(ManualAttestationEstimate item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAttestationEstimateCountryLengthRule(int MaxLength) : IManualAttestationEstimateRule
{
    public List<string> Check(ManualAttestationEstimate item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAttestationEstimateRequiredEmployeeIdRule(string Message) : IManualAttestationEstimateRule
{
    public List<string> Check(ManualAttestationEstimate item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualAttestationEstimateCodePrefixRule(string Prefix) : IManualAttestationEstimateRule
{
    public List<string> Check(ManualAttestationEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual attestation estimate.</summary>
public sealed record ManualAttestationEstimateValidator(List<IManualAttestationEstimateRule> Rules)
{
    public List<string> Validate(ManualAttestationEstimate item)
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

    public void RequireValid(ManualAttestationEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualAttestationEstimate> items)
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

/// <summary>Functions of manual attestation estimate rules.</summary>
public static class ManualAttestationEstimateRules
{
    public static ManualAttestationEstimateValidator DefaultManualAttestationEstimateValidator()
    {
        return new ManualAttestationEstimateValidator(
            new List<IManualAttestationEstimateRule>
            {
                new ManualAttestationEstimateAllowedEmployeeIdRule(new List<string>()),
                new ManualAttestationEstimateRequiredLabelRule("manual attestation estimate needs this field"),
                new ManualAttestationEstimateEmployeeIdLengthRule(80),
                new ManualAttestationEstimateCountryLengthRule(86400),
                new ManualAttestationEstimateRequiredEmployeeIdRule("manual attestation estimate needs a value"),
                new ManualAttestationEstimateCodePrefixRule("ACM"),
            });
    }
}
