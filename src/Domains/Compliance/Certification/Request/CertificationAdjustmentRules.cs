namespace Acme.Workforce.Compliance.Certification.Request;

/// <summary>A rule a certification adjustment must pass. Check returns its problems, or none.</summary>
public interface ICertificationAdjustmentRule
{
    List<string> Check(CertificationAdjustment item);
}

public sealed record CertificationAdjustmentRequiredLabelRule(string Message) : ICertificationAdjustmentRule
{
    public List<string> Check(CertificationAdjustment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationAdjustmentRequiredCodeRule(string Message) : ICertificationAdjustmentRule
{
    public List<string> Check(CertificationAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationAdjustmentThresholdRangeRule(double Min, double Max) : ICertificationAdjustmentRule
{
    public List<string> Check(CertificationAdjustment item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a certification adjustment.</summary>
public sealed record CertificationAdjustmentValidator(List<ICertificationAdjustmentRule> Rules)
{
    public List<string> Validate(CertificationAdjustment item)
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

    public void RequireValid(CertificationAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationAdjustment> items)
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

/// <summary>Functions of certification adjustment rules.</summary>
public static class CertificationAdjustmentRules
{
    public static CertificationAdjustmentValidator DefaultCertificationAdjustmentValidator()
    {
        return new CertificationAdjustmentValidator(
            new List<ICertificationAdjustmentRule>
            {
                new CertificationAdjustmentRequiredLabelRule("certification adjustment needs this field"),
                new CertificationAdjustmentRequiredCodeRule("certification adjustment needs this field"),
                new CertificationAdjustmentThresholdRangeRule(7.3, 2680.7),
            });
    }
}
