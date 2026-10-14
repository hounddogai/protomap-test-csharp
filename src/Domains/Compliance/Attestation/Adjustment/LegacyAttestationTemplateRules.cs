namespace Acme.Workforce.Compliance.Attestation.Adjustment;

/// <summary>A rule a legacy attestation template must pass. Check returns its problems, or none.</summary>
public interface ILegacyAttestationTemplateRule
{
    List<string> Check(LegacyAttestationTemplate item);
}

public sealed record LegacyAttestationTemplateCodePrefixRule(string Prefix) : ILegacyAttestationTemplateRule
{
    public List<string> Check(LegacyAttestationTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyAttestationTemplateRequiredCodeRule(string Message) : ILegacyAttestationTemplateRule
{
    public List<string> Check(LegacyAttestationTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyAttestationTemplateLabelLengthRule(int MaxLength) : ILegacyAttestationTemplateRule
{
    public List<string> Check(LegacyAttestationTemplate item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy attestation template.</summary>
public sealed record LegacyAttestationTemplateValidator(List<ILegacyAttestationTemplateRule> Rules)
{
    public List<string> Validate(LegacyAttestationTemplate item)
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

    public void RequireValid(LegacyAttestationTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyAttestationTemplate> items)
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

/// <summary>Functions of legacy attestation template rules.</summary>
public static class LegacyAttestationTemplateRules
{
    public static LegacyAttestationTemplateValidator DefaultLegacyAttestationTemplateValidator()
    {
        return new LegacyAttestationTemplateValidator(
            new List<ILegacyAttestationTemplateRule>
            {
                new LegacyAttestationTemplateCodePrefixRule("ACM"),
                new LegacyAttestationTemplateRequiredCodeRule("legacy attestation template needs a value"),
                new LegacyAttestationTemplateLabelLengthRule(16),
            });
    }
}
