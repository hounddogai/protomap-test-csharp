namespace Acme.Workforce.Compliance.Attestation.Estimate;

/// <summary>A rule a attestation bundle must pass. Check returns its problems, or none.</summary>
public interface IAttestationBundleRule
{
    List<string> Check(AttestationBundle item);
}

public sealed record AttestationBundleCodePrefixRule(string Prefix) : IAttestationBundleRule
{
    public List<string> Check(AttestationBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationBundleRequiredTitleRule(string Message) : IAttestationBundleRule
{
    public List<string> Check(AttestationBundle item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationBundleRequiredNameRule(string Message) : IAttestationBundleRule
{
    public List<string> Check(AttestationBundle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationBundleMaximumRangeRule(double Min, double Max) : IAttestationBundleRule
{
    public List<string> Check(AttestationBundle item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationBundleRequiredUnitRule(string Message) : IAttestationBundleRule
{
    public List<string> Check(AttestationBundle item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationBundleCapacityRangeRule(int Min, int Max) : IAttestationBundleRule
{
    public List<string> Check(AttestationBundle item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a attestation bundle.</summary>
public sealed record AttestationBundleValidator(List<IAttestationBundleRule> Rules)
{
    public List<string> Validate(AttestationBundle item)
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

    public void RequireValid(AttestationBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationBundle> items)
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

/// <summary>Functions of attestation bundle rules.</summary>
public static class AttestationBundleRules
{
    public static AttestationBundleValidator DefaultAttestationBundleValidator()
    {
        return new AttestationBundleValidator(
            new List<IAttestationBundleRule>
            {
                new AttestationBundleCodePrefixRule("X"),
                new AttestationBundleRequiredTitleRule("attestation bundle needs a value"),
                new AttestationBundleRequiredNameRule("attestation bundle needs a value"),
                new AttestationBundleMaximumRangeRule(8.7, 3920.4),
                new AttestationBundleRequiredUnitRule("attestation bundle needs this field"),
                new AttestationBundleCapacityRangeRule(80, 86400),
            });
    }
}
