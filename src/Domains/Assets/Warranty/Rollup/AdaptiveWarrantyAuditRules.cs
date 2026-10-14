namespace Acme.Workforce.Assets.Warranty.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive warranty audit must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveWarrantyAuditRule
{
    List<string> Check(AdaptiveWarrantyAudit item);
}

public sealed record AdaptiveWarrantyAuditRequiredJobCodeRule(string Message) : IAdaptiveWarrantyAuditRule
{
    public List<string> Check(AdaptiveWarrantyAudit item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveWarrantyAuditAllowedCodeRule(List<string> Allowed) : IAdaptiveWarrantyAuditRule
{
    public List<string> Check(AdaptiveWarrantyAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveWarrantyAuditAmountRangeRule(double Min, double Max) : IAdaptiveWarrantyAuditRule
{
    public List<string> Check(AdaptiveWarrantyAudit item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive warranty audit.</summary>
public sealed record AdaptiveWarrantyAuditValidator(List<IAdaptiveWarrantyAuditRule> Rules)
{
    public List<string> Validate(AdaptiveWarrantyAudit item)
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

    public void RequireValid(AdaptiveWarrantyAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveWarrantyAudit> items)
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

/// <summary>Functions of adaptive warranty audit rules.</summary>
public static class AdaptiveWarrantyAuditRules
{
    public static AdaptiveWarrantyAuditValidator DefaultAdaptiveWarrantyAuditValidator()
    {
        return new AdaptiveWarrantyAuditValidator(
            new List<IAdaptiveWarrantyAuditRule>
            {
                new AdaptiveWarrantyAuditRequiredJobCodeRule("adaptive warranty audit needs an entry"),
                new AdaptiveWarrantyAuditAllowedCodeRule(new List<string> { "pending", "global" }),
                new AdaptiveWarrantyAuditAmountRangeRule(0.5, 577.0),
            });
    }
}
