namespace Acme.Workforce.Compliance.Certification.Summary;

/// <summary>A rule a certification summary must pass. Check returns its problems, or none.</summary>
public interface ICertificationSummaryRule
{
    List<string> Check(CertificationSummary item);
}

public sealed record CertificationSummaryRequiredCostCenterRule(string Message) : ICertificationSummaryRule
{
    public List<string> Check(CertificationSummary item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationSummaryMarginRangeRule(double Min, double Max) : ICertificationSummaryRule
{
    public List<string> Check(CertificationSummary item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CertificationSummaryMaximumRangeRule(double Min, double Max) : ICertificationSummaryRule
{
    public List<string> Check(CertificationSummary item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification summary.</summary>
public sealed record CertificationSummaryValidator(List<ICertificationSummaryRule> Rules)
{
    public List<string> Validate(CertificationSummary item)
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

    public void RequireValid(CertificationSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationSummary> items)
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

/// <summary>Functions of certification summary rules.</summary>
public static class CertificationSummaryRules
{
    public static CertificationSummaryValidator DefaultCertificationSummaryValidator()
    {
        return new CertificationSummaryValidator(
            new List<ICertificationSummaryRule>
            {
                new CertificationSummaryRequiredCostCenterRule("certification summary needs an entry"),
                new CertificationSummaryMarginRangeRule(3.4, 4720.3),
                new CertificationSummaryMaximumRangeRule(9.4, 1635.0),
            });
    }
}
