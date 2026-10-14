namespace Acme.Workforce.Compliance.Certification.Budget;

/// <summary>A rule a certification cycle must pass. Check returns its problems, or none.</summary>
public interface ICertificationCycleRule
{
    List<string> Check(CertificationCycle item);
}

public sealed record CertificationCycleExpiresAtRangeRule(int Min, int Max) : ICertificationCycleRule
{
    public List<string> Check(CertificationCycle item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationCycleCodeLengthRule(int MaxLength) : ICertificationCycleRule
{
    public List<string> Check(CertificationCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationCycleLabelLengthRule(int MaxLength) : ICertificationCycleRule
{
    public List<string> Check(CertificationCycle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification cycle.</summary>
public sealed record CertificationCycleValidator(List<ICertificationCycleRule> Rules)
{
    public List<string> Validate(CertificationCycle item)
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

    public void RequireValid(CertificationCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationCycle> items)
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

/// <summary>Functions of certification cycle rules.</summary>
public static class CertificationCycleRules
{
    public static CertificationCycleValidator DefaultCertificationCycleValidator()
    {
        return new CertificationCycleValidator(
            new List<ICertificationCycleRule>
            {
                new CertificationCycleExpiresAtRangeRule(86400, 86400),
                new CertificationCycleCodeLengthRule(255),
                new CertificationCycleLabelLengthRule(86400),
            });
    }
}
