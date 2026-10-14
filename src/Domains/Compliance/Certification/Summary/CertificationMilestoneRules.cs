namespace Acme.Workforce.Compliance.Certification.Summary;

/// <summary>A rule a certification milestone must pass. Check returns its problems, or none.</summary>
public interface ICertificationMilestoneRule
{
    List<string> Check(CertificationMilestone item);
}

public sealed record CertificationMilestoneSalaryBandLengthRule(int MaxLength) : ICertificationMilestoneRule
{
    public List<string> Check(CertificationMilestone item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationMilestoneEndsAtRangeRule(int Min, int Max) : ICertificationMilestoneRule
{
    public List<string> Check(CertificationMilestone item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CertificationMilestoneRequiredSourceRule(string Message) : ICertificationMilestoneRule
{
    public List<string> Check(CertificationMilestone item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationMilestoneRateRangeRule(double Min, double Max) : ICertificationMilestoneRule
{
    public List<string> Check(CertificationMilestone item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationMilestoneRequiredSalaryBandRule(string Message) : ICertificationMilestoneRule
{
    public List<string> Check(CertificationMilestone item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationMilestoneCodePrefixRule(string Prefix) : ICertificationMilestoneRule
{
    public List<string> Check(CertificationMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification milestone.</summary>
public sealed record CertificationMilestoneValidator(List<ICertificationMilestoneRule> Rules)
{
    public List<string> Validate(CertificationMilestone item)
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

    public void RequireValid(CertificationMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationMilestone> items)
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

/// <summary>Functions of certification milestone rules.</summary>
public static class CertificationMilestoneRules
{
    public static CertificationMilestoneValidator DefaultCertificationMilestoneValidator()
    {
        return new CertificationMilestoneValidator(
            new List<ICertificationMilestoneRule>
            {
                new CertificationMilestoneSalaryBandLengthRule(86400),
                new CertificationMilestoneEndsAtRangeRule(64, 120),
                new CertificationMilestoneRequiredSourceRule("certification milestone needs an entry"),
                new CertificationMilestoneRateRangeRule(7.3, 1768.0),
                new CertificationMilestoneRequiredSalaryBandRule("certification milestone needs this field"),
                new CertificationMilestoneCodePrefixRule("ACM"),
            });
    }
}
