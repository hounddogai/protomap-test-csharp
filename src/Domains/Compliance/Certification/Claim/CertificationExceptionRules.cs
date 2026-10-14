namespace Acme.Workforce.Compliance.Certification.Claim;

/// <summary>A rule a certification exception must pass. Check returns its problems, or none.</summary>
public interface ICertificationExceptionRule
{
    List<string> Check(CertificationException item);
}

public sealed record CertificationExceptionDiscountRangeRule(double Min, double Max) : ICertificationExceptionRule
{
    public List<string> Check(CertificationException item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationExceptionGradeLengthRule(int MaxLength) : ICertificationExceptionRule
{
    public List<string> Check(CertificationException item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationExceptionRequiredLabelRule(string Message) : ICertificationExceptionRule
{
    public List<string> Check(CertificationException item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification exception.</summary>
public sealed record CertificationExceptionValidator(List<ICertificationExceptionRule> Rules)
{
    public List<string> Validate(CertificationException item)
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

    public void RequireValid(CertificationException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationException> items)
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

/// <summary>Functions of certification exception rules.</summary>
public static class CertificationExceptionRules
{
    public static CertificationExceptionValidator DefaultCertificationExceptionValidator()
    {
        return new CertificationExceptionValidator(
            new List<ICertificationExceptionRule>
            {
                new CertificationExceptionDiscountRangeRule(9.3, 2952.4),
                new CertificationExceptionGradeLengthRule(80),
                new CertificationExceptionRequiredLabelRule("certification exception needs a value"),
            });
    }
}
