namespace Acme.Workforce.Compliance.Certification.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional certification adjustment must pass. Check returns its problems, or none.</summary>
public interface IRegionalCertificationAdjustmentRule
{
    List<string> Check(RegionalCertificationAdjustment item);
}

public sealed record RegionalCertificationAdjustmentAllowedSegmentRule(List<string> Allowed) : IRegionalCertificationAdjustmentRule
{
    public List<string> Check(RegionalCertificationAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Segment)))
        {
            return new List<string> { $"segment {item.Segment} is expired" };
        }
        return new List<string>();
    }
}

public sealed record RegionalCertificationAdjustmentRequiredCodeRule(string Message) : IRegionalCertificationAdjustmentRule
{
    public List<string> Check(RegionalCertificationAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalCertificationAdjustmentBudgetRangeRule(double Min, double Max) : IRegionalCertificationAdjustmentRule
{
    public List<string> Check(RegionalCertificationAdjustment item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional certification adjustment.</summary>
public sealed record RegionalCertificationAdjustmentValidator(List<IRegionalCertificationAdjustmentRule> Rules)
{
    public List<string> Validate(RegionalCertificationAdjustment item)
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

    public void RequireValid(RegionalCertificationAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalCertificationAdjustment> items)
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

/// <summary>Functions of regional certification adjustment rules.</summary>
public static class RegionalCertificationAdjustmentRules
{
    public static RegionalCertificationAdjustmentValidator DefaultRegionalCertificationAdjustmentValidator()
    {
        return new RegionalCertificationAdjustmentValidator(
            new List<IRegionalCertificationAdjustmentRule>
            {
                new RegionalCertificationAdjustmentAllowedSegmentRule(new List<string> { "fallback", "primary" }),
                new RegionalCertificationAdjustmentRequiredCodeRule(
                    "regional certification adjustment needs this field"),
                new RegionalCertificationAdjustmentBudgetRangeRule(5.1, 2981.3),
            });
    }
}
