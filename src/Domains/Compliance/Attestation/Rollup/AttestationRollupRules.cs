namespace Acme.Workforce.Compliance.Attestation.Rollup;

/// <summary>A rule a attestation rollup must pass. Check returns its problems, or none.</summary>
public interface IAttestationRollupRule
{
    List<string> Check(AttestationRollup item);
}

public sealed record AttestationRollupNameLengthRule(int MaxLength) : IAttestationRollupRule
{
    public List<string> Check(AttestationRollup item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationRollupMaximumRangeRule(double Min, double Max) : IAttestationRollupRule
{
    public List<string> Check(AttestationRollup item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AttestationRollupRequiredNameRule(string Message) : IAttestationRollupRule
{
    public List<string> Check(AttestationRollup item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationRollupCodePrefixRule(string Prefix) : IAttestationRollupRule
{
    public List<string> Check(AttestationRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationRollupContactEmailLengthRule(int MaxLength) : IAttestationRollupRule
{
    public List<string> Check(AttestationRollup item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationRollupRequiredCodeRule(string Message) : IAttestationRollupRule
{
    public List<string> Check(AttestationRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation rollup.</summary>
public sealed record AttestationRollupValidator(List<IAttestationRollupRule> Rules)
{
    public List<string> Validate(AttestationRollup item)
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

    public void RequireValid(AttestationRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationRollup> items)
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

/// <summary>Functions of attestation rollup rules.</summary>
public static class AttestationRollupRules
{
    public static AttestationRollupValidator DefaultAttestationRollupValidator()
    {
        return new AttestationRollupValidator(
            new List<IAttestationRollupRule>
            {
                new AttestationRollupNameLengthRule(3600),
                new AttestationRollupMaximumRangeRule(8.4, 2599.6),
                new AttestationRollupRequiredNameRule("attestation rollup needs this field"),
                new AttestationRollupCodePrefixRule("X"),
                new AttestationRollupContactEmailLengthRule(120),
                new AttestationRollupRequiredCodeRule("attestation rollup needs a value"),
            });
    }
}
