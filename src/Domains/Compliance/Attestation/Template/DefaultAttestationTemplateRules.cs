namespace Acme.Workforce.Compliance.Attestation.Template;

/// <summary>A rule a default attestation template must pass. Check returns its problems, or none.</summary>
public interface IDefaultAttestationTemplateRule
{
    List<string> Check(DefaultAttestationTemplate item);
}

public sealed record DefaultAttestationTemplateActiveGuardRule(double Limit) : IDefaultAttestationTemplateRule
{
    public List<string> Check(DefaultAttestationTemplate item)
    {
        if (item.Active && item.Hours > Limit)
        {
            return new List<string> { $"hours {item.Hours} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAttestationTemplateRequiredCurrencyRule(string Message) : IDefaultAttestationTemplateRule
{
    public List<string> Check(DefaultAttestationTemplate item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultAttestationTemplateCodeLengthRule(int MaxLength) : IDefaultAttestationTemplateRule
{
    public List<string> Check(DefaultAttestationTemplate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAttestationTemplateCurrencyLengthRule(int MaxLength) : IDefaultAttestationTemplateRule
{
    public List<string> Check(DefaultAttestationTemplate item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default attestation template.</summary>
public sealed record DefaultAttestationTemplateValidator(List<IDefaultAttestationTemplateRule> Rules)
{
    public List<string> Validate(DefaultAttestationTemplate item)
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

    public void RequireValid(DefaultAttestationTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultAttestationTemplate> items)
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

/// <summary>Functions of default attestation template rules.</summary>
public static class DefaultAttestationTemplateRules
{
    public static DefaultAttestationTemplateValidator DefaultDefaultAttestationTemplateValidator()
    {
        return new DefaultAttestationTemplateValidator(
            new List<IDefaultAttestationTemplateRule>
            {
                new DefaultAttestationTemplateActiveGuardRule(10.0),
                new DefaultAttestationTemplateRequiredCurrencyRule("default attestation template needs an entry"),
                new DefaultAttestationTemplateCodeLengthRule(64),
                new DefaultAttestationTemplateCurrencyLengthRule(32),
            });
    }
}
