namespace Acme.Workforce.Compliance.Certification.Template;

/// <summary>A rule a certification template must pass. Check returns its problems, or none.</summary>
public interface ICertificationTemplateRule
{
    List<string> Check(CertificationTemplate item);
}

public sealed record CertificationTemplateHoursRangeRule(double Min, double Max) : ICertificationTemplateRule
{
    public List<string> Check(CertificationTemplate item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CertificationTemplateRequiredCodeRule(string Message) : ICertificationTemplateRule
{
    public List<string> Check(CertificationTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationTemplateRequiredLabelRule(string Message) : ICertificationTemplateRule
{
    public List<string> Check(CertificationTemplate item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationTemplateManagerIdLengthRule(int MaxLength) : ICertificationTemplateRule
{
    public List<string> Check(CertificationTemplate item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationTemplateCodePrefixRule(string Prefix) : ICertificationTemplateRule
{
    public List<string> Check(CertificationTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification template.</summary>
public sealed record CertificationTemplateValidator(List<ICertificationTemplateRule> Rules)
{
    public List<string> Validate(CertificationTemplate item)
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

    public void RequireValid(CertificationTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationTemplate> items)
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

/// <summary>Functions of certification template rules.</summary>
public static class CertificationTemplateRules
{
    public static CertificationTemplateValidator DefaultCertificationTemplateValidator()
    {
        return new CertificationTemplateValidator(
            new List<ICertificationTemplateRule>
            {
                new CertificationTemplateHoursRangeRule(2.0, 861.4),
                new CertificationTemplateRequiredCodeRule("certification template needs this field"),
                new CertificationTemplateRequiredLabelRule("certification template needs an entry"),
                new CertificationTemplateManagerIdLengthRule(3600),
                new CertificationTemplateCodePrefixRule("ACM"),
            });
    }
}
