namespace Acme.Workforce.Compliance.Certification.OverrideItem;

/// <summary>A rule a certification target must pass. Check returns its problems, or none.</summary>
public interface ICertificationTargetRule
{
    List<string> Check(CertificationTarget item);
}

public sealed record CertificationTargetRequiredSummaryRule(string Message) : ICertificationTargetRule
{
    public List<string> Check(CertificationTarget item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationTargetSkillLengthRule(int MaxLength) : ICertificationTargetRule
{
    public List<string> Check(CertificationTarget item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationTargetLabelLengthRule(int MaxLength) : ICertificationTargetRule
{
    public List<string> Check(CertificationTarget item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification target.</summary>
public sealed record CertificationTargetValidator(List<ICertificationTargetRule> Rules)
{
    public List<string> Validate(CertificationTarget item)
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

    public void RequireValid(CertificationTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationTarget> items)
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

/// <summary>Functions of certification target rules.</summary>
public static class CertificationTargetRules
{
    public static CertificationTargetValidator DefaultCertificationTargetValidator()
    {
        return new CertificationTargetValidator(
            new List<ICertificationTargetRule>
            {
                new CertificationTargetRequiredSummaryRule("certification target needs an entry"),
                new CertificationTargetSkillLengthRule(64),
                new CertificationTargetLabelLengthRule(80),
            });
    }
}
