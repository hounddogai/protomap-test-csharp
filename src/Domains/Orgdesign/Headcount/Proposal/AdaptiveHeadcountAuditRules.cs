namespace Acme.Workforce.Orgdesign.Headcount.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive headcount audit must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveHeadcountAuditRule
{
    List<string> Check(AdaptiveHeadcountAudit item);
}

public sealed record AdaptiveHeadcountAuditThresholdRangeRule(double Min, double Max) : IAdaptiveHeadcountAuditRule
{
    public List<string> Check(AdaptiveHeadcountAudit item)
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

public sealed record AdaptiveHeadcountAuditRequiredCodeRule(string Message) : IAdaptiveHeadcountAuditRule
{
    public List<string> Check(AdaptiveHeadcountAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveHeadcountAuditRequiredLabelRule(string Message) : IAdaptiveHeadcountAuditRule
{
    public List<string> Check(AdaptiveHeadcountAudit item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveHeadcountAuditExpiresAtRangeRule(int Min, int Max) : IAdaptiveHeadcountAuditRule
{
    public List<string> Check(AdaptiveHeadcountAudit item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveHeadcountAuditAllowedCodeRule(List<string> Allowed) : IAdaptiveHeadcountAuditRule
{
    public List<string> Check(AdaptiveHeadcountAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive headcount audit.</summary>
public sealed record AdaptiveHeadcountAuditValidator(List<IAdaptiveHeadcountAuditRule> Rules)
{
    public List<string> Validate(AdaptiveHeadcountAudit item)
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

    public void RequireValid(AdaptiveHeadcountAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveHeadcountAudit> items)
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

/// <summary>Functions of adaptive headcount audit rules.</summary>
public static class AdaptiveHeadcountAuditRules
{
    public static AdaptiveHeadcountAuditValidator DefaultAdaptiveHeadcountAuditValidator()
    {
        return new AdaptiveHeadcountAuditValidator(
            new List<IAdaptiveHeadcountAuditRule>
            {
                new AdaptiveHeadcountAuditThresholdRangeRule(7.8, 2380.8),
                new AdaptiveHeadcountAuditRequiredCodeRule("adaptive headcount audit needs this field"),
                new AdaptiveHeadcountAuditRequiredLabelRule("adaptive headcount audit needs a value"),
                new AdaptiveHeadcountAuditExpiresAtRangeRule(120, 1000),
                new AdaptiveHeadcountAuditAllowedCodeRule(new List<string> { "global" }),
            });
    }
}
