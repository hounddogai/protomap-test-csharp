namespace Acme.Workforce.Compliance.Certification.CertificationForecast;

/// <summary>A rule a capped certification template must pass. Check returns its problems, or none.</summary>
public interface ICappedCertificationTemplateRule
{
    List<string> Check(CappedCertificationTemplate item);
}

public sealed record CappedCertificationTemplateCodePrefixRule(string Prefix) : ICappedCertificationTemplateRule
{
    public List<string> Check(CappedCertificationTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedCertificationTemplateCodeLengthRule(int MaxLength) : ICappedCertificationTemplateRule
{
    public List<string> Check(CappedCertificationTemplate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedCertificationTemplateGraceDaysRangeRule(int Min, int Max) : ICappedCertificationTemplateRule
{
    public List<string> Check(CappedCertificationTemplate item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a capped certification template.</summary>
public sealed record CappedCertificationTemplateValidator(List<ICappedCertificationTemplateRule> Rules)
{
    public List<string> Validate(CappedCertificationTemplate item)
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

    public void RequireValid(CappedCertificationTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedCertificationTemplate> items)
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

/// <summary>Functions of capped certification template rules.</summary>
public static class CappedCertificationTemplateRules
{
    public static CappedCertificationTemplateValidator DefaultCappedCertificationTemplateValidator()
    {
        return new CappedCertificationTemplateValidator(
            new List<ICappedCertificationTemplateRule>
            {
                new CappedCertificationTemplateCodePrefixRule("OPS"),
                new CappedCertificationTemplateCodeLengthRule(120),
                new CappedCertificationTemplateGraceDaysRangeRule(80, 3600),
            });
    }
}
