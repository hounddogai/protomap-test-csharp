namespace Acme.Workforce.Compliance.Certification.Slot;

/// <summary>A rule a certification metric must pass. Check returns its problems, or none.</summary>
public interface ICertificationMetricRule
{
    List<string> Check(CertificationMetric item);
}

public sealed record CertificationMetricRequiredTimezoneRule(string Message) : ICertificationMetricRule
{
    public List<string> Check(CertificationMetric item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationMetricRequiredLabelRule(string Message) : ICertificationMetricRule
{
    public List<string> Check(CertificationMetric item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationMetricRequiredOwnerIdRule(string Message) : ICertificationMetricRule
{
    public List<string> Check(CertificationMetric item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationMetricCostRangeRule(double Min, double Max) : ICertificationMetricRule
{
    public List<string> Check(CertificationMetric item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationMetricRequiredCodeRule(string Message) : ICertificationMetricRule
{
    public List<string> Check(CertificationMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification metric.</summary>
public sealed record CertificationMetricValidator(List<ICertificationMetricRule> Rules)
{
    public List<string> Validate(CertificationMetric item)
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

    public void RequireValid(CertificationMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationMetric> items)
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

/// <summary>Functions of certification metric rules.</summary>
public static class CertificationMetricRules
{
    public static CertificationMetricValidator DefaultCertificationMetricValidator()
    {
        return new CertificationMetricValidator(
            new List<ICertificationMetricRule>
            {
                new CertificationMetricRequiredTimezoneRule("certification metric needs this field"),
                new CertificationMetricRequiredLabelRule("certification metric needs this field"),
                new CertificationMetricRequiredOwnerIdRule("certification metric needs an entry"),
                new CertificationMetricCostRangeRule(6.0, 2795.7),
                new CertificationMetricRequiredCodeRule("certification metric needs a value"),
            });
    }
}
