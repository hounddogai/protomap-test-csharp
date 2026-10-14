namespace Acme.Workforce.Compliance.Attestation.Policy;

/// <summary>A rule a attestation preference must pass. Check returns its problems, or none.</summary>
public interface IAttestationPreferenceRule
{
    List<string> Check(AttestationPreference item);
}

public sealed record AttestationPreferenceQuantityRangeRule(int Min, int Max) : IAttestationPreferenceRule
{
    public List<string> Check(AttestationPreference item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AttestationPreferenceRequiredCurrencyRule(string Message) : IAttestationPreferenceRule
{
    public List<string> Check(AttestationPreference item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationPreferenceExpiresAtRangeRule(int Min, int Max) : IAttestationPreferenceRule
{
    public List<string> Check(AttestationPreference item)
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

/// <summary>Runs every rule of a attestation preference.</summary>
public sealed record AttestationPreferenceValidator(List<IAttestationPreferenceRule> Rules)
{
    public List<string> Validate(AttestationPreference item)
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

    public void RequireValid(AttestationPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationPreference> items)
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

/// <summary>Functions of attestation preference rules.</summary>
public static class AttestationPreferenceRules
{
    public static AttestationPreferenceValidator DefaultAttestationPreferenceValidator()
    {
        return new AttestationPreferenceValidator(
            new List<IAttestationPreferenceRule>
            {
                new AttestationPreferenceQuantityRangeRule(1000, 3600),
                new AttestationPreferenceRequiredCurrencyRule("attestation preference needs this field"),
                new AttestationPreferenceExpiresAtRangeRule(255, 86400),
            });
    }
}
