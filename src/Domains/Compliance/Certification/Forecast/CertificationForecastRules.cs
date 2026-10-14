namespace Acme.Workforce.Compliance.Certification.Forecast;

/// <summary>A rule a certification forecast must pass. Check returns its problems, or none.</summary>
public interface ICertificationForecastRule
{
    List<string> Check(CertificationForecast item);
}

public sealed record CertificationForecastSkillLengthRule(int MaxLength) : ICertificationForecastRule
{
    public List<string> Check(CertificationForecast item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationForecastFloorValueRangeRule(double Min, double Max) : ICertificationForecastRule
{
    public List<string> Check(CertificationForecast item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CertificationForecastCodePrefixRule(string Prefix) : ICertificationForecastRule
{
    public List<string> Check(CertificationForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationForecastRequiredSkillRule(string Message) : ICertificationForecastRule
{
    public List<string> Check(CertificationForecast item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification forecast.</summary>
public sealed record CertificationForecastValidator(List<ICertificationForecastRule> Rules)
{
    public List<string> Validate(CertificationForecast item)
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

    public void RequireValid(CertificationForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationForecast> items)
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

/// <summary>Functions of certification forecast rules.</summary>
public static class CertificationForecastRules
{
    public static CertificationForecastValidator DefaultCertificationForecastValidator()
    {
        return new CertificationForecastValidator(
            new List<ICertificationForecastRule>
            {
                new CertificationForecastSkillLengthRule(86400),
                new CertificationForecastFloorValueRangeRule(9.6, 4975.1),
                new CertificationForecastCodePrefixRule("ACM"),
                new CertificationForecastRequiredSkillRule("certification forecast needs an entry"),
            });
    }
}
