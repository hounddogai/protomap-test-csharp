namespace Acme.Workforce.Compliance.Attestation.Estimate;

/// <summary>A rule a attestation estimate must pass. Check returns its problems, or none.</summary>
public interface IAttestationEstimateRule
{
    List<string> Check(AttestationEstimate item);
}

public sealed record AttestationEstimateExpiresAtRangeRule(int Min, int Max) : IAttestationEstimateRule
{
    public List<string> Check(AttestationEstimate item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AttestationEstimateCurrencyLengthRule(int MaxLength) : IAttestationEstimateRule
{
    public List<string> Check(AttestationEstimate item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationEstimateContactEmailLengthRule(int MaxLength) : IAttestationEstimateRule
{
    public List<string> Check(AttestationEstimate item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation estimate.</summary>
public sealed record AttestationEstimateValidator(List<IAttestationEstimateRule> Rules)
{
    public List<string> Validate(AttestationEstimate item)
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

    public void RequireValid(AttestationEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationEstimate> items)
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

/// <summary>Functions of attestation estimate rules.</summary>
public static class AttestationEstimateRules
{
    public static AttestationEstimateValidator DefaultAttestationEstimateValidator()
    {
        return new AttestationEstimateValidator(
            new List<IAttestationEstimateRule>
            {
                new AttestationEstimateExpiresAtRangeRule(120, 86400),
                new AttestationEstimateCurrencyLengthRule(32),
                new AttestationEstimateContactEmailLengthRule(255),
            });
    }
}
