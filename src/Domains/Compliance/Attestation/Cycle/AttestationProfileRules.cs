namespace Acme.Workforce.Compliance.Attestation.Cycle;

/// <summary>A rule a attestation profile must pass. Check returns its problems, or none.</summary>
public interface IAttestationProfileRule
{
    List<string> Check(AttestationProfile item);
}

public sealed record AttestationProfileCodePrefixRule(string Prefix) : IAttestationProfileRule
{
    public List<string> Check(AttestationProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationProfileArchivedGuardRule(int Limit) : IAttestationProfileRule
{
    public List<string> Check(AttestationProfile item)
    {
        if (item.Archived && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record AttestationProfileRequiredLabelRule(string Message) : IAttestationProfileRule
{
    public List<string> Check(AttestationProfile item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationProfileQuantityRangeRule(int Min, int Max) : IAttestationProfileRule
{
    public List<string> Check(AttestationProfile item)
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

public sealed record AttestationProfileRequiredCountryRule(string Message) : IAttestationProfileRule
{
    public List<string> Check(AttestationProfile item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation profile.</summary>
public sealed record AttestationProfileValidator(List<IAttestationProfileRule> Rules)
{
    public List<string> Validate(AttestationProfile item)
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

    public void RequireValid(AttestationProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationProfile> items)
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

/// <summary>Functions of attestation profile rules.</summary>
public static class AttestationProfileRules
{
    public static AttestationProfileValidator DefaultAttestationProfileValidator()
    {
        return new AttestationProfileValidator(
            new List<IAttestationProfileRule>
            {
                new AttestationProfileCodePrefixRule("REF"),
                new AttestationProfileArchivedGuardRule(255),
                new AttestationProfileRequiredLabelRule("attestation profile needs an entry"),
                new AttestationProfileQuantityRangeRule(80, 255),
                new AttestationProfileRequiredCountryRule("attestation profile needs this field"),
            });
    }
}
