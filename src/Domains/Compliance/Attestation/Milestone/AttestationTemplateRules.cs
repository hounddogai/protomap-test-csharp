namespace Acme.Workforce.Compliance.Attestation.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a attestation template must pass. Check returns its problems, or none.</summary>
public interface IAttestationTemplateRule
{
    List<string> Check(AttestationTemplate item);
}

public sealed record AttestationTemplateAllowedCodeRule(List<string> Allowed) : IAttestationTemplateRule
{
    public List<string> Check(AttestationTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record AttestationTemplateCodePrefixRule(string Prefix) : IAttestationTemplateRule
{
    public List<string> Check(AttestationTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationTemplateRateRangeRule(double Min, double Max) : IAttestationTemplateRule
{
    public List<string> Check(AttestationTemplate item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AttestationTemplateRequiredTitleRule(string Message) : IAttestationTemplateRule
{
    public List<string> Check(AttestationTemplate item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation template.</summary>
public sealed record AttestationTemplateValidator(List<IAttestationTemplateRule> Rules)
{
    public List<string> Validate(AttestationTemplate item)
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

    public void RequireValid(AttestationTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationTemplate> items)
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

/// <summary>Functions of attestation template rules.</summary>
public static class AttestationTemplateRules
{
    public static AttestationTemplateValidator DefaultAttestationTemplateValidator()
    {
        return new AttestationTemplateValidator(
            new List<IAttestationTemplateRule>
            {
                new AttestationTemplateAllowedCodeRule(new List<string>()),
                new AttestationTemplateCodePrefixRule("REF"),
                new AttestationTemplateRateRangeRule(8.8, 3464.7),
                new AttestationTemplateRequiredTitleRule("attestation template needs this field"),
            });
    }
}
