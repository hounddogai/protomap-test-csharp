namespace Acme.Workforce.Compliance.Attestation.Claim;

/// <summary>A rule a attestation manifest must pass. Check returns its problems, or none.</summary>
public interface IAttestationManifestRule
{
    List<string> Check(AttestationManifest item);
}

public sealed record AttestationManifestHoursRangeRule(double Min, double Max) : IAttestationManifestRule
{
    public List<string> Check(AttestationManifest item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationManifestRequiredCodeRule(string Message) : IAttestationManifestRule
{
    public List<string> Check(AttestationManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationManifestVersionRangeRule(int Min, int Max) : IAttestationManifestRule
{
    public List<string> Check(AttestationManifest item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AttestationManifestRequiredLabelRule(string Message) : IAttestationManifestRule
{
    public List<string> Check(AttestationManifest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation manifest.</summary>
public sealed record AttestationManifestValidator(List<IAttestationManifestRule> Rules)
{
    public List<string> Validate(AttestationManifest item)
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

    public void RequireValid(AttestationManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationManifest> items)
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

/// <summary>Functions of attestation manifest rules.</summary>
public static class AttestationManifestRules
{
    public static AttestationManifestValidator DefaultAttestationManifestValidator()
    {
        return new AttestationManifestValidator(
            new List<IAttestationManifestRule>
            {
                new AttestationManifestHoursRangeRule(5.3, 2194.9),
                new AttestationManifestRequiredCodeRule("attestation manifest needs this field"),
                new AttestationManifestVersionRangeRule(255, 1000),
                new AttestationManifestRequiredLabelRule("attestation manifest needs a value"),
            });
    }
}
