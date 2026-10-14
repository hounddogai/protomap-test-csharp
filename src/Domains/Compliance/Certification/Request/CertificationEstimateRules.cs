namespace Acme.Workforce.Compliance.Certification.Request;

/// <summary>A rule a certification estimate must pass. Check returns its problems, or none.</summary>
public interface ICertificationEstimateRule
{
    List<string> Check(CertificationEstimate item);
}

public sealed record CertificationEstimateRequiredNameRule(string Message) : ICertificationEstimateRule
{
    public List<string> Check(CertificationEstimate item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationEstimateRequiredTitleRule(string Message) : ICertificationEstimateRule
{
    public List<string> Check(CertificationEstimate item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationEstimateNameLengthRule(int MaxLength) : ICertificationEstimateRule
{
    public List<string> Check(CertificationEstimate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationEstimateActiveGuardRule(int Limit) : ICertificationEstimateRule
{
    public List<string> Check(CertificationEstimate item)
    {
        if (item.Active && item.ExpiresAt > Limit)
        {
            return new List<string> { $"expires at {item.ExpiresAt} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record CertificationEstimateRequiredDepartmentRule(string Message) : ICertificationEstimateRule
{
    public List<string> Check(CertificationEstimate item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification estimate.</summary>
public sealed record CertificationEstimateValidator(List<ICertificationEstimateRule> Rules)
{
    public List<string> Validate(CertificationEstimate item)
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

    public void RequireValid(CertificationEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationEstimate> items)
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

/// <summary>Functions of certification estimate rules.</summary>
public static class CertificationEstimateRules
{
    public static CertificationEstimateValidator DefaultCertificationEstimateValidator()
    {
        return new CertificationEstimateValidator(
            new List<ICertificationEstimateRule>
            {
                new CertificationEstimateRequiredNameRule("certification estimate needs an entry"),
                new CertificationEstimateRequiredTitleRule("certification estimate needs a value"),
                new CertificationEstimateNameLengthRule(3600),
                new CertificationEstimateActiveGuardRule(86400),
                new CertificationEstimateRequiredDepartmentRule("certification estimate needs a value"),
            });
    }
}
